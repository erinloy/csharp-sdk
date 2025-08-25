using System;
using System.Collections.Generic;
using System.Linq;

namespace ModelContextProtocol.Core.Client;

/// <summary>
/// Provides comprehensive metrics for connection quality assessment
/// </summary>
public class ConnectionQualityMetrics
{
    /// <summary>
    /// Current connection quality level
    /// </summary>
    public ConnectionQuality Quality { get; set; } = ConnectionQuality.Unknown;

    /// <summary>
    /// Overall health percentage (0-100)
    /// </summary>
    public double HealthPercentage { get; set; }

    /// <summary>
    /// Success rate of operations (0-100)
    /// </summary>
    public double SuccessRate { get; set; }

    /// <summary>
    /// Average response time in milliseconds
    /// </summary>
    public double AverageResponseTime { get; set; }

    /// <summary>
    /// Number of connection errors in the current period
    /// </summary>
    public int ErrorCount { get; set; }

    /// <summary>
    /// Number of successful operations in the current period
    /// </summary>
    public int SuccessCount { get; set; }

    /// <summary>
    /// List of current health issues
    /// </summary>
    public List<HealthIssue> HealthIssues { get; set; } = new();

    /// <summary>
    /// Current quality trend
    /// </summary>
    public QualityTrend Trend { get; set; } = QualityTrend.Stable;

    /// <summary>
    /// Performance impact level
    /// </summary>
    public PerformanceImpact PerformanceImpact { get; set; } = PerformanceImpact.None;

    /// <summary>
    /// Timestamp of last quality assessment
    /// </summary>
    public DateTime LastAssessment { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// Calculates the success rate based on success and error counts
    /// </summary>
    /// <returns>Success rate as a percentage (0-100)</returns>
    public double CalculateSuccessRate()
    {
        var totalOperations = SuccessCount + ErrorCount;
        return totalOperations == 0 ? 100.0 : (double)SuccessCount / totalOperations * 100.0;
    }

    /// <summary>
    /// Determines if the connection is healthy based on current metrics
    /// </summary>
    /// <returns>True if connection is healthy, false otherwise</returns>
    public bool IsHealthy()
    {
        return Quality >= ConnectionQuality.Average && 
               SuccessRate >= 90.0 && 
               !HealthIssues.Any(hi => hi.Severity >= HealthIssueSeverity.High);
    }

    /// <summary>
    /// Gets the highest severity health issue
    /// </summary>
    /// <returns>The highest severity health issue, or null if none exist</returns>
    public HealthIssue? GetHighestSeverityIssue()
    {
        return HealthIssues.OrderByDescending(hi => hi.Severity).FirstOrDefault();
    }
}

/// <summary>
/// Represents a health issue with the connection
/// </summary>
public class HealthIssue
{
    /// <summary>
    /// Type of health issue
    /// </summary>
    public HealthIssueType Type { get; set; }

    /// <summary>
    /// Severity level of the issue
    /// </summary>
    public HealthIssueSeverity Severity { get; set; }

    /// <summary>
    /// Description of the issue
    /// </summary>
    public string Description { get; set; } = string.Empty;

    /// <summary>
    /// Timestamp when the issue was detected
    /// </summary>
    public DateTime DetectedAt { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// Number of times this issue has occurred
    /// </summary>
    public int OccurrenceCount { get; set; } = 1;
}