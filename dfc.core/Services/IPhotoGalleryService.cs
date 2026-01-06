using Dfc.Core.Models;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Dfc.Core.Services;

public interface IPhotoGalleryService
{
    Task<Photo> AddPhotoAsync(Photo photo);
    Task<List<Photo>> GetPhotosForRecipeAsync(Guid recipeId);
    Task<List<Photo>> GetPhotosForEntreeAsync(Guid entreeId);
    Task<Photo?> GetPrimaryPhotoAsync(Guid? recipeId, Guid? entreeId);
    Task SetPrimaryPhotoAsync(Guid photoId);
    Task UpdatePhotoOrderAsync(List<Guid> photoIds); // Reorder photos
    Task DeletePhotoAsync(Guid photoId);
    Task<string> SavePhotoFileAsync(byte[] imageData, string originalFileName);
}
