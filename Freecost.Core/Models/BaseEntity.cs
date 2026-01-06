using System.ComponentModel.DataAnnotations;

namespace Freecost.Core.Models;

public abstract class BaseEntity
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime ModifiedAt { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// Row version for optimistic concurrency control.
    /// Entity Framework automatically manages this field.
    /// </summary>
    [Timestamp]
    public byte[]? RowVersion { get; set; }
}