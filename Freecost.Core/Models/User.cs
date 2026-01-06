using Freecost.Core.Enums;

namespace Freecost.Core.Models;

public class User : BaseEntity
{
    public string Email { get; set; } = string.Empty;

    /// <summary>
    /// Supabase Auth UID (user_id from Supabase Auth/Gotrue)
    /// Used for syncing data to/from Supabase PostgreSQL with RLS policies
    /// </summary>
    public string? SupabaseAuthUid { get; set; }

    public UserRole Role { get; set; }
    public Guid? LocationId { get; set; }
    public bool IsActive { get; set; } = true;

    // Navigation properties
    public Location? Location { get; set; }
}