using System;

namespace ModelContextProtocol.Core.Client;

/// <summary>
/// Defines the severity levels for health issues in MCP client connections.
/// </summary>
public enum HealthIssueSeverity
{
    /// <summary>
    /// Informational level - minor issues that don't affect functionality.
    /// </summary>
    Info = 0,

    /// <summary>
    /// Low severity - minor issues with minimal impact.
    /// </summary>
    Low = 1,

    /// <summary>
    /// Medium severity - issues that may affect performance but don't prevent operation.
    /// </summary>
    Medium = 2,

    /// <summary>
    /// High severity - issues that significantly impact performance or reliability.
    /// </summary>
    High = 3,

    /// <summary>
    /// Critical severity - issues that prevent proper operation and require immediate attention.
    /// </summary>
    Critical = 4
}