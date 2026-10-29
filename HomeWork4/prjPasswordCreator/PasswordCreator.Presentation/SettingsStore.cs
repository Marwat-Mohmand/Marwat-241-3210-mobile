using System.Text.Json;
using PasswordCreator.Core;

namespace PasswordCreator.Presentation;

public static class SettingsStore
{
    public static string DefaultPath => Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
        "prjPasswordCreator", "HomeWork4", "settings.json");

    // Only generation preferences are persisted. Passwords/history stay in memory.
    public static PasswordOptions Load(string path)
    {
        if (!File.Exists(path)) return new();
        return JsonSerializer.Deserialize<PasswordOptions>(File.ReadAllText(path)) ?? new();
    }

    public static void Save(string path, PasswordOptions options)
    {
        Directory.CreateDirectory(Path.GetDirectoryName(Path.GetFullPath(path))!);
        File.WriteAllText(path, JsonSerializer.Serialize(options, new JsonSerializerOptions { WriteIndented = true }));
    }
}
