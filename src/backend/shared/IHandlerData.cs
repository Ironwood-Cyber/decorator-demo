namespace Shared;

/// <summary>
/// Metadata for the handler
/// </summary>
public interface IHandlerData
{
    /// <summary>
    /// The order in which the handler should be called
    /// </summary>
    public EventExecutionOrder ExecutionOrder { get; }

    /// <summary>
    /// The type of service (base or decorator)
    /// </summary>
    public ServiceType ServiceType { get; }
}
