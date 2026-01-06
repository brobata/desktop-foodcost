using Dfc.Core.Models;
using Dfc.Core.Repositories;
using Dfc.Data.LocalDatabase;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Dfc.Data.Repositories;

public class SharedRecipeRepository : ISharedRecipeRepository
{
    private readonly DfcDbContext _context;

    public SharedRecipeRepository(DfcDbContext context)
    {
        _context = context;
    }

    public async Task<SharedRecipe?> GetByIdAsync(Guid id)
    {
        return await _context.SharedRecipes
            .Include(s => s.Recipe)
            .Include(s => s.SharedByUser)
            .Include(s => s.SharedWithUser)
            .FirstOrDefaultAsync(s => s.Id == id);
    }

    public async Task<SharedRecipe?> GetByRecipeAndUserAsync(Guid recipeId, Guid userId)
    {
        return await _context.SharedRecipes
            .Include(s => s.Recipe)
            .Include(s => s.SharedByUser)
            .Include(s => s.SharedWithUser)
            .FirstOrDefaultAsync(s => s.RecipeId == recipeId && s.SharedWithUserId == userId);
    }

    public async Task<List<SharedRecipe>> GetByRecipeAsync(Guid recipeId)
    {
        return await _context.SharedRecipes
            .Include(s => s.Recipe)
            .Include(s => s.SharedByUser)
            .Include(s => s.SharedWithUser)
            .Where(s => s.RecipeId == recipeId)
            .OrderByDescending(s => s.SharedAt)
            .ToListAsync();
    }

    public async Task<List<SharedRecipe>> GetSharedWithUserAsync(Guid userId)
    {
        return await _context.SharedRecipes
            .Include(s => s.Recipe)
            .Include(s => s.SharedByUser)
            .Include(s => s.SharedWithUser)
            .Where(s => s.SharedWithUserId == userId)
            .OrderByDescending(s => s.SharedAt)
            .ToListAsync();
    }

    public async Task<List<SharedRecipe>> GetSharedByUserAsync(Guid userId)
    {
        return await _context.SharedRecipes
            .Include(s => s.Recipe)
            .Include(s => s.SharedByUser)
            .Include(s => s.SharedWithUser)
            .Where(s => s.SharedByUserId == userId)
            .OrderByDescending(s => s.SharedAt)
            .ToListAsync();
    }

    public async Task<SharedRecipe> CreateAsync(SharedRecipe sharedRecipe)
    {
        _context.SharedRecipes.Add(sharedRecipe);
        await _context.SaveChangesAsync();
        return sharedRecipe;
    }

    public async Task UpdateAsync(SharedRecipe sharedRecipe)
    {
        _context.SharedRecipes.Update(sharedRecipe);
        await _context.SaveChangesAsync();
    }

    public async Task DeleteAsync(Guid id)
    {
        var sharedRecipe = await GetByIdAsync(id);
        if (sharedRecipe != null)
        {
            _context.SharedRecipes.Remove(sharedRecipe);
            await _context.SaveChangesAsync();
        }
    }
}
