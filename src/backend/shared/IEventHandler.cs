namespace Shared;

/// <summary>
/// Interface for fetching client side event handers and handling server side events from JsonForms
/// </summary>
public interface IEventHandler
{
    /// <summary>
    /// Gets the event handler code for client side processing
    /// </summary>
    /// <returns>The event handler javascript code as a string</returns>
    public Task<string> GetEventHandlerAsync();

    /// <summary>
    /// Handles the schema event
    /// </summary>
    /// <param name="data">The data to handle</param>
    /// <returns>The result of the event handling in a json string format</returns>
    public string? HandleSchemaEvent(object data);
}
