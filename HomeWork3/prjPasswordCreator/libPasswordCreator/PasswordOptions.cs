namespace PasswordCreator.Core;

public enum PasswordMode { Random }

public sealed record PasswordOptions
{
    public PasswordMode Mode { get; init; } = PasswordMode.Random;
    public int Length { get; init; } = 16;
    public int Count { get; init; } = 5;
    public bool Lowercase { get; init; } = true;
    public bool Uppercase { get; init; } = true;
    public bool Digits { get; init; } = true;
    public bool Symbols { get; init; } = true;

}
