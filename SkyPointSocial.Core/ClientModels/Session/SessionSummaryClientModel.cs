using System;

namespace SkyPointSocial.Core.ClientModels.Session
{
    /// <summary>
    /// Session summary shown on logout
    /// </summary>
    public class SessionSummaryClientModel
    {
        /// <summary>
        /// When the user logged in
        /// </summary>
        public DateTime LoginTime { get; set; }

        /// <summary>
        /// When the user logged out
        /// </summary>
        public DateTime LogoutTime { get; set; }

        /// <summary>
        /// Total session duration
        /// </summary>
        public TimeSpan Duration { get; set; }

        /// <summary>
        /// Human-readable duration (e.g., "2h 30m")
        /// </summary>
        public string FormattedDuration { get; set; }
    }
}