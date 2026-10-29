using System.Text;
using System.Text.Json;
using PasswordCreator.Core;

Console.OutputEncoding = Encoding.UTF8;
try
{
    var options = new PasswordOptions();
    string? outputPath = null;
    if (args.Length == 0)
    {
        Console.WriteLine("Генератор паролей · HomeWork4");
        Console.WriteLine("Режим: 0 — пароль, 1 — PIN, 2 — фраза, 3 — свой алфавит");
        options = options with { Mode = (PasswordMode)ReadInt("Режим", 0) };
        if (options.Mode != PasswordMode.Passphrase)
        {
            options = options with { Length = ReadInt("Длина", options.Length) };
            if (options.Mode == PasswordMode.Random)
            {
                options = options with
                {
                    Lowercase = ReadBool("Строчные буквы"),
                    Uppercase = ReadBool("Прописные буквы"),
                    Digits = ReadBool("Цифры"),
                    Symbols = ReadBool("Спецсимволы")
                };
            }
        }
        if (options.Mode == PasswordMode.Passphrase) options = options with { WordCount = ReadInt("Количество слов", options.WordCount) };
        if (options.Mode == PasswordMode.Custom)
        {
            Console.Write("Свой алфавит: "); options = options with { CustomAlphabet = Console.ReadLine() ?? "" };
        }
        if (options.Mode != PasswordMode.Passphrase)
            options = options with { ExcludeAmbiguous = ReadBool("Исключать похожие символы"), NoRepeatedCharacters = ReadBool("Без повторов символов") };
        options = options with { Count = ReadInt("Количество", options.Count) };
    }
    else
    {
        for (var i = 0; i < args.Length; i++)
        {
            string Value()
            {
                if (++i >= args.Length) throw new ArgumentException("Не указано значение параметра " + args[i - 1]);
                return args[i];
            }
            switch (args[i])
            {
                case "--help":
                    Console.WriteLine("--length N --count N --groups lud s (без пробела: l=abc, u=ABC, d=123, s=!@#) --output FILE --mode Random|Pin|Passphrase|Custom --words N --separator TEXT --alphabet TEXT --exclude TEXT --no-ambiguous --unique --settings FILE");
                    return 0;
                case "--length": options = options with { Length = ParseInt(Value()) }; break;
                case "--count": options = options with { Count = ParseInt(Value()) }; break;
                case "--groups":
                    var groups = Value();
                    if (groups.Any(c => !"luds".Contains(c))) throw new ArgumentException("Группы задаются буквами l, u, d, s.");
                    options = options with { Lowercase = groups.Contains('l'), Uppercase = groups.Contains('u'), Digits = groups.Contains('d'), Symbols = groups.Contains('s') };
                    break;
                case "--output": outputPath = Value(); break;
                case "--mode":
                    if (!Enum.TryParse<PasswordMode>(Value(), true, out var mode) || !Enum.IsDefined(mode)) throw new ArgumentException("Неизвестный режим.");
                    options = options with { Mode = mode }; break;
                case "--words": options = options with { WordCount = ParseInt(Value()) }; break;
                case "--separator": options = options with { Separator = Value() }; break;
                case "--alphabet": options = options with { CustomAlphabet = Value() }; break;
                case "--exclude": options = options with { ExcludedCharacters = Value() }; break;
                case "--no-ambiguous": options = options with { ExcludeAmbiguous = true }; break;
                case "--unique": options = options with { NoRepeatedCharacters = true }; break;
                case "--settings": options = JsonSerializer.Deserialize<PasswordOptions>(File.ReadAllText(Value())) ?? new(); break;
                default: throw new ArgumentException("Неизвестный параметр: " + args[i]);
            }
        }
    }
    var generator = new PasswordGenerator();
    var passwords = generator.Generate(options);
    foreach (var password in passwords) Console.WriteLine(password);
    Console.Error.WriteLine($"Верхняя оценка пространства поиска: {generator.GetSearchSpaceUpperBoundBits(options):F1} бит; не гарантия стойкости.");
    if (outputPath is not null)
    {
        File.WriteAllLines(outputPath, passwords, new UTF8Encoding(false));
        Console.Error.WriteLine("Сохранено: " + Path.GetFullPath(outputPath));
    }
    return 0;
}
catch (Exception ex) when (ex is ArgumentException or IOException or UnauthorizedAccessException or JsonException)
{
    Console.Error.WriteLine("Ошибка: " + ex.Message);
    return 1;
}

static int ParseInt(string value) => int.TryParse(value, out var number) ? number : throw new ArgumentException("Введите целое число.");
static int ReadInt(string label, int fallback)
{
    Console.Write($"{label} [{fallback}]: ");
    var value = Console.ReadLine();
    return string.IsNullOrWhiteSpace(value) ? fallback : ParseInt(value);
}
static bool ReadBool(string label)
{
    Console.Write(label + " [Y/n]: ");
    var value = Console.ReadLine()?.Trim().ToLowerInvariant();
    if (string.IsNullOrEmpty(value) || value is "y" or "yes" or "д" or "да") return true;
    if (value is "n" or "no" or "н" or "нет") return false;
    throw new ArgumentException("Введите y/да или n/нет.");
}
