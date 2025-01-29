namespace Shared;

/// <summary>
/// Interface for handling the various JsonForms schema files
/// </summary>
public interface ISchemaHandler
{
    /// <summary>
    /// Reads the data file and returns it as a string
    /// </summary>
    /// <returns>The data file as a string</returns>
    public Task<string> GetDataAsStringAsync();

    /// <summary>
    /// Reads the schema file and returns it as a string
    /// </summary>
    /// <returns>The schema file as a string</returns>
    public Task<string> GetSchemaAsStringAsync();

    /// <summary>
    /// Reads the UI schema file and returns it as a string
    /// </summary>
    /// <returns>The UI schema file as a string</returns>
    public Task<string> GetUiSchemaAsStringAsync();
}
