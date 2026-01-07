using FluentAssertions;
using Dfc.Core.Constants;
using Dfc.Core.Enums;
using Dfc.Core.Models;
using Dfc.Core.Services;
using Xunit;

namespace Dfc.Tests.Services;

public class ValidationServiceTests
{
    private readonly ValidationService _sut;

    public ValidationServiceTests()
    {
        _sut = new ValidationService();
    }

    #region Recipe Validation Tests

    [Fact]
    public void ValidateRecipe_WithValidRecipe_ShouldReturnValid()
    {
        // Arrange
        var recipe = CreateValidRecipe();

        // Act
        var result = _sut.ValidateRecipe(recipe);

        // Assert
        result.IsValid.Should().BeTrue();
        result.Errors.Should().BeEmpty();
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public void ValidateRecipe_WithEmptyName_ShouldReturnError(string name)
    {
        // Arrange
        var recipe = CreateValidRecipe();
        recipe.Name = name;

        // Act
        var result = _sut.ValidateRecipe(recipe);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.Field == nameof(Recipe.Name));
    }

    [Fact]
    public void ValidateRecipe_WithShortName_ShouldReturnError()
    {
        // Arrange
        var recipe = CreateValidRecipe();
        recipe.Name = "A"; // Less than MIN_LENGTH

        // Act
        var result = _sut.ValidateRecipe(recipe);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.Field == nameof(Recipe.Name));
        var nameError = result.Errors.First(e => e.Field == nameof(Recipe.Name));
        nameError.Message.Should().Contain(ValidationConstants.NameLimits.MIN_LENGTH.ToString());
    }

    [Fact]
    public void ValidateRecipe_WithLongName_ShouldReturnError()
    {
        // Arrange
        var recipe = CreateValidRecipe();
        recipe.Name = new string('A', ValidationConstants.NameLimits.MAX_LENGTH + 1);

        // Act
        var result = _sut.ValidateRecipe(recipe);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.Field == nameof(Recipe.Name));
    }

    [Fact]
    public void ValidateRecipe_WithZeroYield_ShouldReturnError()
    {
        // Arrange
        var recipe = CreateValidRecipe();
        recipe.Yield = 0;

        // Act
        var result = _sut.ValidateRecipe(recipe);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.Field == nameof(Recipe.Yield));
    }

    [Fact]
    public void ValidateRecipe_WithExcessiveYield_ShouldReturnWarning()
    {
        // Arrange
        var recipe = CreateValidRecipe();
        recipe.Yield = ValidationConstants.YieldLimits.MAX_QUANTITY + 1;

        // Act
        var result = _sut.ValidateRecipe(recipe);

        // Assert
        result.Warnings.Should().Contain(w => w.Field == nameof(Recipe.Yield));
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public void ValidateRecipe_WithEmptyYieldUnit_ShouldReturnError(string? yieldUnit)
    {
        // Arrange
        var recipe = CreateValidRecipe();
        recipe.YieldUnit = yieldUnit;

        // Act
        var result = _sut.ValidateRecipe(recipe);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.Field == nameof(Recipe.YieldUnit));
    }

    [Fact]
    public void ValidateRecipe_WithNoIngredients_ShouldReturnError()
    {
        // Arrange
        var recipe = CreateValidRecipe();
        recipe.RecipeIngredients = new List<RecipeIngredient>();

        // Act
        var result = _sut.ValidateRecipe(recipe);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.Field == nameof(Recipe.RecipeIngredients));
    }

    [Fact]
    public void ValidateRecipe_WithExcessivePrepTime_ShouldReturnWarning()
    {
        // Arrange
        var recipe = CreateValidRecipe();
        recipe.PrepTimeMinutes = ValidationConstants.TimeLimits.MAX_PREP_TIME_MINUTES + 1;

        // Act
        var result = _sut.ValidateRecipe(recipe);

        // Assert
        result.Warnings.Should().Contain(w => w.Field == nameof(Recipe.PrepTimeMinutes));
    }

    [Fact]
    public void ValidateRecipe_WithHighCalories_ShouldReturnWarning()
    {
        // Arrange
        var recipe = CreateValidRecipe();
        var ingredient = new Ingredient
        {
            Id = Guid.NewGuid(),
            Name = "High Calorie Ingredient",
            CaloriesPerUnit = ValidationConstants.NutritionLimits.CALORIES_EXTREME_HIGH + 1000,
            CurrentPrice = 10,
            CaseQuantity = 1,
            Unit = UnitType.Pound
        };
        recipe.RecipeIngredients = new List<RecipeIngredient>
        {
            new RecipeIngredient
            {
                Id = Guid.NewGuid(),
                Quantity = 1,
                Ingredient = ingredient
            }
        };

        // Act
        var result = _sut.ValidateRecipe(recipe);

        // Assert
        result.Warnings.Should().Contain(w => w.Field == "Nutrition");
    }

    #endregion

    #region Ingredient Validation Tests

    [Fact]
    public void ValidateIngredient_WithValidIngredient_ShouldReturnValid()
    {
        // Arrange
        var ingredient = CreateValidIngredient();

        // Act
        var result = _sut.ValidateIngredient(ingredient);

        // Assert
        result.IsValid.Should().BeTrue();
        result.Errors.Should().BeEmpty();
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public void ValidateIngredient_WithEmptyName_ShouldReturnError(string name)
    {
        // Arrange
        var ingredient = CreateValidIngredient();
        ingredient.Name = name;

        // Act
        var result = _sut.ValidateIngredient(ingredient);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.Field == nameof(Ingredient.Name));
    }

    [Fact]
    public void ValidateIngredient_WithNegativePrice_ShouldReturnError()
    {
        // Arrange
        var ingredient = CreateValidIngredient();
        ingredient.CurrentPrice = -1;

        // Act
        var result = _sut.ValidateIngredient(ingredient);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.Field == nameof(Ingredient.CurrentPrice));
    }

    [Fact]
    public void ValidateIngredient_WithZeroPrice_ShouldReturnWarning()
    {
        // Arrange
        var ingredient = CreateValidIngredient();
        ingredient.CurrentPrice = 0;

        // Act
        var result = _sut.ValidateIngredient(ingredient);

        // Assert
        result.Warnings.Should().Contain(w => w.Field == nameof(Ingredient.CurrentPrice));
    }

    [Fact]
    public void ValidateIngredient_WithExcessivePrice_ShouldReturnWarning()
    {
        // Arrange
        var ingredient = CreateValidIngredient();
        ingredient.CurrentPrice = ValidationConstants.PriceLimits.MAX_INGREDIENT_PRICE + 1;

        // Act
        var result = _sut.ValidateIngredient(ingredient);

        // Assert
        result.Warnings.Should().Contain(w => w.Field == nameof(Ingredient.CurrentPrice));
    }

    [Fact]
    public void ValidateIngredient_WithZeroCaseQuantity_ShouldReturnError()
    {
        // Arrange
        var ingredient = CreateValidIngredient();
        ingredient.CaseQuantity = 0;

        // Act
        var result = _sut.ValidateIngredient(ingredient);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.Field == nameof(Ingredient.CaseQuantity));
    }

    [Fact]
    public void ValidateIngredient_WithExcessiveCaseQuantity_ShouldReturnWarning()
    {
        // Arrange
        var ingredient = CreateValidIngredient();
        ingredient.CaseQuantity = ValidationConstants.QuantityLimits.MAX_CASE_QUANTITY + 1;

        // Act
        var result = _sut.ValidateIngredient(ingredient);

        // Assert
        result.Warnings.Should().Contain(w => w.Field == nameof(Ingredient.CaseQuantity));
    }

    #endregion

    #region Entree Validation Tests

    [Fact]
    public void ValidateEntree_WithValidEntree_ShouldReturnValid()
    {
        // Arrange
        var entree = CreateValidEntree();

        // Act
        var result = _sut.ValidateEntree(entree);

        // Assert
        result.IsValid.Should().BeTrue();
        result.Errors.Should().BeEmpty();
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public void ValidateEntree_WithEmptyName_ShouldReturnError(string name)
    {
        // Arrange
        var entree = CreateValidEntree();
        entree.Name = name;

        // Act
        var result = _sut.ValidateEntree(entree);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.Field == nameof(Entree.Name));
    }

    [Fact]
    public void ValidateEntree_WithNoRecipes_ShouldReturnError()
    {
        // Arrange
        var entree = CreateValidEntree();
        entree.EntreeRecipes = new List<EntreeRecipe>();

        // Act
        var result = _sut.ValidateEntree(entree);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.Field == nameof(Entree.EntreeRecipes));
    }

    [Fact]
    public void ValidateEntree_WithHighFoodCostPercent_ShouldReturnWarning()
    {
        // Arrange
        var entree = CreateValidEntree();
        entree.MenuPrice = 10m; // Set low menu price to create high food cost percentage

        var ingredient = new Ingredient
        {
            Id = Guid.NewGuid(),
            Name = "Expensive Ingredient",
            CurrentPrice = 100m,
            CaseQuantity = 1,
            Unit = UnitType.Pound
        };

        var recipe = new Recipe
        {
            Id = Guid.NewGuid(),
            Name = "Expensive Recipe",
            Yield = 1,
            YieldUnit = "servings",
            RecipeIngredients = new List<RecipeIngredient>
            {
                new RecipeIngredient
                {
                    Id = Guid.NewGuid(),
                    Quantity = 1,
                    Ingredient = ingredient
                }
            }
        };

        entree.EntreeRecipes = new List<EntreeRecipe>
        {
            new EntreeRecipe
            {
                Id = Guid.NewGuid(),
                Quantity = 1,
                Recipe = recipe
            }
        };

        // Act
        var result = _sut.ValidateEntree(entree);

        // Assert
        result.Warnings.Should().Contain(w => w.Field == "Profitability");
    }

    [Fact]
    public void ValidateEntree_WithExcessiveMenuPrice_ShouldReturnWarning()
    {
        // Arrange
        var entree = CreateValidEntree();
        entree.MenuPrice = ValidationConstants.PriceLimits.MAX_MENU_PRICE + 1;

        // Act
        var result = _sut.ValidateEntree(entree);

        // Assert
        result.Warnings.Should().Contain(w => w.Field == nameof(Entree.MenuPrice));
    }

    #endregion

    #region Helper Methods

    private Recipe CreateValidRecipe()
    {
        return new Recipe
        {
            Id = Guid.NewGuid(),
            Name = "Test Recipe",
            Yield = 4,
            YieldUnit = "servings",
            Instructions = "Test instructions that are long enough",
            PrepTimeMinutes = 30,
            Category = "Main Dish",
            Difficulty = DifficultyLevel.Medium,
            Tags = "test",
            RecipeIngredients = new List<RecipeIngredient>
            {
                new RecipeIngredient
                {
                    Id = Guid.NewGuid(),
                    Quantity = 1,
                    Ingredient = CreateValidIngredient()
                }
            }
        };
    }

    private Ingredient CreateValidIngredient()
    {
        return new Ingredient
        {
            Id = Guid.NewGuid(),
            Name = "Test Ingredient",
            CurrentPrice = 10m,
            CaseQuantity = 1m,
            Unit = UnitType.Pound,
            VendorName = "Test Vendor",
            Category = "Test Category"
        };
    }

    private Entree CreateValidEntree()
    {
        var ingredient = CreateValidIngredient();
        ingredient.CurrentPrice = 5m;

        var recipe = new Recipe
        {
            Id = Guid.NewGuid(),
            Name = "Test Recipe",
            Yield = 4,
            YieldUnit = "servings",
            RecipeIngredients = new List<RecipeIngredient>
            {
                new RecipeIngredient
                {
                    Id = Guid.NewGuid(),
                    Quantity = 1,
                    Ingredient = ingredient
                }
            }
        };

        return new Entree
        {
            Id = Guid.NewGuid(),
            Name = "Test Entree",
            Description = "Test description for menu item",
            Category = "Appetizer",
            MenuPrice = 20m,
            EntreeRecipes = new List<EntreeRecipe>
            {
                new EntreeRecipe
                {
                    Id = Guid.NewGuid(),
                    Quantity = 1,
                    Recipe = recipe
                }
            }
        };
    }

    #endregion
}
