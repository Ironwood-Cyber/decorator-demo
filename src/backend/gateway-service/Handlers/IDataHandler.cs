using System.Threading.Tasks;
using Newtonsoft.Json.Linq;

namespace GatewayService.Handlers;

/// <summary>
/// Interface for handling data from various services
/// </summary>
public interface IDataHandler
{
    /// <summary>
    /// Gets the data schema from the various services
    /// </summary>
    /// <returns><see cref="JObject"/> containing the JsonForms data</returns>
    public Task<JObject> GetDataAsync();

    /// <summary>
    /// Gets the schema from the various services
    /// </summary>
    /// <returns><see cref="JObject"/> containing the JsonForms schema</returns>
    public Task<JObject> GetSchemaAsync();

    /// <summary>
    /// Gets the UI schema from the various services
    /// </summary>
    /// <returns><see cref="JObject"/> containing the JsonForms UI schema</returns>
    public Task<JObject> GetUiSchemaAsync();

    /// <summary>
    /// Gets the event handler from the various service(s)
    /// </summary>
    /// <returns><see cref="JObject"/> containing the json encoded javascript to handle an event client side</returns>
    public Task<JObject> GetEventHandlerAsync();

    /// <summary>
    /// Handles the event produced by JsonForms on the client side
    /// </summary>
    /// <param name="data">The data produced by the event</param>
    /// <returns><see cref="JObject"/> representing the processed data</returns>
    public Task<JObject?> HandleEventAsync(object data);
}
