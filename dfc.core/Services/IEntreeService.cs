using Dfc.Core.Models;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Dfc.Core.Services;

public interface IEntreeService
{
    Task<List<Entree>> GetAllEntreesAsync(Guid locationId);
    Task<Entree?> GetEntreeByIdAsync(Guid id);
    Task<Entree> CreateEntreeAsync(Entree entree);
    Task<Entree> UpdateEntreeAsync(Entree entree);
    Task DeleteEntreeAsync(Guid id);
    Task<decimal> CalculateEntreeCostAsync(Entree entree);
}