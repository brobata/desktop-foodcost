using Freecost.Core.Models;
using Freecost.Core.Repositories;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Freecost.Core.Services;

public class CommentService : ICommentService
{
    private readonly IRecipeCommentRepository _recipeCommentRepository;
    private readonly IEntreeCommentRepository _entreeCommentRepository;

    public CommentService(
        IRecipeCommentRepository recipeCommentRepository,
        IEntreeCommentRepository entreeCommentRepository)
    {
        _recipeCommentRepository = recipeCommentRepository;
        _entreeCommentRepository = entreeCommentRepository;
    }

    // Recipe Comments
    public async Task<RecipeComment> AddRecipeCommentAsync(
        Guid recipeId,
        Guid userId,
        string content,
        Guid? parentCommentId = null)
    {
        var comment = new RecipeComment
        {
            Id = Guid.NewGuid(),
            RecipeId = recipeId,
            UserId = userId,
            ParentCommentId = parentCommentId,
            Content = content,
            CommentedAt = DateTime.UtcNow,
            CreatedAt = DateTime.UtcNow,
            Mentions = ExtractMentions(content)
        };

        return await _recipeCommentRepository.CreateAsync(comment);
    }

    public async Task<RecipeComment> EditRecipeCommentAsync(Guid commentId, string newContent)
    {
        var comment = await _recipeCommentRepository.GetByIdAsync(commentId);
        if (comment == null)
        {
            throw new InvalidOperationException("Comment not found");
        }

        comment.Content = newContent;
        comment.EditedAt = DateTime.UtcNow;
        comment.IsEdited = true;
        comment.Mentions = ExtractMentions(newContent);

        await _recipeCommentRepository.UpdateAsync(comment);
        return comment;
    }

    public async Task DeleteRecipeCommentAsync(Guid commentId)
    {
        var comment = await _recipeCommentRepository.GetByIdAsync(commentId);
        if (comment == null)
        {
            throw new InvalidOperationException("Comment not found");
        }

        comment.IsDeleted = true;
        comment.Content = "[Comment deleted]";
        await _recipeCommentRepository.UpdateAsync(comment);
    }

    public async Task<List<RecipeComment>> GetRecipeCommentsAsync(Guid recipeId)
    {
        return await _recipeCommentRepository.GetByRecipeAsync(recipeId);
    }

    public async Task<List<RecipeComment>> GetRecipeCommentThreadAsync(Guid parentCommentId)
    {
        return await _recipeCommentRepository.GetThreadAsync(parentCommentId);
    }

    public async Task<RecipeComment?> GetRecipeCommentByIdAsync(Guid commentId)
    {
        return await _recipeCommentRepository.GetByIdAsync(commentId);
    }

    // Entree Comments
    public async Task<EntreeComment> AddEntreeCommentAsync(
        Guid entreeId,
        Guid userId,
        string content,
        Guid? parentCommentId = null)
    {
        var comment = new EntreeComment
        {
            Id = Guid.NewGuid(),
            EntreeId = entreeId,
            UserId = userId,
            ParentCommentId = parentCommentId,
            Content = content,
            CommentedAt = DateTime.UtcNow,
            CreatedAt = DateTime.UtcNow,
            Mentions = ExtractMentions(content)
        };

        return await _entreeCommentRepository.CreateAsync(comment);
    }

    public async Task<EntreeComment> EditEntreeCommentAsync(Guid commentId, string newContent)
    {
        var comment = await _entreeCommentRepository.GetByIdAsync(commentId);
        if (comment == null)
        {
            throw new InvalidOperationException("Comment not found");
        }

        comment.Content = newContent;
        comment.EditedAt = DateTime.UtcNow;
        comment.IsEdited = true;
        comment.Mentions = ExtractMentions(newContent);

        await _entreeCommentRepository.UpdateAsync(comment);
        return comment;
    }

    public async Task DeleteEntreeCommentAsync(Guid commentId)
    {
        var comment = await _entreeCommentRepository.GetByIdAsync(commentId);
        if (comment == null)
        {
            throw new InvalidOperationException("Comment not found");
        }

        comment.IsDeleted = true;
        comment.Content = "[Comment deleted]";
        await _entreeCommentRepository.UpdateAsync(comment);
    }

    public async Task<List<EntreeComment>> GetEntreeCommentsAsync(Guid entreeId)
    {
        return await _entreeCommentRepository.GetByEntreeAsync(entreeId);
    }

    public async Task<List<EntreeComment>> GetEntreeCommentThreadAsync(Guid parentCommentId)
    {
        return await _entreeCommentRepository.GetThreadAsync(parentCommentId);
    }

    public async Task<EntreeComment?> GetEntreeCommentByIdAsync(Guid commentId)
    {
        return await _entreeCommentRepository.GetByIdAsync(commentId);
    }

    // Mentions
    public async Task<List<RecipeComment>> GetCommentsMentioningUserAsync(Guid userId)
    {
        return await _recipeCommentRepository.GetMentioningUserAsync(userId);
    }

    public async Task<List<EntreeComment>> GetEntreeCommentsMentioningUserAsync(Guid userId)
    {
        return await _entreeCommentRepository.GetMentioningUserAsync(userId);
    }

    // Helper method to extract @mentions from content
    private List<string> ExtractMentions(string content)
    {
        var mentions = new List<string>();
        var regex = new Regex(@"@([a-zA-Z0-9_]+)");
        var matches = regex.Matches(content);

        foreach (Match match in matches)
        {
            mentions.Add(match.Groups[1].Value);
        }

        return mentions;
    }
}
