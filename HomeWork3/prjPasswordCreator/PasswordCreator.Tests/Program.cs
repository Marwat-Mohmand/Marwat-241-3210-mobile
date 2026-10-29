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

Console.WriteLine($"PASS: {passed} checks (HomeWork3).");
