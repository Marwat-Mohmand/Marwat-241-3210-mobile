using System.Globalization;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Media;

namespace GeometryTrainer.Desktop.Views;

/// <summary>Draws a real, resizable Avalonia grid. It contains no geometry formulas.</summary>
public sealed class RectangleGrid : Control
{
    public static readonly StyledProperty<int> SideAProperty =
        AvaloniaProperty.Register<RectangleGrid, int>(nameof(SideA), 6);
    public static readonly StyledProperty<int> SideBProperty =
        AvaloniaProperty.Register<RectangleGrid, int>(nameof(SideB), 4);

    private static readonly IBrush Blue = new SolidColorBrush(Color.Parse("#3478F6"));
    private static readonly IBrush Ink = new SolidColorBrush(Color.Parse("#536985"));
    private static readonly Pen GridPen = new(new SolidColorBrush(Color.Parse("#E6EAF0")), 1);
    private static readonly Pen FilledGridPen = new(new SolidColorBrush(Color.Parse("#AECFFF")), 1);

    public int SideA { get => GetValue(SideAProperty); set => SetValue(SideAProperty, value); }
    public int SideB { get => GetValue(SideBProperty); set => SetValue(SideBProperty, value); }

    static RectangleGrid() => AffectsRender<RectangleGrid>(SideAProperty, SideBProperty);

    protected override void OnPointerPressed(PointerPressedEventArgs e)
    {
        base.OnPointerPressed(e);
        Focus();
    }

    public override void Render(DrawingContext context)
    {
        base.Render(context);
        const int cells = 12;
        var cell = Math.Min((Bounds.Width - 90) / cells, (Bounds.Height - 78) / cells);
        if (cell <= 0) return;
        var side = cell * cells;
        var x = (Bounds.Width - side) / 2 - 6;
        var y = (Bounds.Height - side) / 2 - 5;
        context.DrawRectangle(Brushes.White, null, new Rect(x, y, side, side));
        for (var i = 0; i <= cells; i++)
        {
            context.DrawLine(GridPen, new Point(x + i * cell, y), new Point(x + i * cell, y + side));
            context.DrawLine(GridPen, new Point(x, y + i * cell), new Point(x + side, y + i * cell));
        }

        // Both axes always use the same cell size; vertices sit on grid intersections.
        var a = Math.Clamp(SideA, 1, cells);
        var b = Math.Clamp(SideB, 1, cells);
        var left = x + Math.Floor((cells - a) / 2d) * cell;
        var top = y + Math.Floor((cells - b) / 2d) * cell;
        var rectangle = new Rect(left, top, a * cell, b * cell);
        context.DrawRectangle(new SolidColorBrush(Color.Parse("#E0EDFF")), null, rectangle);
        for (var i = 1; i < a; i++)
            context.DrawLine(FilledGridPen, new Point(left + i * cell, top), new Point(left + i * cell, rectangle.Bottom));
        for (var j = 1; j < b; j++)
            context.DrawLine(FilledGridPen, new Point(left, top + j * cell), new Point(rectangle.Right, top + j * cell));
        context.DrawRectangle(null, new Pen(Blue, 2.5), rectangle);

        Point[] corners = [rectangle.TopLeft, rectangle.TopRight, rectangle.BottomRight, rectangle.BottomLeft];
        foreach (var corner in corners)
            context.DrawEllipse(Blue, new Pen(Brushes.White, 2.5), corner, 6.5, 6.5);

        var labelA = Text($"a = {a}", 14, Blue);
        var labelB = Text($"b = {b}", 14, Blue);
        context.DrawText(labelA, new Point(rectangle.Center.X - labelA.Width / 2, rectangle.Bottom + 12));
        context.DrawText(labelB, new Point(rectangle.Right + 12, rectangle.Center.Y - labelB.Height / 2));
        for (var i = 0; i <= cells; i += 2)
        {
            var number = Text(i.ToString(CultureInfo.InvariantCulture), 10, Ink);
            context.DrawText(number, new Point(x + i * cell - number.Width / 2, y + side + 29));
        }
    }

    private static FormattedText Text(string text, double size, IBrush brush) => new(text,
        CultureInfo.GetCultureInfo("ru-RU"), FlowDirection.LeftToRight,
        new Typeface("avares://Avalonia.Fonts.Inter/Assets#Inter"), size, brush);
}
