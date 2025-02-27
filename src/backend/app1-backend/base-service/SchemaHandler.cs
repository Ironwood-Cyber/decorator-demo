using System;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using MassTransit;
using MongoDB.Bson;
using MongoDB.Driver;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Shared;

namespace BaseService;

public class Data
{
    [JsonIgnore]
    public ObjectId Id { get; set; }

    public string JsonData { get; set; } = string.Empty;
}

public class Message
{
    public required string JsonData { get; set; }
}

/// <summary>
/// Handles reading the various JsonForms schema files as well as the event handling
/// </summary>
public class SchemaHandler(IMongoDatabase db, IBus bus) : ISchemaHandler, IEventHandler
{
    private readonly IMongoCollection<Data> _collection = db.GetCollection<Data>(nameof(Data));
    private readonly IBus _bus = bus;

    /// <inheritdoc/>
    public async Task<string> GetDataAsStringAsync()
    {
        var documents = await _collection.Find(_ => true).Project<Data>(Builders<Data>.Projection.Exclude("_id")).ToListAsync();
        var data = documents.FirstOrDefault();
        if (data is not null)
        {
            return data.JsonData;
        }
        return await GetFileAsStringAsync(Path.Combine(GetPath, Constants.Files.Data));
    }

    /// <inheritdoc/>
    public async Task<string> GetSchemaAsStringAsync() => await GetFileAsStringAsync(Path.Combine(GetPath, Constants.Files.Schema));

    /// <inheritdoc/>
    public async Task<string> GetUiSchemaAsStringAsync() => await GetFileAsStringAsync(Path.Combine(GetPath, Constants.Files.UiSchema));

    /// <inheritdoc/>
    public Task<string> GetEventHandlerAsync() => throw new NotImplementedException("Base service does not have a client side event handler");

    /// <inheritdoc/>
    public string? HandleSchemaEvent(object data)
    {
        // Process the data
        JObject jsonData = JObject.Parse(data?.ToString() ?? string.Empty);
        if (jsonData is null || !jsonData.HasValues)
        {
            return null;
        }

        if (!jsonData!.TryGetValue("firstNumber", out JToken? firstNumber))
        {
            return null;
        }
        jsonData["result"] = int.Parse(firstNumber.ToString()) * 5;

        // Persist the data
        var documents = _collection.Find(new BsonDocument()).Project<BsonDocument>(Builders<Data>.Projection.Include("_id")).ToList();
        var document = documents.FirstOrDefault();
        if (document is not null)
        {
            _collection.ReplaceOne(document, new Data { Id = document["_id"].AsObjectId, JsonData = jsonData.ToString() });
        }
        else
        {
            _collection.InsertOne(new Data { JsonData = jsonData.ToString() });
        }

        // Publish the event
        _bus.Publish(new Message { JsonData = jsonData.ToString() });

        // Return the modified data
        return jsonData.ToString();
    }

    /// <summary>
    /// Gets the path of the executing assembly
    /// </summary>
    private static string GetPath => Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location)!;

    /// <summary>
    /// Reads a file and returns it as a string
    /// </summary>
    /// <param name="filePath">The file path</param>
    /// <returns>The content of the file in string form</returns>
    private static async Task<string> GetFileAsStringAsync(string filePath) => await File.ReadAllTextAsync(Path.GetFullPath(filePath));
}
