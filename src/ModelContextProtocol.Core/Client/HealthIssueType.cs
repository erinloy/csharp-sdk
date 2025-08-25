using System;

namespace ModelContextProtocol.Core.Client;

/// <summary>
/// Defines types of health issues that can affect MCP client connections.
/// </summary>
public enum HealthIssueType
{
    /// <summary>
    /// No health issues detected.
    /// </summary>
    None = 0,

    /// <summary>
    /// High latency affecting response times.
    /// </summary>
    HighLatency = 1,

    /// <summary>
    /// Frequent timeout events occurring.
    /// </summary>
    FrequentTimeouts = 2,

    /// <summary>
    /// Connection drops or instability.
    /// </summary>
    ConnectionInstability = 3,

    /// <summary>
    /// Authentication or authorization failures.
    /// </summary>
    AuthenticationFailure = 4,

    /// <summary>
    /// Server-side errors affecting operations.
    /// </summary>
    ServerError = 5,

    /// <summary>
    /// Network connectivity issues.
    /// </summary>
    NetworkIssue = 6,

    /// <summary>
    /// Critical error requiring immediate attention.
    /// </summary>
    CriticalError = 7,

    /// <summary>
    /// Resource constraints affecting performance.
    /// </summary>
    ResourceConstraint = 8,

    /// <summary>
    /// Protocol version mismatch or compatibility issues.
    /// </summary>
    ProtocolMismatch = 9
}