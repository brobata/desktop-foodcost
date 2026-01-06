namespace Freecost.Core.Enums;

/// <summary>
/// Represents the type of modification made to a local entity
/// </summary>
public enum ModificationType
{
    /// <summary>
    /// A new entity was created locally
    /// </summary>
    Create = 0,

    /// <summary>
    /// An existing entity was updated locally
    /// </summary>
    Update = 1,

    /// <summary>
    /// An entity was deleted locally
    /// </summary>
    Delete = 2
}
