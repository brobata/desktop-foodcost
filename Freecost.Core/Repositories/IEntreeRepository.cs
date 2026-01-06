using Freecost.Core.Models;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Freecost.Core.Repositories;

public interface IEntreeRepository
{
    Task<IEnumerable<Entree>> GetAllAsync(Guid locationId);
    Task<Entree?> GetByIdAsync(Guid id);
    Task<Entree> CreateAsync(Entree entree);
    Task<Entree> UpdateAsync(Entree entree);
    Task DeleteAsync(Guid id);
    Task<Entree?> GetByNameAsync(string name, Guid locationId);
}