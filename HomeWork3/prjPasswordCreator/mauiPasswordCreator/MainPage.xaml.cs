using System.Text.Json;
using PasswordCreator.Presentation;

namespace PasswordCreator.Maui;

public partial class MainPage : ContentPage
{
    private readonly GeneratorViewModel model = new();
    public MainPage()
    {
        InitializeComponent();
        BindingContext = model;

    }
    private void PresetChanged(object? sender, EventArgs e)
    {
        if (PresetPicker.SelectedIndex >= 0) model.ApplyPreset(PresetPicker.SelectedIndex);
    }
    private bool ReadNumbers()
    {
        if (!int.TryParse(CountInput.Text, out var count)) { model.Status = "Количество должно быть целым числом."; return false; }
        model.Count = count;
        if (!int.TryParse(LengthInput.Text, out var length)) { model.Status = "Длина должна быть целым числом."; return false; }
        model.Length = length;
        return true;
    }
    private void GenerateClick(object? sender, EventArgs e) { if (ReadNumbers()) model.Generate(); }
    private async void CopyClick(object? sender, EventArgs e)
    {
        if (model.Output.Length == 0) { model.Status = "Сначала создайте пароли."; return; }
        try { await Clipboard.Default.SetTextAsync(model.Output); model.Status = "Результаты скопированы."; }
        catch (Exception ex) { model.Status = "Не удалось скопировать: " + ex.Message; }
    }
    private async void ExportClick(object? sender, EventArgs e)
    {
        if (model.Output.Length == 0) { model.Status = "Сначала создайте пароли."; return; }
        try
        {
            var path = Path.Combine(FileSystem.CacheDirectory, "passwordcreator-export.txt");
            await File.WriteAllTextAsync(path, model.Output);
            await Share.Default.RequestAsync(new ShareFileRequest { Title = "Экспорт паролей", File = new ShareFile(path) });
            model.Status = "Диалог экспорта закрыт.";
        }
        catch (Exception ex) { model.Status = "Не удалось экспортировать: " + ex.Message; }
    }

}
