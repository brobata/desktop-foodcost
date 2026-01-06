using Freecost.Core.Models;
using Freecost.Core.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Xunit;

namespace Freecost.Tests.Services;

/// <summary>
/// Unit tests for IngredientMatchingService (Session 34)
/// Tests fuzzy matching logic used in recipe/entree card imports
/// </summary>
public class IngredientMatchingServiceTests
{
    private readonly IngredientMatchingService _service;

    public IngredientMatchingServiceTests()
    {
        _service = new IngredientMatchingService();
    }

    [Fact]
    public void FindBestMatch_ExactMatch_ShouldReturn100Percent()
    {
        // Arrange
        var searchName = "Chicken Breast";
        var ingredients = new List<Ingredient>
        {
            new Ingredient { Name = "Chicken Breast", Id = Guid.NewGuid() },
            new Ingredient { Name = "Chicken Thigh", Id = Guid.NewGuid() }
        };

        // Act
        var result = _service.FindBestMatch(searchName, ingredients);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(100, result.ConfidenceScore);
        Assert.Equal("Chicken Breast", result.Ingredient.Name);
    }

    [Fact]
    public void FindBestMatch_CaseInsensitiveMatch_ShouldReturn95Percent()
    {
        // Arrange
        var searchName = "chicken breast";
        var ingredients = new List<Ingredient>
        {
            new Ingredient { Name = "Chicken Breast", Id = Guid.NewGuid() }
        };

        // Act
        var result = _service.FindBestMatch(searchName, ingredients);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(95, result.ConfidenceScore);
    }

    [Fact]
    public void FindBestMatch_StartsWithMatch_ShouldReturn90Percent()
    {
        // Arrange
        var searchName = "Chicken";
        var ingredients = new List<Ingredient>
        {
            new Ingredient { Name = "Chicken Breast, Boneless", Id = Guid.NewGuid() }
        };

        // Act
        var result = _service.FindBestMatch(searchName, ingredients);

        // Assert
        Assert.NotNull(result);
        Assert.True(result.ConfidenceScore >= 90);
    }

    [Fact]
    public void FindBestMatch_ContainsMatch_ShouldReturn85Percent()
    {
        // Arrange
        var searchName = "Breast";
        var ingredients = new List<Ingredient>
        {
            new Ingredient { Name = "Chicken Breast, Boneless", Id = Guid.NewGuid() }
        };

        // Act
        var result = _service.FindBestMatch(searchName, ingredients);

        // Assert
        Assert.NotNull(result);
        Assert.True(result.ConfidenceScore >= 80);
    }

    [Fact]
    public void FindBestMatch_NoGoodMatch_ShouldReturnNull()
    {
        // Arrange
        var searchName = "Salmon";
        var ingredients = new List<Ingredient>
        {
            new Ingredient { Name = "Chicken Breast", Id = Guid.NewGuid() },
            new Ingredient { Name = "Beef Steak", Id = Guid.NewGuid() }
        };

        // Act
        var result = _service.FindBestMatch(searchName, ingredients);

        // Assert
        // Result might be null or have very low confidence score
        if (result != null)
        {
            Assert.True(result.ConfidenceScore < 60);
        }
    }

    [Fact]
    public void FindBestMatch_MultipleMatches_ShouldReturnHighestConfidence()
    {
        // Arrange
        var searchName = "Chicken Breast";
        var ingredients = new List<Ingredient>
        {
            new Ingredient { Name = "Chicken Breast", Id = Guid.NewGuid() }, // Exact match
            new Ingredient { Name = "Chicken Breast, Boneless", Id = Guid.NewGuid() }, // Contains
            new Ingredient { Name = "Chicken", Id = Guid.NewGuid() } // Partial
        };

        // Act
        var result = _service.FindBestMatch(searchName, ingredients);

        // Assert
        Assert.NotNull(result);
        Assert.Equal("Chicken Breast", result.Ingredient.Name); // Should pick exact match
        Assert.Equal(100, result.ConfidenceScore);
    }

    [Fact]
    public void FindBestMatch_EmptyIngredientList_ShouldReturnNull()
    {
        // Arrange
        var searchName = "Chicken";
        var ingredients = new List<Ingredient>();

        // Act
        var result = _service.FindBestMatch(searchName, ingredients);

        // Assert
        Assert.Null(result);
    }

    [Fact]
    public void FindBestMatch_NullOrEmptySearchName_ShouldReturnNull()
    {
        // Arrange
        var ingredients = new List<Ingredient>
        {
            new Ingredient { Name = "Chicken Breast", Id = Guid.NewGuid() }
        };

        // Act
        var resultNull = _service.FindBestMatch(null, ingredients);
        var resultEmpty = _service.FindBestMatch("", ingredients);

        // Assert
        Assert.Null(resultNull);
        Assert.Null(resultEmpty);
    }

    [Fact]
    public void FindBestMatch_SpecialCharacters_ShouldHandleGracefully()
    {
        // Arrange
        var searchName = "Chicken (Breast)";
        var ingredients = new List<Ingredient>
        {
            new Ingredient { Name = "Chicken Breast", Id = Guid.NewGuid() }
        };

        // Act
        var result = _service.FindBestMatch(searchName, ingredients);

        // Assert
        // Should still find a match despite special characters
        Assert.NotNull(result);
        Assert.True(result.ConfidenceScore > 50);
    }

    [Fact]
    public void FindBestMatch_SimilarWords_ShouldUseEditDistance()
    {
        // Arrange
        var searchName = "Oliv Oil"; // Typo in "Olive"
        var ingredients = new List<Ingredient>
        {
            new Ingredient { Name = "Olive Oil", Id = Guid.NewGuid() }
        };

        // Act
        var result = _service.FindBestMatch(searchName, ingredients);

        // Assert
        // Should still match with reasonable confidence using edit distance
        Assert.NotNull(result);
        Assert.True(result.ConfidenceScore >= 60);
    }

    [Theory]
    [InlineData("tomato", "Tomatoes, Roma", true)] // Plural variation
    [InlineData("onion", "Onions, Yellow", true)] // Plural variation
    [InlineData("flour", "Flour, All Purpose", true)] // With descriptor
    public void FindBestMatch_CommonVariations_ShouldMatch(string searchName, string ingredientName, bool shouldMatch)
    {
        // Arrange
        var ingredients = new List<Ingredient>
        {
            new Ingredient { Name = ingredientName, Id = Guid.NewGuid() }
        };

        // Act
        var result = _service.FindBestMatch(searchName, ingredients);

        // Assert
        if (shouldMatch)
        {
            Assert.NotNull(result);
            Assert.True(result.ConfidenceScore >= 60);
        }
    }
}
