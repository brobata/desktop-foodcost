using Dfc.Core.Models;

namespace Dfc.Core.Repositories;

public interface IImportMapRepository
{
    /// <summary>
    /// Get all user-saved import mappings for a location
    /// </summary>
    Task<List<ImportMap>> GetUserMapsAsync(Guid locationId);

    /// <summary>
    /// Get a specific import map by ID
    /// </summary>
    Task<ImportMap?> GetByIdAsync(Guid id);

    /// <summary>
    /// Add a new import mapping
    /// </summary>
    Task<ImportMap> AddAsync(ImportMap map);

    /// <summary>
    /// Update an existing import mapping
    /// </summary>
    Task<ImportMap> UpdateAsync(ImportMap map);

    /// <summary>
    /// Delete an import mapping
    /// </summary>
    Task DeleteAsync(Guid id);

    /// <summary>
    /// Find a saved mapping that matches the given headers
    /// </summary>
    Task<ImportMap?> FindMatchingMapAsync(List<string> headers, Guid locationId);

    /// <summary>
    /// Update the usage statistics for a mapping (last used, import count)
    /// </summary>
    Task UpdateUsageAsync(Guid mapId);
}
