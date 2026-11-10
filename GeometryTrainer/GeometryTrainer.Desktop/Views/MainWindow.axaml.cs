using Avalonia.Controls;
using Avalonia.Input;
using GeometryTrainer.Desktop.ViewModels;

namespace GeometryTrainer.Desktop.Views;

public partial class MainWindow : Window
{
    public MainWindow() => InitializeComponent();

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
