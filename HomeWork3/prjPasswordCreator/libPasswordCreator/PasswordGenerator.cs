using System.Security.Cryptography;

namespace PasswordCreator.Core;

public sealed class PasswordGenerator
{
    public const string Lowercase = "abcdefghijklmnopqrstuvwxyz";
    public const string Uppercase = "ABCDEFGHIJKLMNOPQRSTUVWXYZ";
    public const string Digits = "0123456789";
    public const string Symbols = "!@#$%^&*()-_=+[]{};:,.?";


    public IReadOnlyList<string> Generate(PasswordOptions options)
    {
        ArgumentNullException.ThrowIfNull(options);
        if (options.Count is < 1 or > 100)
            throw new ArgumentException("Количество паролей: от 1 до 100.");
        if (!Enum.IsDefined(options.Mode))
            throw new ArgumentException("Неизвестный режим генерации.");

        var groups = GetGroups(options);
        var alphabet = string.Concat(groups);
        if (options.Length is < 1 or > 256)
            throw new ArgumentException("Длина пароля: от 1 до 256.");
        if (groups.Count == 0 || groups.Any(group => group.Length == 0))
            throw new ArgumentException("Выберите хотя бы одну группу. Каждая выбранная группа должна содержать доступные символы.");
        if (options.Length < groups.Count)
            throw new ArgumentException("Длина должна быть не меньше количества выбранных групп.");

        var passwords = new List<string>(options.Count);
        for (var i = 0; i < options.Count; i++)
        {
            var chars = new List<char>(options.Length);
            // Guarantee one character from each selected, disjoint group.
            foreach (var group in groups)
                chars.Add(group[RandomNumberGenerator.GetInt32(group.Length)]);
            while (chars.Count < options.Length)
            {
                var available = alphabet;
                chars.Add(available[RandomNumberGenerator.GetInt32(available.Length)]);
            }
            // Fisher-Yates removes the fixed positions of required groups.
            for (var j = chars.Count - 1; j > 0; j--)
            {
                var k = RandomNumberGenerator.GetInt32(j + 1);
                (chars[j], chars[k]) = (chars[k], chars[j]);
            }
            passwords.Add(new string(chars.ToArray()));
        }
        return passwords;
    }

    private static List<string> GetGroups(PasswordOptions options)
    {

        var groups = new List<string>();
        if (options.Lowercase) groups.Add(Lowercase);
        if (options.Uppercase) groups.Add(Uppercase);
        if (options.Digits) groups.Add(Digits);
        if (options.Symbols) groups.Add(Symbols);

        return groups;
    }

    // An upper bound on the search space, NOT a measured entropy or cracking time.
    // Mandatory groups and the generation algorithm reduce the actual entropy.
    public double GetSearchSpaceUpperBoundBits(PasswordOptions options)
    {

        var size = string.Concat(GetGroups(options)).Length;
        if (size == 0) return 0;

        return options.Length * Math.Log2(size);
    }
}
