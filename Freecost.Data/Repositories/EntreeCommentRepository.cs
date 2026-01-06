using Freecost.Core.Models;
using Freecost.Core.Repositories;
using Freecost.Data.LocalDatabase;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Freecost.Data.Repositories;

public class EntreeCommentRepository : IEntreeCommentRepository
{
    private readonly FreecostDbContext _context;

    public EntreeCommentRepository(FreecostDbContext context)
    {
        _context = context;
    }

    public async Task<EntreeComment?> GetByIdAsync(Guid id)
    {
        return await _context.EntreeComments
            .Include(c => c.Entree)
            .Include(c => c.User)
            .Include(c => c.Replies)
                .ThenInclude(r => r.User)
            .FirstOrDefaultAsync(c => c.Id == id);
    }

    public async Task<List<EntreeComment>> GetByEntreeAsync(Guid entreeId)
    {
        return await _context.EntreeComments
            .Include(c => c.User)
            .Include(c => c.Replies)
                .ThenInclude(r => r.User)
            .Where(c => c.EntreeId == entreeId && c.ParentCommentId == null)
            .OrderByDescending(c => c.CommentedAt)
            .ToListAsync();
    }

    public async Task<List<EntreeComment>> GetThreadAsync(Guid parentCommentId)
    {
        return await _context.EntreeComments
            .Include(c => c.User)
            .Where(c => c.ParentCommentId == parentCommentId)
            .OrderBy(c => c.CommentedAt)
            .ToListAsync();
    }

    public async Task<List<EntreeComment>> GetByUserAsync(Guid userId)
    {
        return await _context.EntreeComments
            .Include(c => c.Entree)
            .Include(c => c.User)
            .Where(c => c.UserId == userId)
            .OrderByDescending(c => c.CommentedAt)
            .ToListAsync();
    }

    public async Task<List<EntreeComment>> GetMentioningUserAsync(Guid userId)
    {
        var userIdStr = userId.ToString();
        return await _context.EntreeComments
            .Include(c => c.Entree)
            .Include(c => c.User)
            .Where(c => c.Mentions.Contains(userIdStr))
            .OrderByDescending(c => c.CommentedAt)
            .ToListAsync();
    }

    public async Task<EntreeComment> CreateAsync(EntreeComment comment)
    {
        _context.EntreeComments.Add(comment);
        await _context.SaveChangesAsync();
        return comment;
    }

    public async Task UpdateAsync(EntreeComment comment)
    {
        _context.EntreeComments.Update(comment);
        await _context.SaveChangesAsync();
    }

    public async Task DeleteAsync(Guid id)
    {
        var comment = await GetByIdAsync(id);
        if (comment != null)
        {
            _context.EntreeComments.Remove(comment);
            await _context.SaveChangesAsync();
        }
    }
}
