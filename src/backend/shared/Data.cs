using MongoDB.Bson;
using Newtonsoft.Json;

namespace Shared;

public class Data
{
    [JsonIgnore]
    public ObjectId Id { get; set; }

    public string JsonData { get; set; } = string.Empty;
}
