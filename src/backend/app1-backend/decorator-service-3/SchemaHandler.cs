using System;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using MassTransit;
using MongoDB.Bson;
using MongoDB.Driver;
using Newtonsoft.Json.Linq;
using Shared;

namespace DecoratorService3;

/// <summary>
/// Handles reading the various JsonForms schema files as well as the event handling
/// </summary>
public class SchemaHandler(IMongoDatabase db, IBus bus) : ISchemaHandler, IEventHandler
{
    private readonly IMongoCollection<Data> _db = db.GetCollection<Data>(nameof(Data));

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
        var documents = _db.Find(new BsonDocument()).Project<BsonDocument>(Builders<Data>.Projection.Include("_id")).ToList();
        var document = documents.FirstOrDefault();
        if (document is not null)
        {
            _db.ReplaceOne(document, new Data { Id = document["_id"].AsObjectId, JsonData = jsonData.ToString() });
        }
        else
        {
            _db.InsertOne(new Data { JsonData = jsonData.ToString() });
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
