using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using SkyPointSocial.Core.ClientModels.Session;

namespace SkyPointSocial.Core.Interfaces
{
    /// <summary>
    /// Service for managing user sessions
    /// </summary>
    public interface ISessionService
    {
        /// <summary>
        /// Create a new session when user logs in
        /// </summary>
        /// <param name="userId">User ID</param>
        /// <returns>Session ID</returns>
        Task<Guid> CreateSessionAsync(Guid userId);

        /// <summary>
        /// End a session when user logs out
        /// - Records logout time
        /// - Calculates session duration
        /// </summary>
        /// <param name="sessionId">Session ID</param>
        /// <returns>Session summary with duration</returns>
        Task<SessionSummaryClientModel> EndSessionAsync(Guid sessionId);

        /// <summary>
        /// Get session details by ID
        /// </summary>
        /// <param name="sessionId">Session ID</param>
        /// <returns>Session summary</returns>
        Task<SessionSummaryClientModel> GetSessionAsync(Guid sessionId);

        /// <summary>
        /// Get all sessions for a user
        /// </summary>
        /// <param name="userId">User ID</param>
        /// <param name="page">Page number</param>
        /// <param name="pageSize">Items per page</param>
        /// <returns>List of session summaries</returns>
        Task<List<SessionSummaryClientModel>> GetUserSessionsAsync(Guid userId, int page = 1, int pageSize = 20);

        /// <summary>
        /// Check if a session is active
        /// </summary>
        /// <param name="sessionId">Session ID</param>
        /// <returns>True if session is active</returns>
        Task<bool> IsSessionActiveAsync(Guid sessionId);

        /// <summary>
        /// Get active session for a user
        /// </summary>
        /// <param name="userId">User ID</param>
        /// <returns>Active session ID or null</returns>
        Task<Guid?> GetActiveSessionAsync(Guid userId);

        /// <summary>
        /// Clean up expired sessions
        /// - Sets logout time for abandoned sessions
        /// </summary>
        /// <param name="expirationTime">Time after which sessions are considered expired</param>
        Task CleanupExpiredSessionsAsync(TimeSpan expirationTime);
    }
}