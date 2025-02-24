namespace GatewayService.Configuration.Dll;

/// <summary>
/// Represents a decorator service dll that can be called by the gateway service
/// </summary>
public class DecoratorServiceDllDefinition
{
    /// <summary>
    /// The path to the dll of the service to call
    /// </summary>
    public string ServiceDll { get; set; } = string.Empty;

    /// <summary>
    /// The order in which the decorator dll should handle events in relation to the base dll
    /// </summary>
    public EventExecutionOrder ExecutionOrder { get; set; }
}
