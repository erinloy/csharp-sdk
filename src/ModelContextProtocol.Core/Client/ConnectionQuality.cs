namespace ModelContextProtocol.Core.Client;

/// <summary>
/// Represents the quality level of a connection
/// </summary>
public enum ConnectionQuality
{
    /// <summary>
    /// Connection quality is unknown or not assessed
    /// </summary>
    Unknown = 0,

    /// <summary>
    /// Poor connection quality with significant issues
    /// </summary>
    Poor = 1,

    /// <summary>
    /// Below average connection quality with some issues
    /// </summary>
    BelowAverage = 2,

    /// <summary>
    /// Average connection quality with minor issues
    /// </summary>
    Average = 3,

    /// <summary>
    /// Good connection quality with minimal issues
    /// </summary>
    Good = 4,

    /// <summary>
    /// Excellent connection quality with no issues
    /// </summary>
    Excellent = 5
}