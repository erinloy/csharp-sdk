namespace ModelContextProtocol.Core.Client;

/// <summary>
/// Levels of performance impact from connection quality changes
/// </summary>
public enum PerformanceImpact
{
    /// <summary>
    /// No performance impact
    /// </summary>
    None = 0,

    /// <summary>
    /// Minimal performance impact
    /// </summary>
    Minimal = 1,

    /// <summary>
    /// Minor performance impact
    /// </summary>
    Minor = 2,

    /// <summary>
    /// Moderate performance impact
    /// </summary>
    Moderate = 3,

    /// <summary>
    /// Significant performance impact
    /// </summary>
    Significant = 4,

    /// <summary>
    /// Severe performance impact
    /// </summary>
    Severe = 5
}