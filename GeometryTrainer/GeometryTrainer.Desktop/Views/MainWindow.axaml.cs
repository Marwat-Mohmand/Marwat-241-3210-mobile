using System.Globalization;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using GeometryTrainer.Desktop.ViewModels;

namespace GeometryTrainer.Desktop.Views;

public partial class MainWindow : Window
{
    public MainWindow()
    {
        InitializeComponent();
        // NumericUpDown can handle Enter itself when it cannot parse the text.
        WidthInput.AddHandler(KeyDownEvent, DimensionKeyDown, RoutingStrategies.Bubble, handledEventsToo: true);
        HeightInput.AddHandler(KeyDownEvent, DimensionKeyDown, RoutingStrategies.Bubble, handledEventsToo: true);
    }

    private void DimensionKeyDown(object? sender, KeyEventArgs e)
    {
        if (e.Key == Key.Enter && sender is NumericUpDown input)
            RestoreDimension(input);
    }

    private void DimensionLostFocus(object? sender, FocusChangedEventArgs e)
    {
        if (sender is NumericUpDown input && !input.IsKeyboardFocusWithin)
            RestoreDimension(input);
    }

    private void RestoreDimension(NumericUpDown input)
    {
        if (DataContext is not MainWindowViewModel model) return;

        // Keep the committed field, slider and grid in sync with the accepted dimension.
        int dimension = input == WidthInput ? model.RectangleWidth : model.RectangleHeight;
        input.SetCurrentValue(NumericUpDown.ValueProperty, (decimal?)dimension);
        input.SetCurrentValue(NumericUpDown.TextProperty, dimension.ToString(CultureInfo.CurrentCulture));
    }

    protected override void OnKeyDown(KeyEventArgs e)
    {
        base.OnKeyDown(e);
        if (DataContext is not MainWindowViewModel model) return;
        if (e.Key == Key.F1) { model.IsHelpOpen = true; e.Handled = true; }
        if (e.Key == Key.Escape) { model.IsHelpOpen = false; e.Handled = true; }
    }

    private void GridKeyDown(object? sender, KeyEventArgs e)
    {
        if (DataContext is not MainWindowViewModel model || model.IsHelpOpen) return;
        switch (e.Key)
        {
            case Key.Left: model.AdjustWidth(-1); break;
            case Key.Right: model.AdjustWidth(1); break;
            case Key.Up: model.AdjustHeight(1); break;
            case Key.Down: model.AdjustHeight(-1); break;
            default: return;
        }
        e.Handled = true;
    }
}
