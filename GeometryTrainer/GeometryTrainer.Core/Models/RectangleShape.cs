namespace GeometryTrainer.Core.Models;

/// <summary>A rectangle measured in positive whole units. No UI dependencies.</summary>
public sealed class RectangleShape
{
    public int Width { get; private set; }
    public int Height { get; private set; }

    public event EventHandler? Changed;

    public RectangleShape(int width, int height)
    {
        Validate(width, height);
        Width = width;
        Height = height;
    }

    /// <summary>Validate both sides before changing either one.</summary>
    public void Resize(int width, int height)
    {
        Validate(width, height);
        if (Width == width && Height == height) return;
        Width = width;
        Height = height;
        Changed?.Invoke(this, EventArgs.Empty);
    }

    private static void Validate(int width, int height)
    {
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(width);
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(height);
    }
}
