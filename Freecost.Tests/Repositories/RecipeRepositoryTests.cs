using System;
using System.Linq;
using System.Threading.Tasks;
using FluentAssertions;
using Freecost.Core.Enums;
using Freecost.Core.Models;
using Freecost.Data.Repositories;
using Freecost.Tests.Infrastructure;
using Xunit;

namespace Freecost.Tests.Repositories;

public class RecipeRepositoryTests : DatabaseTestBase
{
    private readonly RecipeRepository _repository;
    private readonly IngredientRepository _ingredientRepository;
    private readonly Guid _testLocationId = Guid.NewGuid();
    private Guid _testIngredientId;

    public RecipeRepositoryTests()
    {
        _repository = new RecipeRepository(Context);
        _ingredientRepository = new IngredientRepository(Context);

        // Create test location
        Context.Locations.Add(new Location
        {
            Id = _testLocationId,
            Name = "Test Location",
            CreatedAt = DateTime.UtcNow,
            ModifiedAt = DateTime.UtcNow
        });
        Context.SaveChanges();

        // Create test ingredient for recipes
        var ingredient = new Ingredient
        {
            Id = Guid.NewGuid(),
            Name = "Test Ingredient",
            CurrentPrice = 5.00m,
            CaseQuantity = 1.0m,
            Unit = UnitType.Pound,
            LocationId = _testLocationId,
            CreatedAt = DateTime.UtcNow,
            ModifiedAt = DateTime.UtcNow
        };
        Context.Ingredients.Add(ingredient);
        Context.SaveChanges();
        _testIngredientId = ingredient.Id;
    }

    #region CreateRecipeAsync Tests

