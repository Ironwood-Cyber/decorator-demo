namespace Shared;

/// <summary>
/// A message object sent between services via MassTransit
/// </summary>
public class Message
{
    /// <summary>
    /// The serialized JSON data
    /// </summary>
    public required string JsonData { get; set; }

    /// <summary>
    /// The timestamp of the message
    /// </summary>
    public DateTime Timestamp { get; set; } = DateTime.UtcNow;
}
