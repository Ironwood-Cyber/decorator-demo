using System;
using System.IO;
using System.Reflection;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;
using Shared;

namespace BaseService;

/// <summary>
/// Handles reading the various JsonForms schema files as well as the event handling
/// </summary>
public class SchemaHandler : ISchemaHandler, IEventHandler
{
    /// <inheritdoc/>
    public async Task<string> GetDataAsStringAsync() => await GetFileAsStringAsync(Path.Combine(GetPath, Constants.Files.Data));

    /// <inheritdoc/>
    public async Task<string> GetSchemaAsStringAsync() => await GetFileAsStringAsync(Path.Combine(GetPath, Constants.Files.Schema));

    /// <inheritdoc/>
    public async Task<string> GetUiSchemaAsStringAsync() => await GetFileAsStringAsync(Path.Combine(GetPath, Constants.Files.UiSchema));

    /// <inheritdoc/>
    public Task<string> GetEventHandlerAsync() => throw new NotImplementedException("Base service does not have a client side event handler");

    /// <inheritdoc/>
    public string? HandleSchemaEvent(object data)
    {
        // Parse the data and multiply the first number field by 5
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
