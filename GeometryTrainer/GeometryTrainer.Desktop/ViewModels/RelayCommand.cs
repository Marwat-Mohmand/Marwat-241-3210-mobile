using System.Windows.Input;

namespace GeometryTrainer.Desktop.ViewModels;

public sealed class RelayCommand(Action action) : ICommand
{
    public bool CanExecute(object? parameter) => true;
    public void Execute(object? parameter) => action();
    public event EventHandler? CanExecuteChanged { add { } remove { } }
}
