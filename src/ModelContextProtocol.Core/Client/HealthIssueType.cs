namespace ModelContextProtocol.Core.Client;

/// <summary>
/// Types of health issues that can occur with a connection
/// </summary>
public enum HealthIssueType
{
    /// <summary>
    /// Unknown or unclassified health issue
    /// </summary>
    Unknown = 0,

    /// <summary>
    /// Connection timeout issues
    /// </summary>
    Timeout = 1,

    /// <summary>
    /// Network connectivity problems
    /// </summary>
    NetworkConnectivity = 2,

    /// <summary>
    /// Authentication or authorization failures
    /// </summary>
    Authentication = 3,

    /// <summary>
    /// Server-side errors or unavailability
    /// </summary>
    ServerError = 4,

    /// <summary>
    /// Protocol-level communication errors
    /// </summary>
    ProtocolError = 5,

    /// <summary>
    /// Resource exhaustion (memory, CPU, etc.)
    /// </summary>
    ResourceExhaustion = 6,

    /// <summary>
    /// Configuration-related issues
    /// </summary>
    Configuration = 7,

    /// <summary>
    /// Rate limiting or throttling issues
    /// </summary>
    RateLimiting = 8,

    /// <summary>
    /// Data serialization/deserialization errors
    /// </summary>
    Serialization = 9,

    /// <summary>
    /// High latency affecting performance
    /// </summary>
    HighLatency = 10
}