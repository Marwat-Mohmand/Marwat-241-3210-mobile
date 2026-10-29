namespace PasswordCreator.Core;

public enum PasswordMode { Random, Pin, Passphrase, Custom }

public sealed record PasswordOptions
{
    public PasswordMode Mode { get; init; } = PasswordMode.Random;
    public int Length { get; init; } = 20;
    public int Count { get; init; } = 5;
    public bool Lowercase { get; init; } = true;
    public bool Uppercase { get; init; } = true;
    public bool Digits { get; init; } = true;
    public bool Symbols { get; init; } = true;
    public bool ExcludeAmbiguous { get; init; }
    public bool NoRepeatedCharacters { get; init; }
    public string ExcludedCharacters { get; init; } = "";
    public string CustomAlphabet { get; init; } = "abcdef0123456789";
    public int WordCount { get; init; } = 10;
    public string Separator { get; init; } = "-";
}
