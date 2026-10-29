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

        Add(Row(new Label { Text = "Длина (1–256)", AutoSize = true }, length, new Label { Text = "Количество (1–100)", AutoSize = true }, count));
        Add(Row(lower, upper, digits, symbols));

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

        Add(actions); Add(status); Add(output); Add(estimate);

        model.PropertyChanged += (_, _) => { output.Text = model.Output; status.Text = model.Status; estimate.Text = model.Estimate; };

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

    }
    private void SyncFromModel()
    {
        length.Value = Math.Clamp(model.Length, 1, 256); count.Value = Math.Clamp(model.Count, 1, 100);
        lower.Checked = model.Lowercase; upper.Checked = model.Uppercase; digits.Checked = model.Digits; symbols.Checked = model.Symbols;

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

}
