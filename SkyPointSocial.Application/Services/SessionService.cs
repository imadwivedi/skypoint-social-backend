using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using SkyPointSocial.Core.ClientModels.Session;
using SkyPointSocial.Core.Entities;
using SkyPointSocial.Core.Interfaces;
using SkyPointSocial.Application.Data;

namespace SkyPointSocial.Application.Services
{
    /// <summary>
    /// Service for managing user sessions
    /// </summary>
    public class SessionService : ISessionService
    {
        private readonly AppDbContext _context;
        private readonly ITimeService _timeService;

        public SessionService(AppDbContext context, ITimeService timeService)
        {
            _context = context;
            _timeService = timeService;
        }

        /// <summary>
        /// Create a new session when user logs in
        /// </summary>
        public async Task<Guid> CreateSessionAsync(Guid userId)
        {
            // Session ID is string in the entity, so we generate a string ID
            var sessionId = Guid.NewGuid();
            var session = new Session
            {
                Id = sessionId,  // String ID as per entity
                UserId = userId, // Guid as per entity
                LoginTime = _timeService.GetCurrentUtcTime()
            };

            _context.Sessions.Add(session);
            await _context.SaveChangesAsync();

            return sessionId;
        }

        /// <summary>
        /// End a session when user logs out
        /// </summary>
        public async Task<SessionSummaryClientModel> EndSessionAsync(Guid sessionId)
        {
            var session = await _context.Sessions
                .FirstOrDefaultAsync(s => s.Id == sessionId); // String comparison

            if (session == null)
                throw new InvalidOperationException("Session not found");

            session.LogoutTime = _timeService.GetCurrentUtcTime();
            session.Duration = session.LogoutTime.Value - session.LoginTime;

            await _context.SaveChangesAsync();

            return MapToClientModel(session);
        }

        /// <summary>
        /// Get session details by ID
        /// </summary>
        public async Task<SessionSummaryClientModel> GetSessionAsync(Guid sessionId)
        {
            var session = await _context.Sessions
                .FirstOrDefaultAsync(s => s.Id == sessionId);

            if (session == null)
                return null;

            return MapToClientModel(session);
        }

        /// <summary>
        /// Get all sessions for a user
        /// </summary>
        public async Task<List<SessionSummaryClientModel>> GetUserSessionsAsync(Guid userId, int page = 1, int pageSize = 20)
        {
            var sessions = await _context.Sessions
                .Where(s => s.UserId == userId) // Guid comparison
                .OrderByDescending(s => s.LoginTime)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            return sessions.Select(MapToClientModel).ToList();
        }

        /// <summary>
        /// Check if a session is active
        /// </summary>
        public async Task<bool> IsSessionActiveAsync(Guid sessionId)
        {
            var session = await _context.Sessions
                .FirstOrDefaultAsync(s => s.Id == sessionId); // String comparison

            return session != null && !session.LogoutTime.HasValue;
        }

        /// <summary>
        /// Get active session for a user
        /// </summary>
        public async Task<Guid?> GetActiveSessionAsync(Guid userId)
        {
            var activeSession = await _context.Sessions
                .Where(s => s.UserId == userId && !s.LogoutTime.HasValue) // Guid comparison
                .OrderByDescending(s => s.LoginTime)
                .FirstOrDefaultAsync();

            return activeSession?.Id;
        }

        /// <summary>
        /// Clean up expired sessions
        /// </summary>
        public async Task CleanupExpiredSessionsAsync(TimeSpan expirationTime)
        {
            var cutoffTime = _timeService.GetCurrentUtcTime() - expirationTime;
            
            var expiredSessions = await _context.Sessions
                .Where(s => !s.LogoutTime.HasValue && s.LoginTime < cutoffTime)
                .ToListAsync();

            foreach (var session in expiredSessions)
            {
                session.LogoutTime = session.LoginTime.Add(expirationTime);
                session.Duration = expirationTime;
            }

            await _context.SaveChangesAsync();
        }

        private SessionSummaryClientModel MapToClientModel(Session session)
        {
            var duration = session.Duration ?? 
                (session.LogoutTime?.Subtract(session.LoginTime) ?? 
                (_timeService.GetCurrentUtcTime() - session.LoginTime));

            return new SessionSummaryClientModel
            {
                LoginTime = session.LoginTime,
                LogoutTime = session.LogoutTime ?? _timeService.GetCurrentUtcTime(),
                Duration = duration,
                FormattedDuration = _timeService.FormatDuration(duration)
            };
        }
    }
}