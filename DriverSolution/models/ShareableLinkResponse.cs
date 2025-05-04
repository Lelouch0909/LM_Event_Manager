using Newtonsoft.Json;

namespace DriverSolution.models;

 class ShareableLinkResponse
{
    [JsonProperty("success")]
    public bool Success { get; set; }
    
    [JsonProperty("eventId")]
    public string EventId { get; set; }
    
    [JsonProperty("shareableLink")]
    public string ShareableLink { get; set; }
}
