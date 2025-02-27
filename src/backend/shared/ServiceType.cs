namespace Shared;

/// <summary>
/// Enum representing the type of service (base or decorator)
/// </summary>
public enum ServiceType
{
    /// <summary>
    /// The service is a base service, implementing the core functionality
    /// </summary>
    Base = 0,

    /// <summary>
    /// The service is a decorator
    /// </summary>
    Decorator = 1,
}
