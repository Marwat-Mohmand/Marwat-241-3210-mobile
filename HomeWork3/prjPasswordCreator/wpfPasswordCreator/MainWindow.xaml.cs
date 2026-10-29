using System.IO;
using System.Runtime.InteropServices;
using System.Text.Json;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using Microsoft.Win32;
using PasswordCreator.Presentation;

namespace PasswordCreator.Wpf;

public partial class MainWindow : Window
{
    private readonly GeneratorViewModel model = new();
    public MainWindow()
    {
        InitializeComponent();
        DataContext = model;

    }
    private void PresetChanged(object sender, SelectionChangedEventArgs e)
    {
        if (PresetPicker.SelectedIndex >= 0) model.ApplyPreset(PresetPicker.SelectedIndex);
    }
    private static bool HasErrors(DependencyObject parent)
    {
        if (Validation.GetHasError(parent)) return true;
        for (var i = 0; i < VisualTreeHelper.GetChildrenCount(parent); i++)
            if (HasErrors(VisualTreeHelper.GetChild(parent, i))) return true;
        return false;
    }
    private void GenerateClick(object sender, RoutedEventArgs e)
    {
        if (HasErrors(this)) { model.Status = "Исправьте числовые поля, выделенные красным."; return; }
        model.Generate();
    }
    private void CopyClick(object sender, RoutedEventArgs e)
    {
        if (string.IsNullOrEmpty(model.Output)) { model.Status = "Сначала создайте пароли."; return; }
        try { Clipboard.SetText(model.Output); model.Status = "Результаты скопированы."; }
        catch (ExternalException) { model.Status = "Буфер обмена занят. Повторите попытку."; }
    }
    private void SaveClick(object sender, RoutedEventArgs e)
    {
        if (string.IsNullOrEmpty(model.Output)) { model.Status = "Сначала создайте пароли."; return; }
        var dialog = new SaveFileDialog { Filter = "Текстовый файл (*.txt)|*.txt", FileName = "passwords.txt" };
        if (dialog.ShowDialog(this) != true) return;
        try { File.WriteAllText(dialog.FileName, model.Output); model.Status = "Файл сохранён: " + dialog.FileName; }
        catch (Exception ex) when (ex is IOException or UnauthorizedAccessException) { model.Status = ex.Message; }
    }

}
