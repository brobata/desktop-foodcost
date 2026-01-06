using System;

namespace Freecost.Core.Models;

public class SharedRecipe : BaseEntity
{
    public Guid RecipeId { get; set; }
    public Guid SharedByUserId { get; set; }
    public Guid SharedWithUserId { get; set; }
    public SharePermission Permission { get; set; } = SharePermission.View;
    public DateTime SharedAt { get; set; } = DateTime.UtcNow;
    public DateTime? ExpiresAt { get; set; }
    public bool IsActive { get; set; } = true;
    public string? Message { get; set; }

    // Navigation
    public Recipe? Recipe { get; set; }
    public User? SharedByUser { get; set; }
    public User? SharedWithUser { get; set; }
}

public enum SharePermission
{
    View = 0,
    Edit = 1,
    FullControl = 2
}
