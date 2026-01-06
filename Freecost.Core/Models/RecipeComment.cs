using System;
using System.Collections.Generic;

namespace Freecost.Core.Models;

public class RecipeComment : BaseEntity
{
    public Guid RecipeId { get; set; }
    public Guid UserId { get; set; }
    public Guid? ParentCommentId { get; set; } // For threaded replies
    public string Content { get; set; } = string.Empty;
    public DateTime CommentedAt { get; set; } = DateTime.UtcNow;
    public DateTime? EditedAt { get; set; }
    public bool IsEdited { get; set; }
    public bool IsDeleted { get; set; }
    public List<string> Mentions { get; set; } = new(); // User IDs mentioned in comment

    // Navigation
    public Recipe? Recipe { get; set; }
    public User? User { get; set; }
    public RecipeComment? ParentComment { get; set; }
    public List<RecipeComment> Replies { get; set; } = new();
}

public class EntreeComment : BaseEntity
{
    public Guid EntreeId { get; set; }
    public Guid UserId { get; set; }
    public Guid? ParentCommentId { get; set; }
    public string Content { get; set; } = string.Empty;
    public DateTime CommentedAt { get; set; } = DateTime.UtcNow;
    public DateTime? EditedAt { get; set; }
    public bool IsEdited { get; set; }
    public bool IsDeleted { get; set; }
    public List<string> Mentions { get; set; } = new();

    // Navigation
    public Entree? Entree { get; set; }
    public User? User { get; set; }
    public EntreeComment? ParentComment { get; set; }
    public List<EntreeComment> Replies { get; set; } = new();
}
