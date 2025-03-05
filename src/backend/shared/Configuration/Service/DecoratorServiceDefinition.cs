namespace Shared.Configuration.Service;

/// <summary>
/// Represents a decorator service that can be called by the gateway service
/// </summary>
public class DecoratorServiceDefinition
{
    /// <summary>
    /// The URI of the service to call
    /// </summary>
    public string ServiceUri { get; set; } = string.Empty;

    /// <summary>
    /// The order in which the decorator service should handle events in relation to the base service
    /// </summary>
    public EventExecutionOrder ExecutionOrder { get; set; }
}
