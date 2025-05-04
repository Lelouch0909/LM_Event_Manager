using Appwrite;

namespace DriverSolution.models;

public class AppEvent
{
    public string Id { get; set; } = ID.Unique();
    public string IdUser { get; set; }
    public string Name { get; set; }
    public string Type { get; set; }
    public string Description { get; set; }
    public string ImageUrl { get; set; }
    
    public string ConnectionInstructions { get; set; } 

    public enum LocationType { Hybride, Physique, Online }
    public LocationType EventLocationType { get; set; } = LocationType.Hybride;
    
    public string PlaceName { get; set; }
    public string Address { get; set; }
    public string Link { get; set; }
    
    public enum RecurrenceType { Unique, A_Seances }
    public RecurrenceType Recurrence { get; set; } = RecurrenceType.A_Seances;
    
    public DateTime StartDateTime { get; set; }
    public DateTime? EndDateTime { get; set; }
    
    
}