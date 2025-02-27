namespace Shared;

/// <summary>
/// The order in which the decorator should be executed
/// </summary>
public enum EventExecutionOrder
{
    /// <summary>
    /// The specified decorator does not have event execution logic and does not need to be called.
    /// </summary>
    None = 0,

    /// <summary>
    /// Execute the decorator as the base service.
    /// </summary>
    BaseService = 1,

    /// <summary>
    /// Override the base service with the decorator. The base service will not be executed.
    /// </summary>
    OverrideBaseService = 2,

    /// <summary>
    /// Execute the decorator before the base service.
    /// The decorator will receive the request body as sent by the client.
    /// </summary>
    BeforeBaseService = 3,

    /// <summary>
    /// Execute the decorator after the base service.
    /// The decorator will receive the result of the base service as the request body.
    /// </summary>
    AfterBaseService = 4,

    // InsertBaseService = 4, For future: insert into the middle of the base service functionality???? could be difficult
}
