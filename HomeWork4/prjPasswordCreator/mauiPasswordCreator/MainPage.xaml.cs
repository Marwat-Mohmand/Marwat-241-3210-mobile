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
        try { model.LoadOptions(SettingsStore.Load(SettingsPath)); }
        catch (Exception ex) when (ex is IOException or UnauthorizedAccessException or JsonException) { model.Status = "Не удалось загрузить настройки: " + ex.Message; }
    }
    private void PresetChanged(object? sender, EventArgs e)
    {
        if (PresetPicker.SelectedIndex >= 0) model.ApplyPreset(PresetPicker.SelectedIndex);
    }
    private bool ReadNumbers()
    {
        if (!int.TryParse(CountInput.Text, out var count)) { model.Status = "Количество должно быть целым числом."; return false; }
        model.Count = count;
        if (model.IsPhraseMode)
        {
            if (!int.TryParse(WordsInput.Text, out var words)) { model.Status = "Количество слов должно быть целым числом."; return false; }
            model.WordCount = words;
        }
        else
        {
            if (!int.TryParse(LengthInput.Text, out var length)) { model.Status = "Длина должна быть целым числом."; return false; }
            model.Length = length;
        }
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
    private static string SettingsPath => Path.Combine(FileSystem.AppDataDirectory, "settings.json");
    private void SettingsClick(object? sender, EventArgs e)
    {
        if (!ReadNumbers()) return;
        try { SettingsStore.Save(SettingsPath, model.Options); model.Status = "Настройки сохранены."; }
        catch (Exception ex) when (ex is IOException or UnauthorizedAccessException) { model.Status = ex.Message; }
    }
}
