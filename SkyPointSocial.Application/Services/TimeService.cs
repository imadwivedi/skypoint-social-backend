using System;
using SkyPointSocial.Core.Interfaces;

namespace SkyPointSocial.Application.Services
{
    /// <summary>
    /// Implementation of time-related operations
    /// </summary>
    public class TimeService : ITimeService
    {
        /// <summary>
        /// Convert DateTime to human-readable "time ago" format
        /// </summary>
        public string GetTimeAgo(DateTime dateTime)
        {
            var timeSpan = GetCurrentUtcTime() - dateTime;

            if (timeSpan.TotalSeconds < 60)
                return "just now";
            
            if (timeSpan.TotalMinutes < 60)
                return $"{(int)timeSpan.TotalMinutes}m ago";
            
            if (timeSpan.TotalHours < 24)
                return $"{(int)timeSpan.TotalHours}h ago";
            
            if (timeSpan.TotalDays < 7)
                return $"{(int)timeSpan.TotalDays}d ago";
            
            if (timeSpan.TotalDays < 30)
                return $"{(int)(timeSpan.TotalDays / 7)}w ago";
            
            if (timeSpan.TotalDays < 365)
                return $"{(int)(timeSpan.TotalDays / 30)}mo ago";
            
            return $"{(int)(timeSpan.TotalDays / 365)}y ago";
        }

        /// <summary>
        /// Format duration for session summary
        /// </summary>
        public string FormatDuration(TimeSpan duration)
        {
            if (duration.TotalDays >= 1)
            {
                var days = (int)duration.TotalDays;
                var hours = duration.Hours;
                return hours > 0 ? $"{days}d {hours}h" : $"{days}d";
            }
            
            if (duration.TotalHours >= 1)
            {
                var hours = (int)duration.TotalHours;
                var minutes = duration.Minutes;
                return minutes > 0 ? $"{hours}h {minutes}m" : $"{hours}h";
            }
            
            if (duration.TotalMinutes >= 1)
            {
                return $"{(int)duration.TotalMinutes}m";
            }
            
            return "< 1m";
        }

        /// <summary>
        /// Get current UTC time
        /// </summary>
        public DateTime GetCurrentUtcTime()
        {
            return DateTime.UtcNow;
        }
    }
}