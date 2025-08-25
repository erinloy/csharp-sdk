using System;

namespace ModelContextProtocol.Core.Client;

/// <summary>
/// Defines the trend direction for connection quality metrics.
/// </summary>
public enum QualityTrend
{
    /// <summary>
    /// Quality trend is unknown or cannot be determined.
    /// </summary>
    Unknown = 0,

    /// <summary>
    /// Quality is rapidly deteriorating.
    /// </summary>
    Deteriorating = 1,

    /// <summary>
    /// Quality is slowly declining.
    /// </summary>
    Declining = 2,

    /// <summary>
    /// Quality remains stable with no significant change.
    /// </summary>
    Stable = 3,

    /// <summary>
    /// Quality is gradually improving.
    /// </summary>
    Improving = 4,

    /// <summary>
    /// Quality is rapidly improving.
    /// </summary>
    Optimizing = 5
}