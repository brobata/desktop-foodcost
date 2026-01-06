using Freecost.Core.Models;
using Freecost.Core.Services;
using Freecost.Data.LocalDatabase;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace Freecost.Data.Services;

public class PhotoGalleryService : IPhotoGalleryService
{
    private readonly FreecostDbContext _context;
    private readonly string _photoDirectory;

    public PhotoGalleryService(FreecostDbContext context)
    {
        _context = context;

        _photoDirectory = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
            "Freecost",
            "Photos"
        );

        if (!Directory.Exists(_photoDirectory))
        {
            Directory.CreateDirectory(_photoDirectory);
        }
    }

    public async Task<Photo> AddPhotoAsync(Photo photo)
    {
        photo.CreatedAt = DateTime.UtcNow;
        photo.ModifiedAt = DateTime.UtcNow;

        // If this is marked as primary, unset other primary photos
        if (photo.IsPrimary)
        {
            if (photo.RecipeId.HasValue)
            {
                var existingPrimary = await _context.Photos
                    .Where(p => p.RecipeId == photo.RecipeId && p.IsPrimary)
                    .ToListAsync();

                foreach (var p in existingPrimary)
                {
                    p.IsPrimary = false;
                }
            }
            else if (photo.EntreeId.HasValue)
            {
                var existingPrimary = await _context.Photos
                    .Where(p => p.EntreeId == photo.EntreeId && p.IsPrimary)
                    .ToListAsync();

                foreach (var p in existingPrimary)
                {
                    p.IsPrimary = false;
                }
            }
        }

        _context.Photos.Add(photo);
        await _context.SaveChangesAsync();
        return photo;
    }

    public async Task<List<Photo>> GetPhotosForRecipeAsync(Guid recipeId)
    {
        return await _context.Photos
            .Where(p => p.RecipeId == recipeId)
            .OrderBy(p => p.Order)
            .ToListAsync();
    }

    public async Task<List<Photo>> GetPhotosForEntreeAsync(Guid entreeId)
    {
        return await _context.Photos
            .Where(p => p.EntreeId == entreeId)
            .OrderBy(p => p.Order)
            .ToListAsync();
    }

    public async Task<Photo?> GetPrimaryPhotoAsync(Guid? recipeId, Guid? entreeId)
    {
        if (recipeId.HasValue)
        {
            return await _context.Photos
                .Where(p => p.RecipeId == recipeId && p.IsPrimary)
                .FirstOrDefaultAsync();
        }
        else if (entreeId.HasValue)
        {
            return await _context.Photos
                .Where(p => p.EntreeId == entreeId && p.IsPrimary)
                .FirstOrDefaultAsync();
        }

        return null;
    }

    public async Task SetPrimaryPhotoAsync(Guid photoId)
    {
        var photo = await _context.Photos.FindAsync(photoId);
        if (photo == null) return;

        // Unset other primary photos for the same recipe/entree
        if (photo.RecipeId.HasValue)
        {
            var others = await _context.Photos
                .Where(p => p.RecipeId == photo.RecipeId && p.Id != photoId)
                .ToListAsync();

            foreach (var other in others)
            {
                other.IsPrimary = false;
            }
        }
        else if (photo.EntreeId.HasValue)
        {
            var others = await _context.Photos
                .Where(p => p.EntreeId == photo.EntreeId && p.Id != photoId)
                .ToListAsync();

            foreach (var other in others)
            {
                other.IsPrimary = false;
            }
        }

        photo.IsPrimary = true;
        await _context.SaveChangesAsync();
    }

    public async Task UpdatePhotoOrderAsync(List<Guid> photoIds)
    {
        for (int i = 0; i < photoIds.Count; i++)
        {
            var photo = await _context.Photos.FindAsync(photoIds[i]);
            if (photo != null)
            {
                photo.Order = i;
            }
        }

        await _context.SaveChangesAsync();
    }

    public async Task DeletePhotoAsync(Guid photoId)
    {
        var photo = await _context.Photos.FindAsync(photoId);
        if (photo != null)
        {
            // Delete physical file
            if (File.Exists(photo.FilePath))
            {
                try
                {
                    File.Delete(photo.FilePath);
                }
                catch
                {
                    // Ignore file deletion errors
                }
            }

            _context.Photos.Remove(photo);
            await _context.SaveChangesAsync();
        }
    }

    public async Task<string> SavePhotoFileAsync(byte[] imageData, string originalFileName)
    {
        var fileName = $"{Guid.NewGuid()}_{Path.GetFileName(originalFileName)}";
        var filePath = Path.Combine(_photoDirectory, fileName);

        await File.WriteAllBytesAsync(filePath, imageData);
        return filePath;
    }
}
