using Dfc.Core.Models;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Dfc.Core.Repositories;

public interface IEntreeCommentRepository
{
    Task<EntreeComment?> GetByIdAsync(Guid id);
    Task<List<EntreeComment>> GetByEntreeAsync(Guid entreeId);
    Task<List<EntreeComment>> GetThreadAsync(Guid parentCommentId);
    Task<List<EntreeComment>> GetByUserAsync(Guid userId);
    Task<List<EntreeComment>> GetMentioningUserAsync(Guid userId);
    Task<EntreeComment> CreateAsync(EntreeComment comment);
    Task UpdateAsync(EntreeComment comment);
    Task DeleteAsync(Guid id);
}
