using System.IO;
using System.Net.Http;
using Appwrite;
using Appwrite.Services;
using Appwrite.Models;
using DriverSolution.models;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using File = System.IO.File;

public class AppwriteService
{
    private const string DatabaseId = "68095ddb0003973d5a36";
    private const string CollectionUserId = "68095de4002182d02de0";
    private const string BucketId = "6809603d002a97bba696"; // Make sure this is correct
    private const string ProjectId = "6808ec3d000402274753";
    private const string CollectionMembersId = "680d53fc000503cc682d";
    private const string CollectionActivitesId = "6810daac00333e0910c6";
    private const string CollectionSpeakersId = "6810f40800020d21821c";

    private readonly Client _client;
    private readonly Account _account;
    private readonly Databases _databases;
    private readonly Users _users;
    private readonly Storage _storage;
    private static AppwriteService _instance;

    public AppwriteService()
    {
        _client = new Client()
            .SetEndpoint("https://cloud.appwrite.io/v1")
            .SetProject(ProjectId);

        _account = new Account(_client);
        _databases = new Databases(_client);
        _users = new Users(_client);
        _storage = new Storage(_client);
    }

    public static AppwriteService Instance
    {
        get
        {
            if (_instance == null)
                _instance = new AppwriteService();
            return _instance;
        }
    }

    /// <summary>
    /// Crée un nouvel utilisateur
    /// </summary>
    public async Task<User> SignUpAsync(string email, string password, string name)
    {
        return await _account.Create(
            userId: ID.Unique(),
            email: email,
            password: password,
            name: name
        );
    }

    public void RestoreSession(string sessionId)
    {
        _client.SetSession(sessionId);
    }

    /// <summary>
    /// Connecte un utilisateur par email/mot de passe
    /// </summary>
    public async Task<Session> LoginAsync(string email, string password)
    {
        var session = await _account.CreateEmailPasswordSession(
            email: email,
            password: password
        );

        _client.SetSession(session.Id);
        return session;
    }

    /// <summary>
    /// Récupère l'utilisateur courant
    /// </summary>
    public async Task<User> GetCurrentUserAsync()
    {
        return await _account.Get();
    }

    /// <summary>
    /// Déconnecte l'utilisateur
    /// </summary>
    public async Task LogoutAsync()
    {
        await _account.DeleteSession("current");
    }

    /// <summary>
    /// Ajoute un document dans la collection Evenement
    /// </summary>
    /// 
    public async Task<Document> CreateEventAsync(AppEvent eventData)
    {
        // Validation
        if (string.IsNullOrEmpty(eventData.IdUser))
            throw new ArgumentException("User ID is required");

        if (string.IsNullOrEmpty(eventData.Name))
            throw new ArgumentException("Event name is required");

        // Conversion des enums
        var locationType = eventData.EventLocationType switch
        {
            AppEvent.LocationType.Physique => "Physique",
            AppEvent.LocationType.Online => "Online",
            _ => "Hybride"
        };

        var recurrence = eventData.Recurrence == AppEvent.RecurrenceType.Unique
            ? "unique"
            : "a_seances";

        // Construction des données
        var data = new Dictionary<string, object>
        {
            { "idUser", eventData.IdUser },
            { "name", eventData.Name },
            { "type", eventData.Type ?? "general" },
            { "description", eventData.Description ?? "" },
            { "imageUrl", eventData.ImageUrl ?? "" },
            { "locationType", locationType },
            { "recurrence", recurrence },
            { "startDateTime", eventData.StartDateTime.ToString("o") },
            { "status", "planned" }
        };

        // Champs conditionnels
        if (!string.IsNullOrEmpty(eventData.PlaceName))
            data.Add("placeName", eventData.PlaceName);

        if (!string.IsNullOrEmpty(eventData.Address))
            data.Add("address", eventData.Address);

        if (!string.IsNullOrEmpty(eventData.Link))
            data.Add("link", eventData.Link);

        if (eventData.EndDateTime.HasValue)
            data.Add("endDateTime", eventData.EndDateTime.Value.ToString("o"));

        return await _databases.CreateDocument(
            databaseId: DatabaseId,
            collectionId: CollectionUserId,
            documentId: eventData.Id,
            data: data
        );
    }

