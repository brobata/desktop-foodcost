using FluentAssertions;
using Freecost.Core.Services;
using Xunit;

namespace Freecost.Tests.Services;

public class CategoryColorServiceTests
{
    private readonly CategoryColorService _sut;

    public CategoryColorServiceTests()
    {
        _sut = new CategoryColorService();
    }

    [Fact]
    public void GetOrAssignColor_WithNewCategory_ShouldAssignColor()
    {
        // Arrange
        var category = "New Category";

        // Act
        var color = _sut.GetOrAssignColor(category);

        // Assert
        color.Should().NotBeNullOrEmpty();
        color.Should().StartWith("#");
        color.Length.Should().Be(7); // #RRGGBB format
    }

    [Fact]
    public void GetOrAssignColor_WithSameCategory_ShouldReturnSameColor()
    {
        // Arrange
        var category = "Consistent Category";

        // Act
        var color1 = _sut.GetOrAssignColor(category);
        var color2 = _sut.GetOrAssignColor(category);

        // Assert
        color1.Should().Be(color2);
    }

    [Fact]
    public void GetOrAssignColor_WithExistingColor_ShouldReturnExistingColor()
    {
        // Arrange
        var category = "Test Category";
        var existingColor = "#FF5733";

        // Act
        var color = _sut.GetOrAssignColor(category, existingColor);

        // Assert
        color.Should().Be(existingColor);
    }

    [Fact]
    public void GetOrAssignColor_WithNullCategory_ShouldReturnDefaultColor()
    {
        // Act
        var color = _sut.GetOrAssignColor(null);

        // Assert
        color.Should().NotBeNullOrEmpty();
    }

    [Fact]
    public void GetOrAssignColor_WithEmptyCategory_ShouldReturnDefaultColor()
    {
        // Act
        var color = _sut.GetOrAssignColor("");

        // Assert
        color.Should().NotBeNullOrEmpty();
    }

    [Fact]
    public void GetOrAssignColor_WithDifferentCategories_ShouldAssignDifferentColors()
    {
        // Arrange
        var category1 = "Category One";
        var category2 = "Category Two";

        // Act
        var color1 = _sut.GetOrAssignColor(category1);
        var color2 = _sut.GetOrAssignColor(category2);

        // Assert
        color1.Should().NotBe(color2);
    }

    [Theory]
    [InlineData("Meat & Poultry")]
    [InlineData("Seafood & Fish")]
    [InlineData("Dairy & Eggs")]
    [InlineData("Fresh Produce")]
    public void GetOrAssignColor_WithCommonCategories_ShouldAssignValidColors(string category)
    {
        // Act
        var color = _sut.GetOrAssignColor(category);

        // Assert
        color.Should().NotBeNullOrEmpty();
        color.Should().MatchRegex("^#[0-9A-Fa-f]{6}$");
    }

    [Fact]
    public void GetOrAssignColor_WithCaseVariation_ShouldBeConsistent()
    {
        // Arrange - Categories are case-sensitive in hashing
        var category1 = "Vegetables";
        var category2 = "vegetables";

        // Act
        var color1 = _sut.GetOrAssignColor(category1);
        var color2 = _sut.GetOrAssignColor(category2);

        // Assert
        // Note: Hash collisions can occur, but each specific category should be consistent
        // The colors may or may not be different depending on hash values
        var color1Again = _sut.GetOrAssignColor(category1);
        var color2Again = _sut.GetOrAssignColor(category2);

        color1.Should().Be(color1Again, "same category should return same color");
        color2.Should().Be(color2Again, "same category should return same color");
    }

    [Fact]
    public void GetOrAssignColor_CalledMultipleTimes_ShouldBeConsistent()
    {
        // Arrange
        var category = "Consistent Test";
        var iterations = 100;

        // Act
        var colors = Enumerable.Range(0, iterations)
            .Select(_ => _sut.GetOrAssignColor(category))
            .ToList();

        // Assert
        colors.Should().OnlyContain(c => c == colors[0]);
    }
}
