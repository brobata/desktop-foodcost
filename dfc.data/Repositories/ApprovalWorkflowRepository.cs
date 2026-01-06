using Dfc.Core.Models;
using Dfc.Core.Repositories;
using Dfc.Data.LocalDatabase;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Dfc.Data.Repositories;

public class ApprovalWorkflowRepository : IApprovalWorkflowRepository
{
    private readonly DfcDbContext _context;

    public ApprovalWorkflowRepository(DfcDbContext context)
    {
        _context = context;
    }

    public async Task<ApprovalWorkflow?> GetByIdAsync(Guid id)
    {
        return await _context.ApprovalWorkflows
            .Include(w => w.SubmittedByUser)
            .Include(w => w.ReviewedByUser)
            .Include(w => w.Comments)
                .ThenInclude(c => c.User)
            .FirstOrDefaultAsync(w => w.Id == id);
    }

    public async Task<ApprovalWorkflow?> GetByEntityAsync(string entityType, Guid entityId)
    {
        return await _context.ApprovalWorkflows
            .Include(w => w.SubmittedByUser)
            .Include(w => w.ReviewedByUser)
            .Include(w => w.Comments)
                .ThenInclude(c => c.User)
            .Where(w => w.EntityType == entityType && w.EntityId == entityId)
            .OrderByDescending(w => w.SubmittedAt)
            .FirstOrDefaultAsync();
    }

    public async Task<List<ApprovalWorkflow>> GetByStatusAsync(ApprovalStatus status)
    {
        return await _context.ApprovalWorkflows
            .Include(w => w.SubmittedByUser)
            .Include(w => w.ReviewedByUser)
            .Include(w => w.Comments)
            .Where(w => w.Status == status)
            .OrderByDescending(w => w.Priority)
            .ThenByDescending(w => w.SubmittedAt)
            .ToListAsync();
    }

    public async Task<List<ApprovalWorkflow>> GetBySubmittedUserAsync(Guid userId)
    {
        return await _context.ApprovalWorkflows
            .Include(w => w.SubmittedByUser)
            .Include(w => w.ReviewedByUser)
            .Include(w => w.Comments)
            .Where(w => w.SubmittedByUserId == userId)
            .OrderByDescending(w => w.SubmittedAt)
            .ToListAsync();
    }

    public async Task<List<ApprovalWorkflow>> GetByReviewedUserAsync(Guid userId)
    {
        return await _context.ApprovalWorkflows
            .Include(w => w.SubmittedByUser)
            .Include(w => w.ReviewedByUser)
            .Include(w => w.Comments)
            .Where(w => w.ReviewedByUserId == userId)
            .OrderByDescending(w => w.ReviewedAt)
            .ToListAsync();
    }

    public async Task<ApprovalWorkflow> CreateAsync(ApprovalWorkflow workflow)
    {
        _context.ApprovalWorkflows.Add(workflow);
        await _context.SaveChangesAsync();
        return workflow;
    }

    public async Task UpdateAsync(ApprovalWorkflow workflow)
    {
        _context.ApprovalWorkflows.Update(workflow);
        await _context.SaveChangesAsync();
    }

    public async Task DeleteAsync(Guid id)
    {
        var workflow = await GetByIdAsync(id);
        if (workflow != null)
        {
            _context.ApprovalWorkflows.Remove(workflow);
            await _context.SaveChangesAsync();
        }
    }
}
