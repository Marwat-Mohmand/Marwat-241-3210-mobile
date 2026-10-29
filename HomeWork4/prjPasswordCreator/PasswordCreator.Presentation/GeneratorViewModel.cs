using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Input;
using PasswordCreator.Core;

namespace PasswordCreator.Presentation;

public sealed class GeneratorViewModel : INotifyPropertyChanged
{
    private readonly PasswordGenerator generator = new();
    private PasswordOptions options = new();
    private string output = "";
    private string status = "Выберите настройки и нажмите «Сгенерировать».";
    private string estimate = "";
    public event PropertyChangedEventHandler? PropertyChanged;
    public string Title => "Генератор паролей · HomeWork4";
    public string Subtitle => "Новые режимы, настройки и история сеанса";
    public PasswordOptions Options => options;
    public IReadOnlyList<string> Presets { get; } = ["Надёжный", "Без спецсимволов", "Только цифры", "Парольная фраза", "Свой алфавит"];
    public int Length { get => options.Length; set => Update(options with { Length = value }); }
    public int Count { get => options.Count; set => Update(options with { Count = value }); }
    public bool Lowercase { get => options.Lowercase; set => Update(options with { Lowercase = value }); }
    public bool Uppercase { get => options.Uppercase; set => Update(options with { Uppercase = value }); }
    public bool Digits { get => options.Digits; set => Update(options with { Digits = value }); }
    public bool Symbols { get => options.Symbols; set => Update(options with { Symbols = value }); }
    public IReadOnlyList<string> Modes { get; } = ["Случайный пароль", "PIN-код", "Парольная фраза", "Свой алфавит"];
    public int ModeIndex { get => (int)options.Mode; set => Update(options with { Mode = (PasswordMode)value }); }
    public bool ExcludeAmbiguous { get => options.ExcludeAmbiguous; set => Update(options with { ExcludeAmbiguous = value }); }
    public bool NoRepeatedCharacters { get => options.NoRepeatedCharacters; set => Update(options with { NoRepeatedCharacters = value }); }
    public string ExcludedCharacters { get => options.ExcludedCharacters; set => Update(options with { ExcludedCharacters = value }); }
    public string CustomAlphabet { get => options.CustomAlphabet; set => Update(options with { CustomAlphabet = value }); }
    public int WordCount { get => options.WordCount; set => Update(options with { WordCount = value }); }
    public string Separator { get => options.Separator; set => Update(options with { Separator = value }); }
    public ObservableCollection<string> History { get; } = [];
    public string HistoryText => string.Join(Environment.NewLine, History);
    public bool IsCharacterMode => options.Mode != PasswordMode.Passphrase;
    public bool IsRandomMode => options.Mode == PasswordMode.Random;
    public bool IsCustomMode => options.Mode == PasswordMode.Custom;
    public bool IsPhraseMode => options.Mode == PasswordMode.Passphrase;
    public string Output { get => output; private set { output = value; Notify(); } }
    public string Status { get => status; set { status = value; Notify(); } }
    public string Estimate { get => estimate; private set { estimate = value; Notify(); } }
    public ICommand GenerateCommand { get; }
    public ICommand ClearCommand { get; }
    public GeneratorViewModel()
    {
        GenerateCommand = new ActionCommand(Generate);
        ClearCommand = new ActionCommand(Clear);
    }
    public void LoadOptions(PasswordOptions value) => Update(value);
    private void Update(PasswordOptions value)
    {
        if (options == value) return;
        options = value;
        Notify("");
    }
    public void ApplyPreset(int index)
    {
        var preset = index switch
        {
            0 => new PasswordOptions { Count = Count },
            1 => new PasswordOptions { Count = Count, Length = 20, Symbols = false },
            2 => new PasswordOptions { Count = Count, Length = 6, Lowercase = false, Uppercase = false, Symbols = false, Mode = PasswordMode.Pin },
            3 => new PasswordOptions { Count = Count, Mode = PasswordMode.Passphrase, WordCount = 10 },
            4 => new PasswordOptions { Count = Count, Mode = PasswordMode.Custom, Length = 24 },
            _ => throw new ArgumentOutOfRangeException(nameof(index))
        };
        Update(preset);
        Status = "Применён профиль: " + Presets[index];
    }
    public void Generate()
    {
        try
        {
            var passwords = generator.Generate(options);
            Output = string.Join(Environment.NewLine, passwords);
            Estimate = $"Верхняя оценка пространства поиска: {generator.GetSearchSpaceUpperBoundBits(options):F1} бит. Это не гарантия стойкости.";
            Status = $"Создано паролей: {passwords.Count}.";
            foreach (var password in passwords.Reverse()) History.Insert(0, password);
            while (History.Count > 50) History.RemoveAt(History.Count - 1);
            Notify(nameof(HistoryText));
        }
        catch (ArgumentException ex)
        {
            Output = "";
            Estimate = "";
            Status = ex.Message;
        }
    }
    public void Clear()
    {
        Output = "";
        Estimate = "";
        History.Clear(); Notify(nameof(HistoryText));
        Status = "Результаты очищены.";
    }
    private void Notify([CallerMemberName] string? name = null) => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
}

internal sealed class ActionCommand(Action action) : ICommand
{
    public bool CanExecute(object? parameter) => true;
    public void Execute(object? parameter) => action();
    public event EventHandler? CanExecuteChanged { add { } remove { } }
}
