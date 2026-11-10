using GeometryTrainer.Core.Models;
using GeometryTrainer.Core.Services;

namespace GeometryTrainer.Tests;

public sealed class GeometryTests
{
    [Theory]
    [InlineData(6, 4, 20, 24)]
    [InlineData(4, 4, 16, 16)]
    [InlineData(1, 1, 4, 1)]
    [InlineData(12, 12, 48, 144)]
    [InlineData(8, 3, 22, 24)]
    public void FormulasReturnExpectedResults(int width, int height, long perimeter, long area)
    {
        var rectangle = new RectangleShape(width, height);
        Assert.Equal(perimeter, GeometryCalculator.CalculatePerimeter(rectangle));
        Assert.Equal(area, GeometryCalculator.CalculateArea(rectangle));
    }

    [Theory]
    [InlineData(0, 4)] [InlineData(6, 0)] [InlineData(-1, 4)] [InlineData(6, -2)]
    public void ConstructorRejectsNonpositiveSides(int width, int height) =>
        Assert.Throws<ArgumentOutOfRangeException>(() => new RectangleShape(width, height));

    [Fact]
    public void InvalidResizeDoesNotPartiallyChangeTheRectangle()
    {
        var rectangle = new RectangleShape(6, 4);
        Assert.Throws<ArgumentOutOfRangeException>(() => rectangle.Resize(8, 0));
        Assert.Equal(6, rectangle.Width); Assert.Equal(4, rectangle.Height);
    }

    [Fact]
    public void ResizeNotifiesOnceAndUpdatesBothFormulas()
    {
        var rectangle = new RectangleShape(6, 4);
        var notifications = 0;
        rectangle.Changed += (_, _) => notifications++;
        rectangle.Resize(8, 3); rectangle.Resize(8, 3);
        Assert.Equal(1, notifications);
        Assert.Equal(22, GeometryCalculator.CalculatePerimeter(rectangle));
        Assert.Equal(24, GeometryCalculator.CalculateArea(rectangle));
    }

    [Fact]
    public void SwappingSidesPreservesPerimeterAndArea()
    {
        var first = new RectangleShape(12, 3);
        var second = new RectangleShape(3, 12);
        Assert.Equal(GeometryCalculator.CalculatePerimeter(first), GeometryCalculator.CalculatePerimeter(second));
        Assert.Equal(GeometryCalculator.CalculateArea(first), GeometryCalculator.CalculateArea(second));
    }

    [Fact]
    public void LargeIntegerSidesDoNotOverflow()
    {
        var rectangle = new RectangleShape(int.MaxValue, int.MaxValue);
        Assert.Equal(8_589_934_588L, GeometryCalculator.CalculatePerimeter(rectangle));
        Assert.Equal(4_611_686_014_132_420_609L, GeometryCalculator.CalculateArea(rectangle));
    }

    [Fact]
    public void CalculationsRejectNull()
    {
        Assert.Throws<ArgumentNullException>(() => GeometryCalculator.CalculatePerimeter(null!));
        Assert.Throws<ArgumentNullException>(() => GeometryCalculator.CalculateArea(null!));
    }
}
