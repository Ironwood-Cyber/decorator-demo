using MongoDB.Bson;
using Newtonsoft.Json;

namespace Shared;

/// <summary>
/// Data class to store JSON data in the database
/// </summary>
public class Data
{
    /// <summary>
    /// Id of the data
    /// </summary>
    [JsonIgnore]
    public ObjectId Id { get; set; }

    /// <summary>
    /// JSON data serialized as string
    /// </summary>
    public string JsonData { get; set; } = string.Empty;
}
