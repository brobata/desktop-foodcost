using Dfc.Core.Enums;

namespace Dfc.Core.Models;

public class User : BaseEntity
{
    public string Email { get; set; } = string.Empty;
    public UserRole Role { get; set; }
    public Guid? LocationId { get; set; }
    public bool IsActive { get; set; } = true;

    // Navigation properties
    public Location? Location { get; set; }
}
