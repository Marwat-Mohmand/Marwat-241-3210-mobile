# Phase 1 verification

Date: 10 November 2026. Platform: Windows x64. SDK: .NET 10.0.403. Avalonia: 12.1.6.

## Environment

- .NET 10.0.400 and 10.0.403 were available.
- Avalonia templates were initially missing. `Avalonia.Templates@12.1.6` was installed into the repository's local tooling directory.
- NuGet restored the desktop and test packages into the ignored `.dotnet/packages` cache.

## Build and automated tests

The three-project solution builds with **0 warnings and 0 errors** using:

```powershell
dotnet build GeometryTrainer/GeometryTrainer.slnx --no-restore -m:1 -nr:false -p:UseSharedCompilation=false
```

The xUnit project passed **14 tests**, with 0 failed and 0 skipped:

```powershell
dotnet test GeometryTrainer/GeometryTrainer.Tests/GeometryTrainer.Tests.csproj --no-build --no-restore
```

Tests cover rectangle and square formulas, sides of length one, the visible grid maximum, invalid sides, atomic resize validation, one change event per actual resize, swapping sides, null arguments, and large integer calculations.

The initial test run inside the restricted sandbox timed out connecting to testhost. Running the same tests outside that restriction passed in 176 ms. This was a test-runner process communication issue; no test expectations were weakened.

## Native desktop check

The actual Avalonia application was launched on Windows and inspected with the Windows computer-use tool. It displayed the Russian interface, grid, blue rectangle, four blue corner markers, dimension controls, formulas and green reset button.

Observed interactions:

- Start: 6 × 4, P = 20, S = 24.
- Square preset: 4 × 4, P = 16, S = 16; the explanation changes to the square explanation.
- Increase width: 5 × 4, P = 18, S = 20; both the numeric control and slider synchronise.
- The help button opens the Russian explanation; Esc closes it.
- After focusing the grid, the Right key changes 6 × 4 to 7 × 4, with P = 22 and S = 28.
- Changing the height slider to 3 updates the numeric field and gives 7 × 3, P = 20, S = 21.
- Swapping that rectangle gives 3 × 7 while preserving P = 20 and S = 21.
- F1 opens help from the grid; Esc closes it. Reset restores 6 × 4.
- Closing the first preview exits the application normally with exit code 0.

The final build was reopened and visually checked after spacing and colour refinements. The blue slider controls, green action button and full worked formulas are visible at the default window size. The final preview was left open for the user.

The native application's startup was exercised through `dotnet GeometryTrainer.Desktop.dll`. The helper's first attempt to launch the executable directly timed out; no application window had opened from that attempt. The subsequent .NET launch opened the desktop window successfully.

VS Code configurations are included both at repository level and inside the GeometryTrainer folder. Their JSON, launch paths and build-task references are checked separately from native UI testing.

## Phase boundary

This report covers Phase 1. Corner dragging, exercises, scores, statistics, settings screens, themes and JSON state persistence are not implemented or claimed as tested. The original design screenshot was not attached to the request, so the visual implementation was assessed against the written design requirements.
