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

public class IngredientRepositoryTests : DatabaseTestBase
{
    private readonly IngredientRepository _repository;
    private readonly Guid _testLocationId = Guid.NewGuid();

    public IngredientRepositoryTests()
    {
        _repository = new IngredientRepository(Context);

        // Create a test location
        Context.Locations.Add(new Location
        {
            Id = _testLocationId,
            Name = "Test Location",
            CreatedAt = DateTime.UtcNow,
            ModifiedAt = DateTime.UtcNow
        });
        Context.SaveChanges();
    }

    #region AddAsync Tests

    [Fact]
    public async Task AddAsync_WithValidIngredient_ShouldAddToDatabase()
    {
        // Arrange
        var ingredient = CreateTestIngredient("Flour");

        // Act
        var result = await _repository.AddAsync(ingredient);

        // Assert
        result.Should().NotBeNull();
        result.Id.Should().NotBe(Guid.Empty);
        result.Name.Should().Be("Flour");
        result.CreatedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(5));
        result.ModifiedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(5));
    }

    [Fact]
    public async Task AddAsync_WithAllergens_ShouldAddAllergens()
    {
        // Arrange
        var ingredient = CreateTestIngredient("Milk");
        var milkAllergenId = Guid.Parse("11111111-1111-1111-1111-111111111111");

        ingredient.IngredientAllergens = new List<IngredientAllergen>
        {
            new IngredientAllergen
            {
                AllergenId = milkAllergenId,
                IsAutoDetected = true,
                IsEnabled = true
            }
        };

        // Act
        var result = await _repository.AddAsync(ingredient);

        // Assert
        result.IngredientAllergens.Should().HaveCount(1);
        result.IngredientAllergens.First().AllergenId.Should().Be(milkAllergenId);
        result.IngredientAllergens.First().IngredientId.Should().Be(result.Id);
    }

    [Fact]
    public async Task AddAsync_WithAliases_ShouldAddAliases()
    {
        // Arrange
        var ingredient = CreateTestIngredient("Tomato");
        ingredient.Aliases = new List<IngredientAlias>
        {
            new IngredientAlias { AliasName = "Tomatoes", IsPrimary = false },
            new IngredientAlias { AliasName = "Roma Tomato", IsPrimary = true }
        };

        // Act
        var result = await _repository.AddAsync(ingredient);

        // Assert
        result.Aliases.Should().HaveCount(2);
        result.Aliases.Should().Contain(a => a.AliasName == "Tomatoes");
        result.Aliases.Should().Contain(a => a.AliasName == "Roma Tomato" && a.IsPrimary);
    }

    #endregion

    #region GetAllAsync Tests

    [Fact]
    public async Task GetAllAsync_WithMultipleIngredients_ShouldReturnAllForLocation()
    {
        // Arrange
        await _repository.AddAsync(CreateTestIngredient("Sugar"));
        await _repository.AddAsync(CreateTestIngredient("Salt"));
        await _repository.AddAsync(CreateTestIngredient("Pepper"));

        // Add ingredient for different location (should not be returned)
        var otherLocationId = Guid.NewGuid();
        Context.Locations.Add(new Location
        {
            Id = otherLocationId,
            Name = "Other Location",
            CreatedAt = DateTime.UtcNow,
            ModifiedAt = DateTime.UtcNow
        });
        await Context.SaveChangesAsync();

        var otherIngredient = CreateTestIngredient("Other Ingredient");
        otherIngredient.LocationId = otherLocationId;
        await _repository.AddAsync(otherIngredient);

        // Act
        var results = await _repository.GetAllAsync(_testLocationId);

        // Assert
        results.Should().HaveCount(3);
        results.Should().AllSatisfy(i => i.LocationId.Should().Be(_testLocationId));
        results.Should().BeInAscendingOrder(i => i.Name);
    }

    [Fact]
    public async Task GetAllAsync_WithNoIngredients_ShouldReturnEmptyList()
    {
        // Act
        var results = await _repository.GetAllAsync(_testLocationId);

        // Assert
        results.Should().BeEmpty();
    }

    #endregion

    #region GetByIdAsync Tests

    [Fact]
    public async Task GetByIdAsync_WithExistingId_ShouldReturnIngredient()
    {
        // Arrange
        var added = await _repository.AddAsync(CreateTestIngredient("Butter"));

        // Act
        var result = await _repository.GetByIdAsync(added.Id);

        // Assert
        result.Should().NotBeNull();
        result!.Id.Should().Be(added.Id);
        result.Name.Should().Be("Butter");
    }

    [Fact]
    public async Task GetByIdAsync_WithNonExistentId_ShouldReturnNull()
    {
        // Act
        var result = await _repository.GetByIdAsync(Guid.NewGuid());

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public async Task GetByIdAsync_ShouldIncludeAliases()
    {
        // Arrange
        var ingredient = CreateTestIngredient("Garlic");
        ingredient.Aliases = new List<IngredientAlias>
        {
            new IngredientAlias { AliasName = "Garlic Cloves" }
        };
        var added = await _repository.AddAsync(ingredient);

        // Act
        var result = await _repository.GetByIdAsync(added.Id);

        // Assert
        result.Should().NotBeNull();
        result!.Aliases.Should().HaveCount(1);
        result.Aliases.First().AliasName.Should().Be("Garlic Cloves");
    }

    #endregion

    #region GetBySkuAsync Tests

    [Fact]
    public async Task GetBySkuAsync_WithExistingSku_ShouldReturnIngredient()
    {
        // Arrange
        var ingredient = CreateTestIngredient("Chicken Breast");
        ingredient.VendorSku = "CHK-001";
        await _repository.AddAsync(ingredient);

        // Act
        var result = await _repository.GetBySkuAsync("CHK-001", _testLocationId);

        // Assert
        result.Should().NotBeNull();
        result!.Name.Should().Be("Chicken Breast");
        result.VendorSku.Should().Be("CHK-001");
    }

    [Fact]
    public async Task GetBySkuAsync_WithNullSku_ShouldReturnNull()
    {
        // Act
        var result = await _repository.GetBySkuAsync(null!, _testLocationId);

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public async Task GetBySkuAsync_WithEmptySku_ShouldReturnNull()
    {
        // Act
        var result = await _repository.GetBySkuAsync("", _testLocationId);

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public async Task GetBySkuAsync_WithDifferentLocation_ShouldReturnNull()
    {
        // Arrange
        var ingredient = CreateTestIngredient("Salmon");
        ingredient.VendorSku = "FISH-001";
        await _repository.AddAsync(ingredient);

        var otherLocationId = Guid.NewGuid();

        // Act
        var result = await _repository.GetBySkuAsync("FISH-001", otherLocationId);

        // Assert
        result.Should().BeNull();
    }

    #endregion

    #region UpdateAsync Tests

    [Fact]
    public async Task UpdateAsync_WithModifiedProperties_ShouldUpdateIngredient()
    {
        // Arrange
        var ingredient = await _repository.AddAsync(CreateTestIngredient("Onion"));

        ingredient.Name = "Red Onion";
        ingredient.CurrentPrice = 5.99m;
        ingredient.Category = "Vegetables";

        // Act
        var result = await _repository.UpdateAsync(ingredient);

        // Assert
        result.Name.Should().Be("Red Onion");
        result.CurrentPrice.Should().Be(5.99m);
        result.Category.Should().Be("Vegetables");

        // Verify in database
        var fromDb = await _repository.GetByIdAsync(ingredient.Id);
        fromDb!.Name.Should().Be("Red Onion");
    }

    [Fact]
    public async Task UpdateAsync_WithNewAllergens_ShouldReplaceAllergens()
    {
        // Arrange
        var ingredient = CreateTestIngredient("Bread");
        var wheatAllergenId = Guid.Parse("77777777-7777-7777-7777-777777777777");

        ingredient.IngredientAllergens = new List<IngredientAllergen>
        {
            new IngredientAllergen
            {
                AllergenId = wheatAllergenId,
                IsEnabled = true
            }
        };

        var added = await _repository.AddAsync(ingredient);

        // Modify allergens
        var soyAllergenId = Guid.Parse("88888888-8888-8888-8888-888888888888");
        added.IngredientAllergens = new List<IngredientAllergen>
        {
            new IngredientAllergen
            {
                AllergenId = soyAllergenId,
                IsEnabled = true
            }
        };

        // Act
        var result = await _repository.UpdateAsync(added);

        // Assert
        result.IngredientAllergens.Should().HaveCount(1);
        result.IngredientAllergens.First().AllergenId.Should().Be(soyAllergenId);
    }

    [Fact]
    public async Task UpdateAsync_WithNewAliases_ShouldReplaceAliases()
    {
        // Arrange
        var ingredient = CreateTestIngredient("Potato");
        ingredient.Aliases = new List<IngredientAlias>
        {
            new IngredientAlias { AliasName = "Potatoes" }
        };
        var added = await _repository.AddAsync(ingredient);

        // Modify aliases
        added.Aliases = new List<IngredientAlias>
        {
            new IngredientAlias { AliasName = "Russet Potato" },
            new IngredientAlias { AliasName = "Baking Potato" }
        };

        // Act
        var result = await _repository.UpdateAsync(added);

        // Assert
        result.Aliases.Should().HaveCount(2);
        result.Aliases.Should().Contain(a => a.AliasName == "Russet Potato");
        result.Aliases.Should().Contain(a => a.AliasName == "Baking Potato");
        result.Aliases.Should().NotContain(a => a.AliasName == "Potatoes");
    }

    #endregion

    #region DeleteAsync Tests

    [Fact]
    public async Task DeleteAsync_WithExistingIngredient_ShouldRemoveFromDatabase()
    {
        // Arrange
        var ingredient = await _repository.AddAsync(CreateTestIngredient("Carrot"));

        // Act
        await _repository.DeleteAsync(ingredient.Id);

        // Assert
        var result = await _repository.GetByIdAsync(ingredient.Id);
        result.Should().BeNull();
    }

    [Fact]
    public async Task DeleteAsync_WithNonExistentId_ShouldNotThrow()
    {
        // Act
        Func<Task> act = async () => await _repository.DeleteAsync(Guid.NewGuid());

        // Assert
        await act.Should().NotThrowAsync();
    }

    [Fact]
    public async Task DeleteAsync_ShouldCascadeDeleteAliases()
    {
        // Arrange
        var ingredient = CreateTestIngredient("Cheese");
        ingredient.Aliases = new List<IngredientAlias>
        {
            new IngredientAlias { AliasName = "Cheddar Cheese" }
        };
        var added = await _repository.AddAsync(ingredient);

        // Act
        await _repository.DeleteAsync(added.Id);

        // Assert
        var aliasCount = Context.IngredientAliases.Count(a => a.IngredientId == added.Id);
        aliasCount.Should().Be(0);
    }

    #endregion

    #region SearchAsync Tests

    [Fact]
    public async Task SearchAsync_WithMatchingName_ShouldReturnResults()
    {
        // Arrange
        await _repository.AddAsync(CreateTestIngredient("Olive Oil"));
        await _repository.AddAsync(CreateTestIngredient("Vegetable Oil"));
        await _repository.AddAsync(CreateTestIngredient("Butter"));

        // Act
        var results = await _repository.SearchAsync("Oil", _testLocationId);

        // Assert
        results.Should().HaveCount(2);
        results.Should().Contain(i => i.Name == "Olive Oil");
        results.Should().Contain(i => i.Name == "Vegetable Oil");
    }

    [Fact]
    public async Task SearchAsync_WithMatchingCategory_ShouldReturnResults()
    {
        // Arrange
        var meat1 = CreateTestIngredient("Beef");
        meat1.Category = "Meat";
        await _repository.AddAsync(meat1);

        var meat2 = CreateTestIngredient("Pork");
        meat2.Category = "Meat";
        await _repository.AddAsync(meat2);

        var vegetable = CreateTestIngredient("Lettuce");
        vegetable.Category = "Vegetables";
        await _repository.AddAsync(vegetable);

        // Act
        var results = await _repository.SearchAsync("Meat", _testLocationId);

        // Assert
        results.Should().HaveCount(2);
        results.Should().AllSatisfy(i => i.Category.Should().Be("Meat"));
    }

    [Fact]
    public async Task SearchAsync_WithMatchingVendorName_ShouldReturnResults()
    {
        // Arrange
        var ing1 = CreateTestIngredient("Product A");
        ing1.VendorName = "Sysco";
        await _repository.AddAsync(ing1);

        var ing2 = CreateTestIngredient("Product B");
        ing2.VendorName = "US Foods";
        await _repository.AddAsync(ing2);

        // Act
        var results = await _repository.SearchAsync("Sysco", _testLocationId);

        // Assert
        results.Should().HaveCount(1);
        results.First().VendorName.Should().Be("Sysco");
    }

    [Fact]
    public async Task SearchAsync_WithMatchingAlias_ShouldReturnResults()
    {
        // Arrange
        var ingredient = CreateTestIngredient("Cilantro");
        ingredient.Aliases = new List<IngredientAlias>
        {
            new IngredientAlias { AliasName = "Coriander" }
        };
        await _repository.AddAsync(ingredient);

        // Act
        var results = await _repository.SearchAsync("Coriander", _testLocationId);

        // Assert
        results.Should().HaveCount(1);
        results.First().Name.Should().Be("Cilantro");
    }

    [Fact]
    public async Task SearchAsync_WithEmptySearchTerm_ShouldReturnAll()
    {
        // Arrange
        await _repository.AddAsync(CreateTestIngredient("Item 1"));
        await _repository.AddAsync(CreateTestIngredient("Item 2"));
        await _repository.AddAsync(CreateTestIngredient("Item 3"));

        // Act
        var results = await _repository.SearchAsync("", _testLocationId);

        // Assert
        results.Should().HaveCount(3);
    }

    #endregion

    #region ExistsAsync Tests

    [Fact]
    public async Task ExistsAsync_WithExistingId_ShouldReturnTrue()
    {
        // Arrange
        var ingredient = await _repository.AddAsync(CreateTestIngredient("Test Item"));

        // Act
        var exists = await _repository.ExistsAsync(ingredient.Id);

        // Assert
        exists.Should().BeTrue();
    }

    [Fact]
    public async Task ExistsAsync_WithNonExistentId_ShouldReturnFalse()
    {
        // Act
        var exists = await _repository.ExistsAsync(Guid.NewGuid());

        // Assert
        exists.Should().BeFalse();
    }

    #endregion

    #region Helper Methods

    private Ingredient CreateTestIngredient(string name)
    {
        return new Ingredient
        {
            Name = name,
            CurrentPrice = 10.00m,
            CaseQuantity = 1.0m,
            Unit = UnitType.Pound,
            LocationId = _testLocationId,
            VendorName = "Test Vendor",
            Category = "Test Category"
        };
    }

    #endregion
}
