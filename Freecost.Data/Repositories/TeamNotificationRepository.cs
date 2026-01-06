using Freecost.Core.Models;
using Freecost.Core.Repositories;
using Freecost.Data.LocalDatabase;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Freecost.Data.Repositories;

public class TeamNotificationRepository : ITeamNotificationRepository
{
    private readonly FreecostDbContext _context;

    public TeamNotificationRepository(FreecostDbContext context)
    {
        _context = context;
    }

    public async Task<TeamNotification?> GetByIdAsync(Guid id)
    {
        return await _context.TeamNotifications
            .Include(n => n.User)
            .Include(n => n.RelatedUser)
            .FirstOrDefaultAsync(n => n.Id == id);
    }

    public async Task<List<TeamNotification>> GetByUserAsync(Guid userId, bool unreadOnly = false)
    {
        var query = _context.TeamNotifications
            .Include(n => n.User)
            .Include(n => n.RelatedUser)
            .Where(n => n.UserId == userId);

        if (unreadOnly)
        {
            query = query.Where(n => !n.IsRead);
        }

        return await query
            .OrderByDescending(n => n.Priority)
            .ThenByDescending(n => n.CreatedAt)
            .ToListAsync();
    }

    public async Task<int> GetUnreadCountAsync(Guid userId)
    {
        return await _context.TeamNotifications
            .Where(n => n.UserId == userId && !n.IsRead)
            .CountAsync();
    }

    public async Task<TeamNotification> CreateAsync(TeamNotification notification)
    {
        _context.TeamNotifications.Add(notification);
        await _context.SaveChangesAsync();
        return notification;
    }

    public async Task UpdateAsync(TeamNotification notification)
    {
        _context.TeamNotifications.Update(notification);
        await _context.SaveChangesAsync();
    }

    public async Task MarkAllAsReadAsync(Guid userId)
    {
        var notifications = await _context.TeamNotifications
            .Where(n => n.UserId == userId && !n.IsRead)
            .ToListAsync();

        foreach (var notification in notifications)
        {
            notification.IsRead = true;
            notification.ReadAt = DateTime.UtcNow;
        }

        await _context.SaveChangesAsync();
    }

    public async Task DeleteAsync(Guid id)
    {
        var notification = await GetByIdAsync(id);
        if (notification != null)
        {
            _context.TeamNotifications.Remove(notification);
            await _context.SaveChangesAsync();
        }
    }

    public async Task DeleteOldAsync(DateTime beforeDate)
    {
        var oldNotifications = await _context.TeamNotifications
            .Where(n => n.CreatedAt < beforeDate)
            .ToListAsync();

        _context.TeamNotifications.RemoveRange(oldNotifications);
        await _context.SaveChangesAsync();
    }
}
