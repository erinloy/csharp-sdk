namespace ModelContextProtocol.Core.Client;

/// <summary>
/// Trends in connection quality over time
/// </summary>
public enum QualityTrend
{
    /// <summary>
    /// Connection quality is deteriorating rapidly
    /// </summary>
    Deteriorating = -2,

    /// <summary>
    /// Connection quality is declining gradually
    /// </summary>
    Declining = -1,

    /// <summary>
    /// Connection quality is stable
    /// </summary>
    Stable = 0,

    /// <summary>
    /// Connection quality is improving gradually
    /// </summary>
    Improving = 1,

    /// <summary>
    /// Connection quality is optimizing rapidly
    /// </summary>
    Optimizing = 2
}