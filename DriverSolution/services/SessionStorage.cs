namespace DriverSolution.services;

using System.IO;
using System.Text.Json;

public static class SessionStorage
{
    private static readonly string FilePath = "session.json";

    public static void SaveSession(string sessionId)
    {
        var json = JsonSerializer.Serialize(new { SessionId = sessionId });
        File.WriteAllText(FilePath, json);
    }

    public static string LoadSession()
    {
        if (!File.Exists(FilePath)) return null;
        var json = File.ReadAllText(FilePath);
        var obj = JsonSerializer.Deserialize<SessionData>(json);
        return obj?.SessionId;
    }

    private class SessionData
    {
        public string SessionId { get; set; }
    }
}