    public async Task<string> UploadEventImageAsync(string filePath)
    {
        if (string.IsNullOrWhiteSpace(filePath) || !File.Exists(filePath))
            throw new FileNotFoundException("Fichier d'image introuvable.", filePath);

        try
        {
            // Ensure we have a valid session
            var user = await GetCurrentUserAsync();
            if (user == null)
            {
                Console.WriteLine("User not register");
                throw new Exception("User not authenticated");
            }

            // Create the file
            var result = await _storage.CreateFile(
                bucketId: BucketId,
                fileId: ID.Unique(),
                file: InputFile.FromPath(filePath),
                permissions: new List<string> { Permission.Read(Role.Any()) }
            );

            return GetFilePreviewUrl(result.Id);
        }
        catch (AppwriteException e)
        {
            Console.WriteLine($"Appwrite Error: {e.Message}");
            throw new Exception("Failed to upload image: " + e.Message);
        }
        catch (Exception e)
        {
            Console.WriteLine($"General Error: {e}");
            throw;
        }
    }


    private string GetFilePreviewUrl(string fileId)
    {
        // Construction manuelle de l'URL
        return $"https://cloud.appwrite.io/v1/storage/buckets/{BucketId}/files/{fileId}/view?project={ProjectId}";
    }

    public async Task<List<AppEvent>> GetUserEventsAsync(string userId)
    {
        try
        {
            var response = await _databases.ListDocuments(
                databaseId: DatabaseId,
                collectionId: CollectionUserId,
                queries: new List<string>
                {
                    Query.Equal("idUser", userId),
                    Query.OrderDesc("$createdAt")
                }
            );

            if (response?.Documents == null)
                return new List<AppEvent>();

            return response.Documents
                .Where(doc => doc != null)
                .Select(doc =>
                {
                    var data = doc.Data ?? new Dictionary<string, object>();

                    try
                    {
                        return new AppEvent
                        {
                            Id = doc.Id,
                            IdUser = data.GetValueOrDefault("idUser")?.ToString(),
                            Name = data.GetValueOrDefault("name")?.ToString(),
                            Type = data.GetValueOrDefault("type")?.ToString(),
                            Description = data.GetValueOrDefault("description")?.ToString(),
                            ImageUrl = data.GetValueOrDefault("imageUrl")?.ToString(),
                            ConnectionInstructions = data.GetValueOrDefault("connectionInstructions")?.ToString(),
                            EventLocationType = ParseLocationType(data.GetValueOrDefault("locationType")?.ToString()),
                            PlaceName = data.GetValueOrDefault("placeName")?.ToString(),
                            Address = data.GetValueOrDefault("address")?.ToString(),
                            Link = data.GetValueOrDefault("link")?.ToString(),
                            Recurrence = ParseRecurrenceType(data.GetValueOrDefault("recurrence")?.ToString()),
                            StartDateTime = DateTime.TryParse(data.GetValueOrDefault("startDateTime")?.ToString(),
                                out var startDate)
                                ? startDate
                                : DateTime.MinValue,
                            EndDateTime = DateTime.TryParse(data.GetValueOrDefault("endDateTime")?.ToString(),
                                out var endDate)
                                ? endDate
                                : (DateTime?)null
                        };
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"Error parsing document {doc.Id}: {ex.Message}");
                        return null;
                    }
                })
                .Where(evt => evt != null)
                .ToList();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error in GetUserEventsAsync: {ex.Message}");
            return new List<AppEvent>();
        }
    }

    private AppEvent.LocationType ParseLocationType(string locationType)
    {
        return locationType?.ToLower() switch
        {
            "Physique" => AppEvent.LocationType.Physique,
            "Online" => AppEvent.LocationType.Online,
            _ => AppEvent.LocationType.Hybride
        };
    }

    private AppEvent.RecurrenceType ParseRecurrenceType(string recurrence)
    {
        return recurrence?.ToLower() switch
        {
            "unique" => AppEvent.RecurrenceType.Unique,
            _ => AppEvent.RecurrenceType.A_Seances
        };
    }


    public async Task<List<Member>> GetEventMembersAsync(string eventId)
    {
        try
        {
            var response = await _databases.ListDocuments(
                databaseId: DatabaseId,
                collectionId: CollectionMembersId,
                queries: new List<string>
                {
                    Query.Equal("idEvent", eventId),
                    Query.OrderAsc("Email")
                }
            );


            return response.Documents
                .Select(doc =>
                {
                    var data = doc.Data ?? new Dictionary<string, object>();
                    return new Member()
                    {
                        Email = data.GetValueOrDefault("Email")?.ToString() ?? "",
                    };
                }).ToList();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Erreur lors de la récupération des membres: {ex.Message}");
            return new List<Member>();
        }
    }

    public async Task<long> GetEventMemberCount(string eventId)
    {
        var response = await _databases.ListDocuments(
            databaseId: DatabaseId,
            collectionId: CollectionMembersId,
            queries: new List<string> { Query.Equal("idEvent", eventId) }
        );
        return response.Documents.Count;
    }

    public async Task<GenderStats> GetGenderDistribution(string eventId)
    {
        // Implémentation factice - à adapter avec vos données réelles
        return new GenderStats
        {
            MaleCount = 45,
            FemaleCount = 55
        };
    }

    public async Task<AttendanceData> GetDailyAttendance(string eventId)
    {
        // Implémentation factice - à adapter
        return new AttendanceData
        {
            Dates = new List<string> { "01/06", "02/06", "03/06", "04/06", "05/06" },
            Values = new List<int> { 10, 25, 40, 60, 85 }
        };
    }


    public async Task<string> GetShareableLink(string eventId)
    {
        try
        {
            using var httpClient = new HttpClient();
            var response =
                await httpClient.GetAsync($"https://680e56c47fb9f14f2b23.fra.appwrite.run/?eventId={eventId}");

            if (!response.IsSuccessStatusCode)
                throw new Exception("Failed to get shareable link");

            var content = await response.Content.ReadAsStringAsync();
            var result = JsonConvert.DeserializeObject<ShareableLinkResponse>(content);

            if (result == null || !result.Success)
                throw new Exception("Invalid response from server");

            return result.ShareableLink;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error getting shareable link: {ex.Message}");
            throw;
        }
    }

    public async Task<Document> CreateActivityAsync(Activity activity)
    {
        // Validation
        if (string.IsNullOrEmpty(activity.EventId))
            throw new ArgumentException("Event ID is required");

        if (string.IsNullOrEmpty(activity.Name))
            throw new ArgumentException("Activity name is required");

        // Construction des données
        var data = new Dictionary<string, object>
        {
            { "eventId", activity.EventId },
            { "name", activity.Name },
            { "description", activity.Description ?? "" },
            { "startDateTime", activity.StartDateTime.ToString("o") },
            { "endDateTime", activity.EndDateTime.ToString("o") },
            { "location", activity.Location ?? "" },
            { "onlineLink", activity.OnlineLink ?? "" },
            { "speakers", activity.Speakers ?? new List<string>() },
            { "status", activity.Status.ToString().ToLower() } // "planifie", "confirme", etc.
        };

        Console.WriteLine("donnee ! " + data);
        return await _databases.CreateDocument(
            databaseId: DatabaseId,
            collectionId: CollectionActivitesId,
            documentId: ID.Unique(),
            data: data
        );
    }

    public async Task<List<Activity>> GetEventActivitiesAsync(string eventId)
    {
        try
        {
            var response = await _databases.ListDocuments(
                databaseId: DatabaseId,
                collectionId: CollectionActivitesId,
                queries: new List<string>
                {
                    Query.Equal("eventId", eventId),
                    Query.OrderAsc("startDateTime") // Tri chronologique
                }
            );

            return response.Documents.Select(doc =>
            {
                var data = doc.Data ?? new Dictionary<string, object>();
                return new Activity
                {
                    Id = doc.Id,
                    EventId = data.GetValueOrDefault("eventId")?.ToString(),
                    Name = data.GetValueOrDefault("name")?.ToString(),
                    Description = data.GetValueOrDefault("description")?.ToString(),
                    StartDateTime = DateTime.Parse(data.GetValueOrDefault("startDateTime")?.ToString()),
                    EndDateTime = DateTime.Parse(data.GetValueOrDefault("endDateTime")?.ToString()),
                    Location = data.GetValueOrDefault("location")?.ToString(),
                    OnlineLink = data.GetValueOrDefault("onlineLink")?.ToString(),
                    Speakers = ((JArray)data.GetValueOrDefault("speakers"))?.ToObject<List<string>>() ??
                               new List<string>(),
                    Status = ParseActivityStatus(data.GetValueOrDefault("status")?.ToString())
                };
            }).ToList();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error fetching activities: {ex.Message}");
            return new List<Activity>();
        }
    }

    public async Task<int> GetActivitiesCount(string eventId)
    {
        try
        {
            var response = await _databases.ListDocuments(
                databaseId: DatabaseId,
                collectionId: CollectionActivitesId,
                queries: new List<string>
                {
                    Query.Equal("eventId", eventId),
                }
            );

            return response.Documents.Count;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error fetching activities: {ex.Message}");
            return 0;
        }
    }

    private ActivityStatus ParseActivityStatus(string status)
    {
        return status?.ToLower() switch
        {
            "confirme" => ActivityStatus.Confirme,
            "annule" => ActivityStatus.Annule,
            "termine" => ActivityStatus.Termine,
            _ => ActivityStatus.Planifie // Valeur par défaut
        };
    }

    public async Task UpdateActivityStatusAsync(string activityId, ActivityStatus newStatus)
    {
        await _databases.UpdateDocument(
            databaseId: DatabaseId,
            collectionId: CollectionActivitesId,
            documentId: activityId,
            data: new Dictionary<string, object>
            {
                { "status", newStatus.ToString().ToLower() }
            }
        );
    }

    public async Task<Speaker> CreateSpeakerAsync(Speaker speaker, string eventId)
    {
        var data = new Dictionary<string, object>
        {
            { "eventId", eventId },
            { "name", speaker.Name },
            { "mail", speaker.Mail }
        };

        var response = await _databases.CreateDocument(
            databaseId: DatabaseId,
            collectionId: CollectionSpeakersId,
            documentId: ID.Unique(),
            data: data
        );

        speaker.Id = response.Id;
        return speaker;
    }
    public async Task<List<Speaker>> GetAvailableSpeakersAsync(string eventId)
    {
        // Implémentez cette méthode selon votre structure de données
        // Exemple basique :
        var response = await _databases.ListDocuments(
            databaseId: DatabaseId,
            collectionId: CollectionSpeakersId,
            queries: new List<string>
            {
                Query.Equal("eventId", eventId),
                Query.OrderAsc("name")
            }
        );

        return response.Documents.Select(doc =>
        {
            var data = doc.Data ?? new Dictionary<string, object>();
            return new Speaker
            {
                Id = doc.Id,
                Name = data.GetValueOrDefault("name")?.ToString() ?? "Inconnu",
                Mail = data.GetValueOrDefault("mail")?.ToString() ?? "Inconnu"
            };
        }).ToList();
    }
    
    public async Task<MemberGrowthData> GetMemberGrowthOverTime(string eventId)
    {
        try
        {
            // Récupérer tous les membres de l'événement
            var members = await _databases.ListDocuments(
                databaseId: DatabaseId,
                collectionId: CollectionMembersId,
                queries: new List<string> { Query.Equal("idEvent", eventId) }
            );

            // Grouper par date d'inscription (vous devrez ajouter un champ createdAt à votre collection members)
            var groupedData = members.Documents
                .GroupBy(m => DateTime.Parse(m.Data["$createdAt"].ToString()).Date)
                .OrderBy(g => g.Key)
                .Select(g => new
                {
                    Date = g.Key,
                    Count = g.Count()
                })
                .ToList();

            // Calculer le total cumulé
            int runningTotal = 0;
            var result = new MemberGrowthData();
        
            foreach (var item in groupedData)
            {
                runningTotal += item.Count;
                result.Dates.Add(item.Date.ToString("dd/MM"));
                result.Counts.Add(runningTotal);
            }

            return result;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error in GetMemberGrowthOverTime: {ex.Message}");
            return new MemberGrowthData();
        }
    }
}

public static class DictionaryExtensions
{
    public static object GetValueOrDefault(this Dictionary<string, object> dict, string key)
    {
        return dict != null && dict.TryGetValue(key, out var value) ? value : null;
    }
}


public class MemberGrowthData
{
    public List<string> Dates { get; set; } = new List<string>();
    public List<int> Counts { get; set; } = new List<int>();
}