using System.Windows.Input;
using GeometryTrainer.Core.Models;
using GeometryTrainer.Core.Services;

namespace GeometryTrainer.Desktop.ViewModels;

public sealed class MainWindowViewModel : ViewModelBase
{
    public const int GridSize = 12;
    private readonly RectangleShape rectangle = new(6, 4);
    private bool isHelpOpen;

    public MainWindowViewModel()
    {
        rectangle.Changed += (_, _) => NotifyRectangle();
        ResetCommand = new RelayCommand(() => rectangle.Resize(6, 4));
        SwapCommand = new RelayCommand(() => rectangle.Resize(rectangle.Height, rectangle.Width));
        SquareCommand = new RelayCommand(() => rectangle.Resize(4, 4));
        ShowHelpCommand = new RelayCommand(() => IsHelpOpen = true);
        CloseHelpCommand = new RelayCommand(() => IsHelpOpen = false);
    }

    // NumericUpDown and Slider both bind to the same two properties.
    public int RectangleWidth
    {
        get => rectangle.Width;
        set => SetDimensions(value, rectangle.Height);
    }

    public int RectangleHeight
    {
        get => rectangle.Height;
        set => SetDimensions(rectangle.Width, value);
    }

    public long Perimeter => GeometryCalculator.CalculatePerimeter(rectangle);
    public long Area => GeometryCalculator.CalculateArea(rectangle);
    public string DimensionsText => $"{RectangleWidth} × {RectangleHeight}";
    public string PerimeterCalculation => $"2 × ({RectangleWidth} + {RectangleHeight}) = {Perimeter}";
    public string AreaCalculation => $"{RectangleWidth} × {RectangleHeight} = {Area}";
    public string CellCountText => $"Внутри фигуры — {Area} ед.²";
    public string ShapeDescription => RectangleWidth == RectangleHeight
        ? "Это квадрат: все четыре стороны равны."
        : "У прямоугольника противоположные стороны равны.";

    public bool IsHelpOpen
    {
        get => isHelpOpen;
        set { if (isHelpOpen == value) return; isHelpOpen = value; Notify(); }
    }

    public ICommand ResetCommand { get; }
    public ICommand SwapCommand { get; }
    public ICommand SquareCommand { get; }
    public ICommand ShowHelpCommand { get; }
    public ICommand CloseHelpCommand { get; }

    public void AdjustWidth(int delta) => RectangleWidth = Math.Clamp(RectangleWidth + delta, 1, GridSize);
    public void AdjustHeight(int delta) => RectangleHeight = Math.Clamp(RectangleHeight + delta, 1, GridSize);

    private void SetDimensions(int width, int height)
    {
        if (width is < 1 or > GridSize || height is < 1 or > GridSize)
            throw new ArgumentOutOfRangeException(nameof(width), "Стороны должны быть от 1 до 12 единиц.");
        rectangle.Resize(width, height);
    }

    private void NotifyRectangle()
    {
        Notify(nameof(RectangleWidth)); Notify(nameof(RectangleHeight));
        Notify(nameof(Perimeter)); Notify(nameof(Area)); Notify(nameof(DimensionsText));
        Notify(nameof(PerimeterCalculation)); Notify(nameof(AreaCalculation));
        Notify(nameof(CellCountText)); Notify(nameof(ShapeDescription));
    }
}
