namespace Dfc.Core.Models;

public class Location : BaseEntity
{
    public string Name { get; set; } = string.Empty;
    public string? Address { get; set; }
    public string? Phone { get; set; }
    public bool IsActive { get; set; } = true;

    /// <summary>
    /// Optional user ID associated with this location
    /// </summary>
    public string? UserId { get; set; }

    // Navigation properties
    public ICollection<Ingredient> Ingredients { get; set; } = new List<Ingredient>();
    public ICollection<Recipe> Recipes { get; set; } = new List<Recipe>();
    public ICollection<Entree> Entrees { get; set; } = new List<Entree>();
    public ICollection<User> Users { get; set; } = new List<User>();
    public ICollection<LocationUser> LocationUsers { get; set; } = new List<LocationUser>();
}
