using System;

namespace ModelContextProtocol.Core.Client;

/// <summary>
/// Defines the quality levels for MCP client connections.
/// Used to categorize connection performance and reliability.
/// </summary>
public enum ConnectionQuality
{
    /// <summary>
    /// Connection quality is unknown or not yet determined.
    /// </summary>
    Unknown = 0,

    /// <summary>
    /// Poor connection quality with frequent issues.
    /// High latency, frequent timeouts, or connection drops.
    /// </summary>
    Poor = 1,

    /// <summary>
    /// Below average connection quality with occasional issues.
    /// Moderate latency with some reliability concerns.
    /// </summary>
    BelowAverage = 2,

    /// <summary>
    /// Average connection quality meeting basic requirements.
    /// Acceptable latency and reliability for most operations.
    /// </summary>
    Average = 3,

    /// <summary>
    /// Good connection quality with reliable performance.
    /// Low latency and consistent availability.
    /// </summary>
    Good = 4,

    /// <summary>
    /// Excellent connection quality with optimal performance.
    /// Very low latency and high reliability.
    /// </summary>
    Excellent = 5
}