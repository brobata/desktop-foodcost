using Dfc.Core.Models;
using Dfc.Core.Repositories;
using Dfc.Data.LocalDatabase;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Dfc.Data.Repositories;

public class RecipeCommentRepository : IRecipeCommentRepository
{
    private readonly DfcDbContext _context;

    public RecipeCommentRepository(DfcDbContext context)
    {
        _context = context;
    }

    public async Task<RecipeComment?> GetByIdAsync(Guid id)
    {
        return await _context.RecipeComments
            .Include(c => c.Recipe)
            .Include(c => c.User)
            .Include(c => c.Replies)
                .ThenInclude(r => r.User)
            .FirstOrDefaultAsync(c => c.Id == id);
    }

    public async Task<List<RecipeComment>> GetByRecipeAsync(Guid recipeId)
    {
        return await _context.RecipeComments
            .Include(c => c.User)
            .Include(c => c.Replies)
                .ThenInclude(r => r.User)
            .Where(c => c.RecipeId == recipeId && c.ParentCommentId == null)
            .OrderByDescending(c => c.CommentedAt)
            .ToListAsync();
    }

    public async Task<List<RecipeComment>> GetThreadAsync(Guid parentCommentId)
    {
        return await _context.RecipeComments
            .Include(c => c.User)
            .Where(c => c.ParentCommentId == parentCommentId)
            .OrderBy(c => c.CommentedAt)
            .ToListAsync();
    }

    public async Task<List<RecipeComment>> GetByUserAsync(Guid userId)
    {
        return await _context.RecipeComments
            .Include(c => c.Recipe)
            .Include(c => c.User)
            .Where(c => c.UserId == userId)
            .OrderByDescending(c => c.CommentedAt)
            .ToListAsync();
    }

    public async Task<List<RecipeComment>> GetMentioningUserAsync(Guid userId)
    {
        var userIdStr = userId.ToString();
        return await _context.RecipeComments
            .Include(c => c.Recipe)
            .Include(c => c.User)
            .Where(c => c.Mentions.Contains(userIdStr))
            .OrderByDescending(c => c.CommentedAt)
            .ToListAsync();
    }

    public async Task<RecipeComment> CreateAsync(RecipeComment comment)
    {
        _context.RecipeComments.Add(comment);
        await _context.SaveChangesAsync();
        return comment;
    }

    public async Task UpdateAsync(RecipeComment comment)
    {
        _context.RecipeComments.Update(comment);
        await _context.SaveChangesAsync();
    }

    public async Task DeleteAsync(Guid id)
    {
        var comment = await GetByIdAsync(id);
        if (comment != null)
        {
            _context.RecipeComments.Remove(comment);
            await _context.SaveChangesAsync();
        }
    }
}
