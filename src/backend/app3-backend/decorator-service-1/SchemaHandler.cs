using System.ComponentModel.Composition;
using System.IO;
using System.Reflection;
using System.Threading.Tasks;
using Shared;

namespace DecoratorService1;

/// <summary>
/// Handles reading the various JsonForms schema files
/// </summary>
[Export(typeof(ISchemaHandler))]
[ExportMetadata(nameof(IHandlerData.ExecutionOrder), EventExecutionOrder.None)]
[ExportMetadata(nameof(IHandlerData.ServiceType), ServiceType.Decorator)]
public class SchemaHandler : ISchemaHandler
{
    /// <inheritdoc/>
    public async Task<string> GetDataAsStringAsync() => await GetFileAsStringAsync(Path.Combine(GetPath, Constants.Files.Data));

    /// <inheritdoc/>
    public async Task<string> GetSchemaAsStringAsync() => await GetFileAsStringAsync(Path.Combine(GetPath, Constants.Files.Schema));

    /// <inheritdoc/>
    public async Task<string> GetUiSchemaAsStringAsync() => await GetFileAsStringAsync(Path.Combine(GetPath, Constants.Files.UiSchema));

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
