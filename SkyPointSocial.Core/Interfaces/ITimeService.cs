using System;

namespace SkyPointSocial.Core.Interfaces
{
    /// <summary>
    /// Service for handling time-related operations
    /// </summary>
    public interface ITimeService
    {
        /// <summary>
        /// Convert DateTime to human-readable "time ago" format
        /// Examples: "2h ago", "3d ago", "1mo ago"
        /// </summary>
        /// <param name="dateTime">DateTime to convert</param>
        /// <returns>Human-readable time ago string</returns>
        string GetTimeAgo(DateTime dateTime);

        /// <summary>
        /// Format duration for session summary
        /// Example: "2h 30m", "45m", "1d 3h"
        /// </summary>
        /// <param name="duration">Duration to format</param>
        /// <returns>Human-readable duration string</returns>
        string FormatDuration(TimeSpan duration);

        /// <summary>
        /// Get current UTC time
        /// </summary>
        /// <returns>Current UTC DateTime</returns>
        DateTime GetCurrentUtcTime();
    }
}