using System.Security.Cryptography;

namespace PasswordCreator.Core;

public sealed class PasswordGenerator
{
    public const string Lowercase = "abcdefghijklmnopqrstuvwxyz";
    public const string Uppercase = "ABCDEFGHIJKLMNOPQRSTUVWXYZ";
    public const string Digits = "0123456789";
    public const string Symbols = "!@#$%^&*()-_=+[]{};:,.?";
    public const string Ambiguous = "O0oIl1|";
    private static readonly string[] Words = ("anchor apple autumn bamboo beacon berry bird blossom breeze bridge brook cabin " +
        "cactus candle canyon cedar cherry cloud coast comet coral cotton crane crystal dawn delta dune eagle earth ember " +
        "falcon fern field flame forest frost garden glade globe grape grove harbor hazel hill honey island ivy jasmine " +
        "lake lantern leaf lemon light lilac lotus maple meadow melon mist moon moss mountain ocean olive orchid otter " +
        "panda peach pearl pebble pine planet plum pond poppy quartz rabbit rain raven reef river robin rose sage sand " +
        "shell shore silver sky snow solar sparrow spring spruce star stone storm stream summit sun sunset swift thorn " +
        "tiger timber trail tulip valley velvet violet wave wheat willow wind winter wolf wood wren zenith amber acorn " +
        "apricot badger basil birch butter castle copper daisy dragon elm finch flint fox heron iris juniper kiwi lavender " +
        "lily lynx marble mint morning nectar night oak opal orange palm paper parrot petal polar pondstone pumpkin reed " +
        "ripple ruby seed shadow silk snowdrop sorrel spark summer teal thyme topaz turtle walnut water wild yarrow").Split(' ');
    public static int DictionarySize => Words.Length;

    public IReadOnlyList<string> Generate(PasswordOptions options)
    {
        ArgumentNullException.ThrowIfNull(options);
        if (options.Count is < 1 or > 100)
            throw new ArgumentException("Количество паролей: от 1 до 100.");
        if (!Enum.IsDefined(options.Mode))
            throw new ArgumentException("Неизвестный режим генерации.");
        if (options.Mode == PasswordMode.Passphrase)
        {
            if (options.WordCount is < 4 or > 12)
                throw new ArgumentException("Количество слов: от 4 до 12.");
            if (options.Separator is null || options.Separator.Length > 3 || options.Separator.Any(char.IsControl))
                throw new ArgumentException("Разделитель: до 3 печатных символов.");
            return Enumerable.Range(0, options.Count).Select(_ => string.Join(options.Separator,
                Enumerable.Range(0, options.WordCount).Select(_ => Words[RandomNumberGenerator.GetInt32(Words.Length)]))).ToArray();
        }
        var groups = GetGroups(options);
        var alphabet = string.Concat(groups);
        if (options.Length is < 1 or > 256)
            throw new ArgumentException("Длина пароля: от 1 до 256.");
        if (groups.Count == 0 || groups.Any(group => group.Length == 0))
            throw new ArgumentException("Выберите хотя бы одну группу. Каждая выбранная группа должна содержать доступные символы.");
        if (options.Length < groups.Count)
            throw new ArgumentException("Длина должна быть не меньше количества выбранных групп.");
        if (options.NoRepeatedCharacters && options.Length > alphabet.Length)
            throw new ArgumentException("Для пароля без повторов длина не должна превышать размер доступного алфавита.");
        var passwords = new List<string>(options.Count);
        for (var i = 0; i < options.Count; i++)
        {
            var chars = new List<char>(options.Length);
            // Guarantee one character from each selected, disjoint group.
            foreach (var group in groups)
                chars.Add(group[RandomNumberGenerator.GetInt32(group.Length)]);
            while (chars.Count < options.Length)
            {
                var available = options.NoRepeatedCharacters ? new string(alphabet.Where(c => !chars.Contains(c)).ToArray()) : alphabet;
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
        if (options.Mode == PasswordMode.Pin) return [Filter(Digits, options)];
        if (options.Mode == PasswordMode.Custom)
        {
            var custom = options.CustomAlphabet ?? "";
            if (custom.Length > 1024 || custom.Any(c => c < '!' || c > '~'))
                throw new ArgumentException("Свой алфавит: до 1024 печатных ASCII-символов без пробелов.");
            return [Filter(new string(custom.Distinct().ToArray()), options)];
        }
        var groups = new List<string>();
        if (options.Lowercase) groups.Add(Lowercase);
        if (options.Uppercase) groups.Add(Uppercase);
        if (options.Digits) groups.Add(Digits);
        if (options.Symbols) groups.Add(Symbols);
        groups = groups.Select(group => Filter(group, options)).ToList();
        return groups;
    }

    private static string Filter(string alphabet, PasswordOptions options) => new(alphabet.Where(c =>
!(options.ExcludedCharacters ?? "").Contains(c) && (!options.ExcludeAmbiguous || !Ambiguous.Contains(c))).ToArray());

    // An upper bound on the search space, NOT a measured entropy or cracking time.
    // Mandatory groups and the generation algorithm reduce the actual entropy.
    public double GetSearchSpaceUpperBoundBits(PasswordOptions options)
    {
        if (options.Mode == PasswordMode.Passphrase) return options.WordCount * Math.Log2(Words.Length);
        var size = string.Concat(GetGroups(options)).Length;
        if (size == 0) return 0;
        if (options.NoRepeatedCharacters)
        {
            if (options.Length > size) return 0;
            return Enumerable.Range(0, options.Length).Sum(i => Math.Log2(size - i));
        }
        return options.Length * Math.Log2(size);
    }
}
