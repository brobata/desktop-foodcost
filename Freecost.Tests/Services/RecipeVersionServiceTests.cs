using Freecost.Core.Models;
using Freecost.Core.Repositories;
using Freecost.Core.Services;
using Freecost.Data.Services;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Xunit;

namespace Freecost.Tests.Services;

/// <summary>
/// Unit tests for RecipeVersionService (Session 34)
/// Tests version control, comparison, and audit trail functionality
/// </summary>
public class RecipeVersionServiceTests
{
    private readonly Mock<IRecipeVersionRepository> _mockRepo;
    private readonly RecipeVersionService _service;

    public RecipeVersionServiceTests()
    {
        _mockRepo = new Mock<IRecipeVersionRepository>();
        _service = new RecipeVersionService(_mockRepo.Object);
    }

    [Fact]
    public async Task CreateVersion_ShouldCreateNewVersionWithIncrementedNumber()
    {
        // Arrange
        var recipeId = Guid.NewGuid();
        var recipe = CreateTestRecipe(recipeId);
        var existingVersions = new List<RecipeVersion>
        {
            new RecipeVersion { RecipeId = recipeId, VersionNumber = 1 },
            new RecipeVersion { RecipeId = recipeId, VersionNumber = 2 }
        };

        _mockRepo.Setup(r => r.GetVersionsByRecipeIdAsync(recipeId))
            .ReturnsAsync(existingVersions);
        _mockRepo.Setup(r => r.CreateVersionAsync(It.IsAny<RecipeVersion>()))
            .ReturnsAsync((RecipeVersion v) => v);

        // Act
        var result = await _service.CreateVersionAsync(recipe, "Test User", "Test change");

        // Assert
        Assert.NotNull(result);
        Assert.Equal(3, result.VersionNumber); // Should be next version
        Assert.Equal(recipeId, result.RecipeId);
        Assert.Equal("Test User", result.CreatedBy);
        Assert.Equal("Test change", result.ChangeDescription);
        _mockRepo.Verify(r => r.CreateVersionAsync(It.IsAny<RecipeVersion>()), Times.Once);
    }

    [Fact]
    public async Task CreateVersion_FirstVersion_ShouldStartAt1()
    {
        // Arrange
        var recipeId = Guid.NewGuid();
        var recipe = CreateTestRecipe(recipeId);

        _mockRepo.Setup(r => r.GetVersionsByRecipeIdAsync(recipeId))
            .ReturnsAsync(new List<RecipeVersion>()); // No existing versions
        _mockRepo.Setup(r => r.CreateVersionAsync(It.IsAny<RecipeVersion>()))
            .ReturnsAsync((RecipeVersion v) => v);

        // Act
        var result = await _service.CreateVersionAsync(recipe, "Test User", "Initial version");

        // Assert
        Assert.NotNull(result);
        Assert.Equal(1, result.VersionNumber);
    }

    [Fact]
    public async Task GetVersionsByRecipeId_ShouldReturnOrderedByVersionNumber()
    {
        // Arrange
        var recipeId = Guid.NewGuid();
        var versions = new List<RecipeVersion>
        {
            new RecipeVersion { RecipeId = recipeId, VersionNumber = 3 },
            new RecipeVersion { RecipeId = recipeId, VersionNumber = 1 },
            new RecipeVersion { RecipeId = recipeId, VersionNumber = 2 }
        };

        _mockRepo.Setup(r => r.GetVersionsByRecipeIdAsync(recipeId))
            .ReturnsAsync(versions);

        // Act
        var result = await _service.GetVersionsByRecipeIdAsync(recipeId);

        // Assert
        Assert.Equal(3, result.Count);
        Assert.Equal(3, result[0].VersionNumber);
        Assert.Equal(2, result[1].VersionNumber);
        Assert.Equal(1, result[2].VersionNumber);
    }

