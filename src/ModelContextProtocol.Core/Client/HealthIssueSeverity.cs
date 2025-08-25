namespace ModelContextProtocol.Core.Client;

/// <summary>
/// Severity levels for health issues
/// </summary>
public enum HealthIssueSeverity
{
    /// <summary>
    /// Informational issue with no impact
    /// </summary>
    Info = 0,

    /// <summary>
    /// Low severity issue with minimal impact
    /// </summary>
    Low = 1,

    /// <summary>
    /// Medium severity issue with moderate impact
    /// </summary>
    Medium = 2,

    /// <summary>
    /// High severity issue with significant impact
    /// </summary>
    High = 3,

    /// <summary>
    /// Critical issue that severely impacts functionality
    /// </summary>
    Critical = 4
}