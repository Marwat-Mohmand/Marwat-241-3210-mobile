using GeometryTrainer.Core.Models;

namespace GeometryTrainer.Core.Services;

/// <summary>Reusable C# geometry formulas, independent of Avalonia.</summary>
public static class GeometryCalculator
{
    public static long CalculatePerimeter(RectangleShape rectangle)
    {
        ArgumentNullException.ThrowIfNull(rectangle);
        // Cast before adding to avoid overflowing a 32-bit integer.
        return 2L * ((long)rectangle.Width + rectangle.Height);
    }

    public static long CalculateArea(RectangleShape rectangle)
    {
        ArgumentNullException.ThrowIfNull(rectangle);
        return (long)rectangle.Width * rectangle.Height;
    }
}