    [Fact]
    public async Task CreateRecipeAsync_WithValidRecipe_ShouldAddToDatabase()
    {
        // Arrange
        var recipe = CreateTestRecipe("Chocolate Chip Cookies");

        // Act
        var result = await _repository.CreateRecipeAsync(recipe);

        // Assert
        result.Should().NotBeNull();
        result.Id.Should().NotBe(Guid.Empty);
        result.Name.Should().Be("Chocolate Chip Cookies");
        result.CreatedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(5));
        result.ModifiedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(5));
    }

    [Fact]
    public async Task CreateRecipeAsync_WithIngredients_ShouldAddIngredients()
    {
        // Arrange
        var recipe = CreateTestRecipe("Pasta");
        recipe.RecipeIngredients = new List<RecipeIngredient>
        {
            new RecipeIngredient
            {
                IngredientId = _testIngredientId,
                Quantity = 2.0m,
                Unit = UnitType.Pound
            }
        };

        // Act
        var result = await _repository.CreateRecipeAsync(recipe);

        // Assert
        result.RecipeIngredients.Should().HaveCount(1);
        result.RecipeIngredients.First().RecipeId.Should().Be(result.Id);
        result.RecipeIngredients.First().IngredientId.Should().Be(_testIngredientId);
        result.RecipeIngredients.First().Quantity.Should().Be(2.0m);
    }

    [Fact]
    public async Task CreateRecipeAsync_WithAllergens_ShouldAddAllergens()
    {
        // Arrange
        var recipe = CreateTestRecipe("Peanut Butter Cookies");
        var peanutAllergenId = Guid.Parse("66666666-6666-6666-6666-666666666666");

        recipe.RecipeAllergens = new List<RecipeAllergen>
        {
            new RecipeAllergen
            {
                AllergenId = peanutAllergenId,
                IsAutoDetected = true,
                IsEnabled = true
            }
        };

        // Act
        var result = await _repository.CreateRecipeAsync(recipe);

        // Assert
        result.RecipeAllergens.Should().HaveCount(1);
        result.RecipeAllergens.First().AllergenId.Should().Be(peanutAllergenId);
        result.RecipeAllergens.First().RecipeId.Should().Be(result.Id);
    }

    #endregion

    #region GetAllRecipesAsync Tests

    [Fact]
    public async Task GetAllRecipesAsync_WithMultipleRecipes_ShouldReturnAllForLocation()
    {
        // Arrange
        await _repository.CreateRecipeAsync(CreateTestRecipe("Recipe 1"));
        await _repository.CreateRecipeAsync(CreateTestRecipe("Recipe 2"));
        await _repository.CreateRecipeAsync(CreateTestRecipe("Recipe 3"));

        // Add recipe for different location (should not be returned)
        var otherLocationId = Guid.NewGuid();
        Context.Locations.Add(new Location
        {
            Id = otherLocationId,
            Name = "Other Location",
            CreatedAt = DateTime.UtcNow,
            ModifiedAt = DateTime.UtcNow
        });
        await Context.SaveChangesAsync();

        var otherRecipe = CreateTestRecipe("Other Recipe");
        otherRecipe.LocationId = otherLocationId;
        await _repository.CreateRecipeAsync(otherRecipe);

        // Act
        var results = await _repository.GetAllRecipesAsync(_testLocationId);

        // Assert
        results.Should().HaveCount(3);
        results.Should().AllSatisfy(r => r.LocationId.Should().Be(_testLocationId));
        results.Should().BeInAscendingOrder(r => r.Name);
    }

    [Fact]
    public async Task GetAllRecipesAsync_WithNoRecipes_ShouldReturnEmpty()
    {
        // Act
        var results = await _repository.GetAllRecipesAsync(_testLocationId);

        // Assert
        results.Should().BeEmpty();
    }

    [Fact]
    public async Task GetAllRecipesAsync_ShouldIncludeIngredients()
    {
        // Arrange
        var recipe = CreateTestRecipe("Test Recipe");
        recipe.RecipeIngredients = new List<RecipeIngredient>
        {
            new RecipeIngredient
            {
                IngredientId = _testIngredientId,
                Quantity = 1.0m,
                Unit = UnitType.Pound
            }
        };
        await _repository.CreateRecipeAsync(recipe);

        // Act
        var results = await _repository.GetAllRecipesAsync(_testLocationId);

        // Assert
        var firstRecipe = results.First();
        firstRecipe.RecipeIngredients.Should().HaveCount(1);
        firstRecipe.RecipeIngredients.First().Ingredient.Should().NotBeNull();
    }

    #endregion

    #region GetRecipeByIdAsync Tests

    [Fact]
    public async Task GetRecipeByIdAsync_WithExistingId_ShouldReturnRecipe()
    {
        // Arrange
        var created = await _repository.CreateRecipeAsync(CreateTestRecipe("Brownies"));

        // Act
        var result = await _repository.GetRecipeByIdAsync(created.Id);

        // Assert
        result.Should().NotBeNull();
        result!.Id.Should().Be(created.Id);
        result.Name.Should().Be("Brownies");
    }

    [Fact]
    public async Task GetRecipeByIdAsync_WithNonExistentId_ShouldReturnNull()
    {
        // Act
        var result = await _repository.GetRecipeByIdAsync(Guid.NewGuid());

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public async Task GetRecipeByIdAsync_ShouldIncludeIngredientsWithAllergens()
    {
        // Arrange
        var milkAllergenId = Guid.Parse("11111111-1111-1111-1111-111111111111");

        // Create ingredient with allergen
        var milkIngredient = new Ingredient
        {
            Id = Guid.NewGuid(),
            Name = "Milk",
            CurrentPrice = 3.00m,
            CaseQuantity = 1.0m,
            Unit = UnitType.Gallon,
            LocationId = _testLocationId,
            CreatedAt = DateTime.UtcNow,
            ModifiedAt = DateTime.UtcNow,
            IngredientAllergens = new List<IngredientAllergen>
            {
                new IngredientAllergen
                {
                    Id = Guid.NewGuid(),
                    AllergenId = milkAllergenId,
                    IsEnabled = true,
                    CreatedAt = DateTime.UtcNow,
                    ModifiedAt = DateTime.UtcNow
                }
            }
        };
        Context.Ingredients.Add(milkIngredient);
        await Context.SaveChangesAsync();

        var recipe = CreateTestRecipe("Custard");
        recipe.RecipeIngredients = new List<RecipeIngredient>
        {
            new RecipeIngredient
            {
                IngredientId = milkIngredient.Id,
                Quantity = 2.0m,
                Unit = UnitType.Cup
            }
        };
        var created = await _repository.CreateRecipeAsync(recipe);

        // Act
        var result = await _repository.GetRecipeByIdAsync(created.Id);

        // Assert
        result.Should().NotBeNull();
        result!.RecipeIngredients.Should().HaveCount(1);
        result.RecipeIngredients.First().Ingredient.Should().NotBeNull();
        result.RecipeIngredients.First().Ingredient!.IngredientAllergens.Should().HaveCount(1);
    }

    #endregion

    #region SearchRecipesAsync Tests

    [Fact]
    public async Task SearchRecipesAsync_WithMatchingName_ShouldReturnResults()
    {
        // Arrange
        await _repository.CreateRecipeAsync(CreateTestRecipe("Chocolate Cake"));
        await _repository.CreateRecipeAsync(CreateTestRecipe("Chocolate Mousse"));
        await _repository.CreateRecipeAsync(CreateTestRecipe("Vanilla Ice Cream"));

        // Act
        var results = await _repository.SearchRecipesAsync("Chocolate", _testLocationId);

        // Assert
        results.Should().HaveCount(2);
        results.Should().Contain(r => r.Name == "Chocolate Cake");
        results.Should().Contain(r => r.Name == "Chocolate Mousse");
    }

    [Fact]
    public async Task SearchRecipesAsync_WithMatchingDescription_ShouldReturnResults()
    {
        // Arrange
        var recipe1 = CreateTestRecipe("Recipe 1");
        recipe1.Description = "A delicious dessert";
        await _repository.CreateRecipeAsync(recipe1);

        var recipe2 = CreateTestRecipe("Recipe 2");
        recipe2.Description = "A savory main course";
        await _repository.CreateRecipeAsync(recipe2);

        // Act
        var results = await _repository.SearchRecipesAsync("dessert", _testLocationId);

        // Assert
        results.Should().HaveCount(1);
        results.First().Name.Should().Be("Recipe 1");
    }

    [Fact]
    public async Task SearchRecipesAsync_WithEmptySearchTerm_ShouldReturnAll()
    {
        // Arrange
        await _repository.CreateRecipeAsync(CreateTestRecipe("Recipe A"));
        await _repository.CreateRecipeAsync(CreateTestRecipe("Recipe B"));

        // Act
        var results = await _repository.SearchRecipesAsync("", _testLocationId);

        // Assert
        results.Should().HaveCount(2);
    }

    [Fact]
    public async Task SearchRecipesAsync_IsCaseInsensitive()
    {
        // Arrange
        await _repository.CreateRecipeAsync(CreateTestRecipe("BEEF STEW"));

        // Act
        var results = await _repository.SearchRecipesAsync("beef", _testLocationId);

        // Assert
        results.Should().HaveCount(1);
    }

    #endregion

    #region UpdateRecipeAsync Tests

    [Fact]
    public async Task UpdateRecipeAsync_WithModifiedProperties_ShouldUpdateRecipe()
    {
        // Arrange
        var recipe = await _repository.CreateRecipeAsync(CreateTestRecipe("Original Name"));

        recipe.Name = "Updated Name";
        recipe.Yield = 8;
        recipe.YieldUnit = "portions";
        recipe.Description = "Updated description";

        // Act
        var result = await _repository.UpdateRecipeAsync(recipe);

        // Assert
        result.Name.Should().Be("Updated Name");
        result.Yield.Should().Be(8);
        result.YieldUnit.Should().Be("portions");
        result.Description.Should().Be("Updated description");

        // Verify in database
        var fromDb = await _repository.GetRecipeByIdAsync(recipe.Id);
        fromDb!.Name.Should().Be("Updated Name");
    }

    [Fact]
    public async Task UpdateRecipeAsync_WithNewIngredients_ShouldReplaceIngredients()
    {
        // Arrange
        var recipe = CreateTestRecipe("Recipe");
        recipe.RecipeIngredients = new List<RecipeIngredient>
        {
            new RecipeIngredient
            {
                IngredientId = _testIngredientId,
                Quantity = 1.0m,
                Unit = UnitType.Pound
            }
        };
        var created = await _repository.CreateRecipeAsync(recipe);

        // Modify ingredients
        created.RecipeIngredients = new List<RecipeIngredient>
        {
            new RecipeIngredient
            {
                IngredientId = _testIngredientId,
                Quantity = 2.0m,
                Unit = UnitType.Ounce
            },
            new RecipeIngredient
            {
                IngredientId = _testIngredientId,
                Quantity = 3.0m,
                Unit = UnitType.Cup
            }
        };

        // Act
        var result = await _repository.UpdateRecipeAsync(created);

        // Assert
        result.RecipeIngredients.Should().HaveCount(2);
        result.RecipeIngredients.Should().Contain(ri => ri.Quantity == 2.0m);
        result.RecipeIngredients.Should().Contain(ri => ri.Quantity == 3.0m);
    }

    [Fact]
    public async Task UpdateRecipeAsync_WithNewAllergens_ShouldReplaceAllergens()
    {
        // Arrange
        var wheatAllergenId = Guid.Parse("77777777-7777-7777-7777-777777777777");
        var soyAllergenId = Guid.Parse("88888888-8888-8888-8888-888888888888");

        var recipe = CreateTestRecipe("Bread");
        recipe.RecipeAllergens = new List<RecipeAllergen>
        {
            new RecipeAllergen { AllergenId = wheatAllergenId, IsEnabled = true }
        };
        var created = await _repository.CreateRecipeAsync(recipe);

        // Modify allergens
        created.RecipeAllergens = new List<RecipeAllergen>
        {
            new RecipeAllergen { AllergenId = soyAllergenId, IsEnabled = true }
        };

        // Act
        var result = await _repository.UpdateRecipeAsync(created);

        // Assert
        result.RecipeAllergens.Should().HaveCount(1);
        result.RecipeAllergens.First().AllergenId.Should().Be(soyAllergenId);
    }

    #endregion

    #region DeleteRecipeAsync Tests

    [Fact]
    public async Task DeleteRecipeAsync_WithExistingRecipe_ShouldRemoveFromDatabase()
    {
        // Arrange
        var recipe = await _repository.CreateRecipeAsync(CreateTestRecipe("To Delete"));

        // Act
        await _repository.DeleteRecipeAsync(recipe.Id);

        // Assert
        var result = await _repository.GetRecipeByIdAsync(recipe.Id);
        result.Should().BeNull();
    }

    [Fact]
    public async Task DeleteRecipeAsync_WithNonExistentId_ShouldNotThrow()
    {
        // Act
        Func<Task> act = async () => await _repository.DeleteRecipeAsync(Guid.NewGuid());

        // Assert
        await act.Should().NotThrowAsync();
    }

    [Fact]
    public async Task DeleteRecipeAsync_ShouldCascadeDeleteIngredients()
    {
        // Arrange
        var recipe = CreateTestRecipe("Recipe with Ingredients");
        recipe.RecipeIngredients = new List<RecipeIngredient>
        {
            new RecipeIngredient
            {
                IngredientId = _testIngredientId,
                Quantity = 1.0m,
                Unit = UnitType.Pound
            }
        };
        var created = await _repository.CreateRecipeAsync(recipe);

        // Act
        await _repository.DeleteRecipeAsync(created.Id);

        // Assert
        var recipeIngredientCount = Context.RecipeIngredients.Count(ri => ri.RecipeId == created.Id);
        recipeIngredientCount.Should().Be(0);
    }

    #endregion

    #region RecipeExistsAsync Tests

    [Fact]
    public async Task RecipeExistsAsync_WithExistingName_ShouldReturnTrue()
    {
        // Arrange
        await _repository.CreateRecipeAsync(CreateTestRecipe("Existing Recipe"));

        // Act
        var exists = await _repository.RecipeExistsAsync("Existing Recipe", _testLocationId);

        // Assert
        exists.Should().BeTrue();
    }

    [Fact]
    public async Task RecipeExistsAsync_WithNonExistentName_ShouldReturnFalse()
    {
        // Act
        var exists = await _repository.RecipeExistsAsync("Non-existent", _testLocationId);

        // Assert
        exists.Should().BeFalse();
    }

    [Fact]
    public async Task RecipeExistsAsync_IsCaseInsensitive()
    {
        // Arrange
        await _repository.CreateRecipeAsync(CreateTestRecipe("Test Recipe"));

        // Act
        var exists = await _repository.RecipeExistsAsync("TEST RECIPE", _testLocationId);

        // Assert
        exists.Should().BeTrue();
    }

    [Fact]
    public async Task RecipeExistsAsync_WithExcludeId_ShouldExcludeSpecificRecipe()
    {
        // Arrange
        var recipe = await _repository.CreateRecipeAsync(CreateTestRecipe("Unique Recipe"));

        // Act
        var exists = await _repository.RecipeExistsAsync("Unique Recipe", _testLocationId, recipe.Id);

        // Assert
        exists.Should().BeFalse();
    }

    [Fact]
    public async Task RecipeExistsAsync_WithDifferentLocation_ShouldReturnFalse()
    {
        // Arrange
        await _repository.CreateRecipeAsync(CreateTestRecipe("Location Recipe"));
        var otherLocationId = Guid.NewGuid();

        // Act
        var exists = await _repository.RecipeExistsAsync("Location Recipe", otherLocationId);

        // Assert
        exists.Should().BeFalse();
    }

    #endregion

    #region Helper Methods

    private Recipe CreateTestRecipe(string name)
    {
        return new Recipe
        {
            Name = name,
            Description = "Test description",
            Instructions = "Test instructions",
            Yield = 4,
            YieldUnit = "servings",
            PrepTimeMinutes = 30,
            Category = "Main Dish",
            Difficulty = DifficultyLevel.Medium,
            LocationId = _testLocationId
        };
    }

    #endregion
}
