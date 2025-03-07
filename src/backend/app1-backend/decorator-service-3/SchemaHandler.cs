using System;
using System.ComponentModel.Composition;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using MongoDB.Bson;
using MongoDB.Driver;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Shared;

namespace DecoratorService3;

public class BaseData
{
    [JsonIgnore]
    public ObjectId Id { get; set; }

    public string JsonData { get; set; } = string.Empty;
}

/// <summary>
/// Handles reading the various JsonForms schema files as well as the event handling
/// </summary>
[Export(typeof(ISchemaHandler))] // Export the interface to MEF
[Export(typeof(IEventHandler))] // Export the interface to MEF
[ExportMetadata(nameof(IHandlerData.ExecutionOrder), EventExecutionOrder.OverrideBaseService)] // Add metadata to the interface for MEF
[ExportMetadata(nameof(IHandlerData.ServiceType), ServiceType.Decorator)] // Add metadata to the interface for MEF
public class SchemaHandler : ISchemaHandler, IEventHandler
{
    private readonly IMongoCollection<BaseData> _database;

    public SchemaHandler()
    {
        string mongoUri = "mongodb://admin:admin@localhost:27017/test";
        _database = new MongoClient(mongoUri).GetDatabase("test").GetCollection<BaseData>(nameof(BaseData));
    }

    /// <inheritdoc/>
    public async Task<string> GetUiSchemaAsStringAsync() => await GetFileAsStringAsync(Path.Combine(GetPath, Constants.Files.UiSchema));

    /// <inheritdoc/>
    public Task<string> GetDataAsStringAsync() => throw new NotImplementedException("Decorator service does not have a data file");

    /// <inheritdoc/>
    public Task<string> GetSchemaAsStringAsync() => throw new NotImplementedException("Decorator service does not have a schema file");

    /// <inheritdoc/>
    public Task<string> GetEventHandlerAsync() => throw new NotImplementedException("Decorator service does not have a client side event handler");

    /// <inheritdoc/>
    public string? HandleSchemaEvent(object data)
    {
        JObject jsonData = JObject.Parse(data?.ToString() ?? string.Empty);
        if (jsonData is null || !jsonData.HasValues)
        {
            return null;
        }

        if (!jsonData!.TryGetValue("firstNumber", out JToken? firstNumber))
        {
            return null;
        }
        jsonData["result"] = int.Parse(firstNumber.ToString()) * 100;

        // Persist the data
        var documents = _database.Find(new BsonDocument()).Project<BsonDocument>(Builders<BaseData>.Projection.Include("_id")).ToList();
        var document = documents.FirstOrDefault();
        if (document is not null)
        {
            _database.ReplaceOne(document, new BaseData { Id = document["_id"].AsObjectId, JsonData = jsonData.ToString() });
        }
        else
        {
            _database.InsertOne(new BaseData { JsonData = jsonData.ToString() });
        }

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
