namespace DriverSolution.models;
public class Activity
{
    public string Id { get; set; }
    public string EventId { get; set; }
    public string Name { get; set; }
    public string Description { get; set; }
    public DateTime StartDateTime { get; set; }
    public DateTime EndDateTime { get; set; }
    public string Location { get; set; }
    public string OnlineLink { get; set; }
    public List<string> Speakers { get; set; } = new List<string>();
    public ActivityStatus Status { get; set; } = ActivityStatus.Planifie;
}

public enum ActivityStatus
{
    Planifie,    // Activité planifiée mais non confirmée
    Confirme,    // Confirmée et prête à démarrer
    Annule,      // Annulée
    Termine      // Terminée
}