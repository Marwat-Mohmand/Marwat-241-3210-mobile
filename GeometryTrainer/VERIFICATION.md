# Phase 1 verification

## Final verification, 12 November 2026

**PHASE 1 COMPLETE.** Verified on Windows x64 with .NET SDK 10.0.404 and Avalonia 12.1.6.

| # | Check | Final result |
| --- | --- | --- |
| 1 | .NET 10 solution build | Passed. The full rebuild and the build after the fixes both reported zero warnings and zero errors. |
| 2 | Existing unit tests | Passed. 14 passed, 0 failed, 0 skipped. |
| 3 | Desktop launch | Passed. `GeometryTrainer.Desktop.exe` opened the expected native window. |
| 4 | Russian interface | Passed. The window title, learning mode, controls and measurement labels are in Russian. |
| 5 | Grid and rectangle | Passed. The rendered window shows square cells, a blue rectangle, four corner markers and both side labels. |
| 6 | Width and height updates | Passed. Numeric entry and the slider update the grid, both controls and the result labels. |
| 7 | Perimeter and area | Passed. The displayed substitutions and results match both formulas. |
| 8 | Requested dimensions | Passed. All three requested sizes were exercised in the native window. |
| 9 | Invalid dimensions | Failed initially. After the fix, all 12 commit checks passed across the two fields. |
| 10 | Reusable calculation library | Passed. The formulas are in `GeometryTrainer.Core`. The compiled library has no Avalonia reference. |
| 11 | C# design | Passed. `RectangleShape` owns validated dimensions and raises `Changed` after a valid resize. The calculator and ViewModel have distinct responsibilities. |
| 12 | Structure and duplication | Passed. The review found one implementation of each formula and no unused source files. The input issues found during the review were corrected. |
| 13 | VS Code | Passed for launch configuration and build task. Both workspace layouts resolve to the verified desktop binary, and the configured build command succeeded. |

| Width × height | Perimeter | Area |
| --- | ---: | ---: |
| 3 × 4 | 14 units | 12 square units |
| 5 × 5 | 20 units | 25 square units |
| 2 × 8 | 20 units | 16 square units |

The numeric fields now accept whole numbers from 1 through 12. Pressing Enter or moving focus away after invalid input restores the last valid value. The checks covered zero, negative numbers, values above 12, fractions, letters and blank input. The height validation exception now identifies `height` as the invalid argument.

The Windows computer-use helper timed out. An isolated test harness opened the native Avalonia window, exercised its controls and captured a render for visual inspection. The VS Code check covered the profiles and their build command. An interactive F5 session remains untested.

To launch from VS Code, open the repository or the `GeometryTrainer` folder, press **Ctrl+Shift+D**, choose **GeometryTrainer — build + run**, then press **F5**. After a successful build, **GeometryTrainer — fast launch** opens the compiled app.

## Initial verification, 10 November 2026

Platform: Windows x64. SDK: .NET 10.0.403. Avalonia: 12.1.6.

### Environment

- .NET 10.0.400 and 10.0.403 were available.
- Avalonia templates were initially missing. `Avalonia.Templates@12.1.6` was installed into the repository's local tooling directory.
- NuGet restored the desktop and test packages into the ignored `.dotnet/packages` cache.

### Build and automated tests

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

### Native desktop check

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

### Phase boundary

This report covers Phase 1. Corner dragging, exercises, scores, statistics, settings screens, themes and JSON state persistence are not implemented or claimed as tested. The original design screenshot was not attached to the request, so the visual implementation was assessed against the written design requirements.
