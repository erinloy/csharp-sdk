using System;
using System.Collections.Generic;

namespace ModelContextProtocol.Core.Client;

/// <summary>
/// Comprehensive metrics for monitoring MCP client connection quality and performance.
/// </summary>
public class ConnectionQualityMetrics
{
    /// <summary>
    /// Current connection quality assessment.
    /// </summary>
    public ConnectionQuality Quality { get; set; } = ConnectionQuality.Unknown;

    /// <summary>
    /// Average latency in milliseconds over the measurement period.
    /// </summary>
    public double AverageLatencyMs { get; set; }

    /// <summary>
    /// Current trend in connection quality.
    /// </summary>
    public QualityTrend Trend { get; set; } = QualityTrend.Stable;

    /// <summary>
    /// Timestamp when these metrics were last updated.
    /// </summary>
    public DateTime LastUpdated { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// Connection uptime percentage over the measurement period.
    /// </summary>
    public double UptimePercentage { get; set; } = 100.0;

    /// <summary>
    /// Number of successful operations in the measurement period.
    /// </summary>
    public int SuccessfulOperations { get; set; }

    /// <summary>
    /// Number of failed operations in the measurement period.
    /// </summary>
    public int FailedOperations { get; set; }

    /// <summary>
    /// Number of timeout events in the measurement period.
    /// </summary>
    public int TimeoutCount { get; set; }

    /// <summary>
    /// Current health issues affecting the connection.
    /// </summary>
    public List<HealthIssueType> ActiveHealthIssues { get; set; } = new();

    /// <summary>
    /// Calculates the success rate as a percentage.
    /// </summary>
    public double SuccessRate => 
        SuccessfulOperations + FailedOperations == 0 ? 100.0 : 
        (double)SuccessfulOperations / (SuccessfulOperations + FailedOperations) * 100.0;

    /// <summary>
    /// Determines if the connection is considered healthy based on current metrics.
    /// </summary>
    public bool IsHealthy => Quality >= ConnectionQuality.Average && 
                           SuccessRate >= 95.0 && 
                           UptimePercentage >= 99.0 &&
                           !ActiveHealthIssues.Contains(HealthIssueType.CriticalError);
}