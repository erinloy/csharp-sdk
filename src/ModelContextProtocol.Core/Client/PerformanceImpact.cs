using System;

namespace ModelContextProtocol.Core.Client;

/// <summary>
/// Defines the performance impact levels for connection issues or changes.
/// </summary>
public enum PerformanceImpact
{
    /// <summary>
    /// No measurable performance impact.
    /// </summary>
    None = 0,

    /// <summary>
    /// Minimal performance impact, barely noticeable.
    /// </summary>
    Minimal = 1,

    /// <summary>
    /// Minor performance impact with slight degradation.
    /// </summary>
    Minor = 2,

    /// <summary>
    /// Moderate performance impact affecting user experience.
    /// </summary>
    Moderate = 3,

    /// <summary>
    /// Significant performance impact with noticeable delays.
    /// </summary>
    Significant = 4,

    /// <summary>
    /// Severe performance impact making operations difficult or unusable.
    /// </summary>
    Severe = 5
}