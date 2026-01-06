using System;
using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;
using SkiaSharp;

namespace Dfc.Core.Services;

/// <summary>
/// Local-only photo service that stores photos on the local filesystem.
/// No cloud sync - photos are stored in %APPDATA%/Desktop Food Cost/Photos
/// </summary>
public class LocalPhotoService : IPhotoService
{
    private readonly string _basePhotosPath;
    private readonly string _entreePhotosPath;
    private readonly string _recipePhotosPath;

    public LocalPhotoService()
    {
        _basePhotosPath = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
            "Desktop Food Cost",
            "Photos"
        );

        _entreePhotosPath = Path.Combine(_basePhotosPath, "EntreePhotos");
        _recipePhotosPath = Path.Combine(_basePhotosPath, "RecipePhotos");

        // Ensure directories exist
        Directory.CreateDirectory(_entreePhotosPath);
        Directory.CreateDirectory(_recipePhotosPath);
    }

    public async Task<string> SaveEntreePhotoAsync(string sourceFilePath)
    {
        return await SavePhotoAsync(sourceFilePath, _entreePhotosPath);
    }

    public async Task<string> SaveRecipePhotoAsync(string sourceFilePath)
    {
        return await SavePhotoAsync(sourceFilePath, _recipePhotosPath);
    }

    private async Task<string> SavePhotoAsync(string sourceFilePath, string destinationDirectory)
    {
        if (string.IsNullOrEmpty(sourceFilePath) || !File.Exists(sourceFilePath))
            throw new FileNotFoundException("Source photo file not found", sourceFilePath);

        // Validate file type
        var extension = Path.GetExtension(sourceFilePath).ToLowerInvariant();
        if (extension != ".jpg" && extension != ".jpeg" && extension != ".png")
            throw new InvalidOperationException("Only JPG and PNG files are supported");

        try
        {
            // Optimize the image first (resize/compress if needed)
            var optimizedPath = await OptimizeImageAsync(sourceFilePath);

            // Generate unique filename
            var fileName = $"{Guid.NewGuid()}{extension}";
            var destinationPath = Path.Combine(destinationDirectory, fileName);

            // Copy the optimized file to destination
            if (optimizedPath != sourceFilePath)
            {
                File.Move(optimizedPath, destinationPath);
            }
            else
            {
                File.Copy(sourceFilePath, destinationPath);
            }

            Debug.WriteLine($"[LocalPhotoService] Photo saved locally: {destinationPath}");

            // Return the local path
            return destinationPath;
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"[LocalPhotoService] Failed to save photo: {ex.Message}");
            throw;
        }
    }

    public async Task DeletePhotoAsync(string photoUrl)
    {
        if (string.IsNullOrEmpty(photoUrl))
            return;

        var fullPath = GetPhotoFullPath(photoUrl);

        if (File.Exists(fullPath))
        {
            await Task.Run(() => File.Delete(fullPath));
            Debug.WriteLine($"[LocalPhotoService] Photo deleted: {fullPath}");
        }
    }

    public string GetPhotoFullPath(string photoUrl)
    {
        if (string.IsNullOrEmpty(photoUrl))
            return string.Empty;

        // If it's already a full path, return it
        if (Path.IsPathRooted(photoUrl))
            return photoUrl;

        // Try entree photos first
        var entreePath = Path.Combine(_entreePhotosPath, photoUrl);
        if (File.Exists(entreePath))
            return entreePath;

        // Try recipe photos
        var recipePath = Path.Combine(_recipePhotosPath, photoUrl);
        if (File.Exists(recipePath))
            return recipePath;

        // Default to entree photos directory for backwards compatibility
        return entreePath;
    }

    public bool PhotoExists(string photoUrl)
    {
        if (string.IsNullOrEmpty(photoUrl))
            return false;

        var fullPath = GetPhotoFullPath(photoUrl);
        return File.Exists(fullPath);
    }

    public async Task<string> GetThumbnailPathAsync(string photoUrl)
    {
        if (string.IsNullOrEmpty(photoUrl))
            return string.Empty;

        var fullPath = GetPhotoFullPath(photoUrl);
        if (!File.Exists(fullPath))
            return string.Empty;

        // Create thumbnails directory
        var thumbnailsPath = Path.Combine(_basePhotosPath, "Thumbnails");
        Directory.CreateDirectory(thumbnailsPath);

        var thumbnailFileName = Path.GetFileNameWithoutExtension(Path.GetFileName(photoUrl)) + "_thumb.jpg";
        var thumbnailPath = Path.Combine(thumbnailsPath, thumbnailFileName);

        // If thumbnail doesn't exist, create it
        if (!File.Exists(thumbnailPath))
        {
            await Task.Run(() => CreateThumbnail(fullPath, thumbnailPath, 300, 300));
        }

        return thumbnailPath;
    }

    public async Task<string> OptimizeImageAsync(string sourceFilePath, int maxWidth = 1920, int maxHeight = 1080, int quality = 85)
    {
        if (string.IsNullOrEmpty(sourceFilePath) || !File.Exists(sourceFilePath))
            throw new FileNotFoundException("Source photo file not found", sourceFilePath);

        return await Task.Run(() =>
        {
            using var inputStream = File.OpenRead(sourceFilePath);
            using var original = SKBitmap.Decode(inputStream);

            if (original == null)
                throw new InvalidOperationException("Failed to decode image");

            // Calculate new dimensions maintaining aspect ratio
            var (newWidth, newHeight) = CalculateNewDimensions(original.Width, original.Height, maxWidth, maxHeight);

            // Check if resizing is needed
            var needsResize = original.Width > maxWidth || original.Height > maxHeight;
            var originalSize = new FileInfo(sourceFilePath).Length;

            // If image is already small enough and file size is reasonable, just return it
            if (!needsResize && originalSize < 2 * 1024 * 1024) // 2MB threshold
            {
                return sourceFilePath;
            }

            // Create optimized version
            var optimizedPath = Path.Combine(Path.GetTempPath(), $"optimized_{Guid.NewGuid()}.jpg");

            using var resized = needsResize ? original.Resize(new SKImageInfo(newWidth, newHeight), SKSamplingOptions.Default) : original;
            if (resized == null)
                throw new InvalidOperationException("Failed to resize image");

            using var image = SKImage.FromBitmap(resized);
            using var data = image.Encode(SKEncodedImageFormat.Jpeg, quality);
            using var outputStream = File.OpenWrite(optimizedPath);
            data.SaveTo(outputStream);

            return optimizedPath;
        });
    }

    private void CreateThumbnail(string sourcePath, string destinationPath, int maxWidth, int maxHeight)
    {
        using var inputStream = File.OpenRead(sourcePath);
        using var original = SKBitmap.Decode(inputStream);

        if (original == null)
            return;

        var (newWidth, newHeight) = CalculateNewDimensions(original.Width, original.Height, maxWidth, maxHeight);

        using var resized = original.Resize(new SKImageInfo(newWidth, newHeight), SKSamplingOptions.Default);
        if (resized == null)
            return;

        using var image = SKImage.FromBitmap(resized);
        using var data = image.Encode(SKEncodedImageFormat.Jpeg, 80); // Lower quality for thumbnails
        using var outputStream = File.OpenWrite(destinationPath);
        data.SaveTo(outputStream);
    }

    private (int width, int height) CalculateNewDimensions(int originalWidth, int originalHeight, int maxWidth, int maxHeight)
    {
        if (originalWidth <= maxWidth && originalHeight <= maxHeight)
            return (originalWidth, originalHeight);

        var ratioX = (double)maxWidth / originalWidth;
        var ratioY = (double)maxHeight / originalHeight;
        var ratio = Math.Min(ratioX, ratioY);

        return ((int)(originalWidth * ratio), (int)(originalHeight * ratio));
    }
}
