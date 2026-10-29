using System.Text.Json;
using PasswordCreator.Core;
using PasswordCreator.Presentation;

var passed = 0;
void Check(bool condition, string message)
{
    if (!condition) throw new Exception("FAIL: " + message);
    passed++;
}
void Reject(PasswordOptions options, string message)
{
    try { new PasswordGenerator().Generate(options); throw new Exception("Expected validation: " + message); }
    catch (ArgumentException) { passed++; }
}
var generator = new PasswordGenerator();
// Repeated batches verify invariants, not the random output itself.
for (var mask = 1; mask < 16; mask++)
{
    var options = new PasswordOptions
    {
        Lowercase = (mask & 1) != 0,
        Uppercase = (mask & 2) != 0,
        Digits = (mask & 4) != 0,
        Symbols = (mask & 8) != 0,
        Length = 24,
        Count = 100
    };
    var groups = new List<string>();
    if (options.Lowercase) groups.Add(PasswordGenerator.Lowercase);
    if (options.Uppercase) groups.Add(PasswordGenerator.Uppercase);
    if (options.Digits) groups.Add(PasswordGenerator.Digits);
    if (options.Symbols) groups.Add(PasswordGenerator.Symbols);
    var batch = generator.Generate(options);
    Check(batch.Count == 100, "batch size");
    foreach (var password in batch)
    {
        Check(password.Length == 24, "length");
        Check(password.All(c => string.Concat(groups).Contains(c)), "allowed characters only");
        Check(groups.All(g => password.Any(g.Contains)), "every selected group is present");
    }
}
Check(generator.Generate(new() { Length = 4, Count = 100 }).All(p => p.Any(char.IsLower) && p.Any(char.IsUpper) && p.Any(char.IsDigit) && p.Any(PasswordGenerator.Symbols.Contains)), "minimum length with four groups");
Check(generator.Generate(new() { Length = 256, Count = 1 })[0].Length == 256, "maximum length");
Check(generator.Generate(new() { Length = 1, Count = 1, Lowercase = false, Uppercase = false, Symbols = false })[0].All(char.IsDigit), "single digit");
Reject(new() { Count = 0 }, "zero count"); Reject(new() { Count = 101 }, "count limit");
Reject(new() { Length = 0 }, "zero length"); Reject(new() { Length = 257 }, "length limit");
Reject(new() { Length = 3 }, "too short for required groups");
Reject(new() { Lowercase = false, Uppercase = false, Digits = false, Symbols = false }, "empty alphabet");
Reject(new() { Mode = (PasswordMode)99 }, "undefined mode");
try { generator.Generate(null!); throw new Exception("Expected ArgumentNullException"); }
catch (ArgumentNullException) { passed++; }
Check(Math.Abs(generator.GetSearchSpaceUpperBoundBits(new() { Length = 6, Lowercase = false, Uppercase = false, Symbols = false }) - 6 * Math.Log2(10)) < .0001, "upper bound calculation");
var model = new GeneratorViewModel();
var notifications = 0;
model.PropertyChanged += (_, _) => notifications++;
model.GenerateCommand.Execute(null);
Check(model.Output.Split(Environment.NewLine).Length == model.Count, "view model generates batch");
Check(model.Estimate.Length > 0 && notifications > 0, "observable results");
model.Count = 0; model.Generate();
Check(model.Output == "" && model.Estimate == "" && model.Status.Contains("Количество"), "view model handles validation and clears stale results");
for (var i = 0; i < model.Presets.Count; i++)
{
    model.Count = 2; model.ApplyPreset(i); model.Generate();
    Check(model.Output.Length > 0, "preset generates: " + model.Presets[i]);
}
model.ClearCommand.Execute(null);
Check(model.Output == "" && model.Estimate == "", "clear results");
var pin = new PasswordOptions { Mode = PasswordMode.Pin, Length = 6, Count = 100 };
foreach (var p in generator.Generate(pin)) Check(p.Length == 6 && p.All(char.IsDigit), "PIN mode");
var custom = new PasswordOptions { Mode = PasswordMode.Custom, CustomAlphabet = "aabbcc0011", Length = 5, NoRepeatedCharacters = true, Count = 100 };
foreach (var p in generator.Generate(custom)) Check(p.Length == 5 && p.Distinct().Count() == 5 && p.All("abc01".Contains), "deduplicated custom alphabet without repeats");
Check(generator.Generate(new() { NoRepeatedCharacters = true, Length = 60, Count = 100 }).All(p => p.Distinct().Count() == p.Length), "random passwords without repeats");
var filtered = new PasswordOptions { ExcludeAmbiguous = true, ExcludedCharacters = "!@#abc", Count = 100 };
Check(generator.Generate(filtered).All(p => p.All(c => !(PasswordGenerator.Ambiguous + "!@#abc").Contains(c))), "character exclusions");
Reject(custom with { Length = 6 }, "unique length exceeds alphabet");
Reject(custom with { CustomAlphabet = "" }, "empty custom alphabet");
Reject(custom with { CustomAlphabet = "абв" }, "non-ASCII alphabet");
Reject(custom with { CustomAlphabet = "ab c" }, "space in alphabet");
Reject(new() { ExcludedCharacters = PasswordGenerator.Digits }, "selected group completely excluded");
Reject(pin with { NoRepeatedCharacters = true, Length = 11 }, "PIN without repeats limit");
var phrase = new PasswordOptions { Mode = PasswordMode.Passphrase, Count = 100, WordCount = 10, Separator = ":" };
foreach (var p in generator.Generate(phrase)) Check(p.Split(':').Length == 10 && p.Split(':').All(w => w.All(char.IsAsciiLetterLower)), "passphrase mode");
Check(generator.GetSearchSpaceUpperBoundBits(phrase) == 10 * Math.Log2(PasswordGenerator.DictionarySize), "phrase entropy from actual dictionary size");
Reject(phrase with { WordCount = 3 }, "phrase minimum"); Reject(phrase with { WordCount = 13 }, "phrase maximum");
Reject(phrase with { Separator = "abcd" }, "separator length"); Reject(phrase with { Separator = "\n" }, "control separator");
Check(generator.Generate(phrase with { Separator = "", Count = 1 })[0].All(char.IsAsciiLetterLower), "empty separator");
model.Count = 100; model.ApplyPreset(0); model.Generate();
Check(model.History.Count == 50, "history capped at 50");
model.Clear(); Check(model.History.Count == 0 && model.HistoryText == "", "clear history");
var settingsPath = Path.Combine(AppContext.BaseDirectory, "passwordcreator-tests-" + Guid.NewGuid() + ".json");
try
{
    Check(SettingsStore.Load(settingsPath) == new PasswordOptions(), "missing settings defaults");
    SettingsStore.Save(settingsPath, filtered);
    Check(SettingsStore.Load(settingsPath) == filtered, "settings round trip");
    Check(!File.ReadAllText(settingsPath).Contains("History"), "no history persisted");
    File.WriteAllText(settingsPath, "{broken-json");
    try { SettingsStore.Load(settingsPath); throw new Exception("Expected JsonException"); }
    catch (JsonException) { passed++; }
}
finally { if (File.Exists(settingsPath)) File.Delete(settingsPath); }
Console.WriteLine($"PASS: {passed} checks (HomeWork4).");