    [Fact]
    public async Task CompareVersions_ShouldIdentifyFieldChanges()
    {
        // Arrange
        var recipeId = Guid.NewGuid();
        var version1 = CreateTestVersion(recipeId, 1, "Marinara Sauce", 16, "cups");
        var version2 = CreateTestVersion(recipeId, 2, "Updated Marinara", 20, "cups");

        _mockRepo.Setup(r => r.GetVersionByNumberAsync(recipeId, 1))
            .ReturnsAsync(version1);
        _mockRepo.Setup(r => r.GetVersionByNumberAsync(recipeId, 2))
            .ReturnsAsync(version2);

        // Act
        var differences = await _service.CompareVersionsAsync(recipeId, 1, 2);

        // Assert
        Assert.NotEmpty(differences);
        Assert.Contains(differences, d => d.FieldName == "Name");
        Assert.Contains(differences, d => d.FieldName == "Yield");
    }

    [Fact]
    public async Task GetRecentActivity_ShouldReturnLimitedResults()
    {
        // Arrange
        var locationId = Guid.NewGuid();
        var versions = Enumerable.Range(1, 50)
            .Select(i => new RecipeVersion
            {
                RecipeId = Guid.NewGuid(),
                VersionNumber = 1,
                CreatedAt = DateTime.UtcNow.AddMinutes(-i)
            })
            .ToList();

        _mockRepo.Setup(r => r.GetRecentVersionsAsync(locationId, 25))
            .ReturnsAsync(versions.Take(25).ToList());

        // Act
        var result = await _service.GetRecentActivityAsync(locationId, 25);

        // Assert
        Assert.Equal(25, result.Count);
    }

    [Fact]
    public async Task CreateVersion_ShouldSerializeAllRecipeData()
    {
        // Arrange
        var recipeId = Guid.NewGuid();
        var recipe = CreateTestRecipe(recipeId);
        recipe.RecipeIngredients.Add(new RecipeIngredient
        {
            Id = Guid.NewGuid(),
            IngredientId = Guid.NewGuid(),
            Quantity = 2,
            Unit = Core.Enums.UnitType.Cup
        });

        _mockRepo.Setup(r => r.GetVersionsByRecipeIdAsync(recipeId))
            .ReturnsAsync(new List<RecipeVersion>());
        _mockRepo.Setup(r => r.CreateVersionAsync(It.IsAny<RecipeVersion>()))
            .ReturnsAsync((RecipeVersion v) => v);

        // Act
        var result = await _service.CreateVersionAsync(recipe, "Test User", "Added ingredients");

        // Assert
        Assert.NotNull(result.RecipeDataJson);
        Assert.Contains("\"Name\"", result.RecipeDataJson); // Should contain serialized recipe data
        _mockRepo.Verify(r => r.CreateVersionAsync(It.Is<RecipeVersion>(
            v => !string.IsNullOrEmpty(v.RecipeDataJson)
        )), Times.Once);
    }

    // Helper methods
    private Recipe CreateTestRecipe(Guid id)
    {
        return new Recipe
        {
            Id = id,
            Name = "Test Recipe",
            Description = "Test Description",
            Yield = 10,
            YieldUnit = "servings",
            LocationId = Guid.NewGuid(),
            RecipeIngredients = new List<RecipeIngredient>(),
            CreatedAt = DateTime.UtcNow,
            ModifiedAt = DateTime.UtcNow
        };
    }

    private RecipeVersion CreateTestVersion(Guid recipeId, int versionNumber, string name, decimal yield, string yieldUnit)
    {
        var recipe = new Recipe
        {
            Id = recipeId,
            Name = name,
            Yield = yield,
            YieldUnit = yieldUnit,
            LocationId = Guid.NewGuid()
        };

        return new RecipeVersion
        {
            Id = Guid.NewGuid(),
            RecipeId = recipeId,
            VersionNumber = versionNumber,
            RecipeDataJson = System.Text.Json.JsonSerializer.Serialize(recipe),
            CreatedBy = "Test User",
            CreatedAt = DateTime.UtcNow
        };
    }
}
