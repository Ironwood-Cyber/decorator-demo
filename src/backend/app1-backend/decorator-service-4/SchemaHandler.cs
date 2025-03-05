using System;
using System.IO;
using System.Reflection;
using System.Threading.Tasks;
using MassTransit;
using MongoDB.Driver;
using Shared;

namespace DecoratorService4;

/// <summary>
/// Handles reading the various JsonForms schema files as well as the event handling
/// </summary>
public class SchemaHandler(IMongoDatabase db, IBus bus) : ISchemaHandler, IEventHandler
{
    /// <inheritdoc/>
    public async Task<string> GetUiSchemaAsStringAsync() => await GetFileAsStringAsync(Path.Combine(GetPath, Constants.Files.UiSchema));

    /// <inheritdoc/>
    public Task<string> GetDataAsStringAsync() => throw new NotImplementedException("Decorator service does not have a data file");

    /// <inheritdoc/>
    public Task<string> GetSchemaAsStringAsync() => throw new NotImplementedException("Decorator service does not have a schema file");

    /// <inheritdoc/>
    public async Task<string> GetEventHandlerAsync() => await GetFileAsStringAsync(Path.Combine(GetPath, Constants.Files.EventHandler));

    /// <inheritdoc/>
    public string? HandleSchemaEvent(object data) => throw new NotImplementedException("Decorator service does not have a server side event handler");

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
