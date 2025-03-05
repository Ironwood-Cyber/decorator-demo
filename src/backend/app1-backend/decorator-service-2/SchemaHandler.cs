using System;
using System.IO;
using System.Reflection;
using System.Threading.Tasks;
using MassTransit;
using MongoDB.Driver;
using Shared;

namespace DecoratorService2;

/// <summary>
/// Handles reading the various JsonForms schema files
/// </summary>
public class SchemaHandler(IMongoDatabase db, IBus bus) : ISchemaHandler
{
    /// <inheritdoc/>
    public async Task<string> GetSchemaAsStringAsync() => await GetFileAsStringAsync(Path.Combine(GetPath, Constants.Files.Schema));

    /// <inheritdoc/>
    public Task<string> GetDataAsStringAsync() => throw new NotImplementedException("Decorator service does not have data");

    /// <inheritdoc/>
    public Task<string> GetUiSchemaAsStringAsync() => throw new NotImplementedException("Decorator service does not have a UI schema");

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
