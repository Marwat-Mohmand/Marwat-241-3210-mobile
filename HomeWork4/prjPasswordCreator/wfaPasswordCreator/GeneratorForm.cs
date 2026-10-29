using System.ComponentModel;
using System.Runtime.InteropServices;
using System.Text.Json;
using PasswordCreator.Presentation;

namespace PasswordCreator.WinForms;

public sealed class GeneratorForm : Form
{
    private readonly GeneratorViewModel model = new();
    private readonly FlowLayoutPanel content = new() { Dock = DockStyle.Fill, FlowDirection = FlowDirection.TopDown, WrapContents = false, AutoScroll = true, Padding = new Padding(24) };
    private readonly TextBox output = new() { Multiline = true, ReadOnly = true, WordWrap = false, ScrollBars = ScrollBars.Both, Width = 760, Height = 160, Font = new Font("Consolas", 12) };
    private readonly Label status = new() { AutoSize = true, MaximumSize = new Size(750, 0), ForeColor = Color.FromArgb(36, 87, 166), Margin = new Padding(0, 12, 0, 12) };
    private readonly Label estimate = new() { AutoSize = true, MaximumSize = new Size(750, 0) };
    private readonly NumericUpDown length = new() { Minimum = 1, Maximum = 256, Width = 90 };
    private readonly NumericUpDown count = new() { Minimum = 1, Maximum = 100, Width = 90 };
    private readonly CheckBox lower = new() { Text = "abc", AutoSize = true };
    private readonly CheckBox upper = new() { Text = "ABC", AutoSize = true };
    private readonly CheckBox digits = new() { Text = "123", AutoSize = true };
    private readonly CheckBox symbols = new() { Text = "!@#", AutoSize = true };
    private readonly ComboBox mode = new() { DropDownStyle = ComboBoxStyle.DropDownList, Width = 320 };
    private readonly CheckBox ambiguous = new() { Text = "Без похожих O0oIl1|", AutoSize = true };
    private readonly CheckBox unique = new() { Text = "Без повторов", AutoSize = true };
    private readonly TextBox excluded = new() { Width = 300 };
    private readonly TextBox alphabet = new() { Width = 300 };
    private readonly NumericUpDown words = new() { Minimum = 4, Maximum = 12, Width = 80 };
    private readonly TextBox separator = new() { Width = 80, MaxLength = 3 };
    private readonly TextBox history = new() { Multiline = true, ReadOnly = true, ScrollBars = ScrollBars.Both, WordWrap = false, Width = 760, Height = 100 };
    public GeneratorForm()
    {
        Text = model.Title; Width = 870; Height = 850; MinimumSize = new Size(850, 500);
        Font = new Font("Segoe UI", 10); BackColor = Color.FromArgb(242, 245, 251); AutoScaleMode = AutoScaleMode.Dpi;
        Controls.Add(content);
        Add(new Label { Text = model.Title, AutoSize = true, Font = new Font("Segoe UI", 22, FontStyle.Bold), ForeColor = Color.FromArgb(25, 44, 85) });
        Add(new Label { Text = model.Subtitle, AutoSize = true, Margin = new Padding(0, 4, 0, 16) });
        var presets = new ComboBox { DropDownStyle = ComboBoxStyle.DropDownList, Width = 320 };
        presets.Items.AddRange(model.Presets.Cast<object>().ToArray());
        presets.SelectedIndexChanged += (_, _) => { model.ApplyPreset(presets.SelectedIndex); SyncFromModel(); };
        Add(Row(new Label { Text = "Профиль", AutoSize = true }, presets));
        mode.Items.AddRange(model.Modes.Cast<object>().ToArray());
        mode.SelectedIndexChanged += (_, _) => RefreshMode();
        Add(Row(new Label { Text = "Режим", AutoSize = true }, mode));
        Add(Row(new Label { Text = "Длина (1–256)", AutoSize = true }, length, new Label { Text = "Количество (1–100)", AutoSize = true }, count));
        Add(Row(lower, upper, digits, symbols));
        Add(Row(ambiguous, unique));
        Add(Row(new Label { Text = "Исключить символы", AutoSize = true }, excluded));
        Add(Row(new Label { Text = "Свой алфавит", AutoSize = true }, alphabet));
        Add(Row(new Label { Text = "Слов (4–12)", AutoSize = true }, words, new Label { Text = "Разделитель", AutoSize = true }, separator));
        var actions = Row();
        Button Action(string text, Action handler)
        {
            var button = new Button { Text = text, AutoSize = true, Padding = new Padding(8), Margin = new Padding(0, 8, 8, 8) };
            button.Click += (_, _) => handler(); actions.Controls.Add(button); return button;
        }
        AcceptButton = Action("Сгенерировать", () => { SyncToModel(); model.Generate(); });
        Action("Копировать", Copy);
        Action("Сохранить TXT", Save);
        Action("Очистить", model.Clear);
        Action("Запомнить настройки", SaveSettings);
        Add(actions); Add(status); Add(output); Add(estimate);
        Add(new Label { Text = "История сеанса · до 50 паролей · кнопка «Очистить» удаляет историю", AutoSize = true }); Add(history);
        model.PropertyChanged += (_, _) => { output.Text = model.Output; status.Text = model.Status; estimate.Text = model.Estimate; history.Text = model.HistoryText; };
        try { model.LoadOptions(SettingsStore.Load(SettingsStore.DefaultPath)); }
        catch (Exception ex) when (ex is IOException or UnauthorizedAccessException or JsonException) { model.Status = "Не удалось загрузить настройки: " + ex.Message; }
        SyncFromModel(); status.Text = model.Status;
    }
    private void Add(Control control) => content.Controls.Add(control);
    private static FlowLayoutPanel Row(params Control[] controls)
    {
        var panel = new FlowLayoutPanel { AutoSize = true, MaximumSize = new Size(770, 0), Margin = new Padding(0, 4, 0, 4) };
        panel.Controls.AddRange(controls); return panel;
    }
    private void SyncToModel()
    {
        model.Length = (int)length.Value; model.Count = (int)count.Value;
        model.Lowercase = lower.Checked; model.Uppercase = upper.Checked; model.Digits = digits.Checked; model.Symbols = symbols.Checked;
        model.ModeIndex = mode.SelectedIndex; model.ExcludeAmbiguous = ambiguous.Checked; model.NoRepeatedCharacters = unique.Checked;
        model.ExcludedCharacters = excluded.Text; model.CustomAlphabet = alphabet.Text; model.WordCount = (int)words.Value; model.Separator = separator.Text;
    }
    private void SyncFromModel()
    {
        length.Value = Math.Clamp(model.Length, 1, 256); count.Value = Math.Clamp(model.Count, 1, 100);
        lower.Checked = model.Lowercase; upper.Checked = model.Uppercase; digits.Checked = model.Digits; symbols.Checked = model.Symbols;
        mode.SelectedIndex = Math.Clamp(model.ModeIndex, 0, 3); ambiguous.Checked = model.ExcludeAmbiguous; unique.Checked = model.NoRepeatedCharacters;
        excluded.Text = model.ExcludedCharacters; alphabet.Text = model.CustomAlphabet; words.Value = Math.Clamp(model.WordCount, 4, 12); separator.Text = model.Separator;
        RefreshMode();
    }
    private void Copy()
    {
        if (model.Output.Length == 0) { model.Status = "Сначала создайте пароли."; return; }
        try { Clipboard.SetText(model.Output); model.Status = "Результаты скопированы."; }
        catch (ExternalException) { model.Status = "Буфер обмена занят. Повторите попытку."; }
    }
    private void Save()
    {
        if (model.Output.Length == 0) { model.Status = "Сначала создайте пароли."; return; }
        using var dialog = new SaveFileDialog { Filter = "Текстовый файл (*.txt)|*.txt", FileName = "passwords.txt" };
        if (dialog.ShowDialog(this) != DialogResult.OK) return;
        try { File.WriteAllText(dialog.FileName, model.Output); model.Status = "Файл сохранён: " + dialog.FileName; }
        catch (Exception ex) when (ex is IOException or UnauthorizedAccessException) { model.Status = ex.Message; }
    }
    private void RefreshMode()
    {
        var random = mode.SelectedIndex == 0; var phrase = mode.SelectedIndex == 2;
        lower.Enabled = upper.Enabled = digits.Enabled = symbols.Enabled = random;
        length.Enabled = ambiguous.Enabled = unique.Enabled = excluded.Enabled = !phrase;
        alphabet.Enabled = mode.SelectedIndex == 3; words.Enabled = separator.Enabled = phrase;
    }
    private void SaveSettings()
    {
        SyncToModel();
        try { SettingsStore.Save(SettingsStore.DefaultPath, model.Options); model.Status = "Настройки сохранены."; }
        catch (Exception ex) when (ex is IOException or UnauthorizedAccessException) { model.Status = ex.Message; }
    }
}
