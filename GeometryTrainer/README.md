# GeometryTrainer — Phase 1

**University project #36:** «Тренажёр определения периметра и площади прямоугольника».

This is a C# .NET 10 desktop application built with Avalonia XAML. The interface is in Russian. The code explanations in this folder are in simple English.

## What Phase 1 contains

- A light educational interface with rounded cards, green action buttons and a blue rectangle.
- A square 12 × 12 grid with four blue corner markers. Cells stay square when the window changes size.
- Width and height from 1 to 12 whole units, using numeric fields and sliders.
- Immediate perimeter and area results, with the numbers substituted into each formula.
- Swap sides, try a 4 × 4 square, and reset to 6 × 4.
- Arrow keys when the grid has focus; F1 opens help, Esc closes it.
- A separate reusable C# library and xUnit tests for the formulas and model.

The four corner markers identify the rectangle's vertices. **Dragging corners is reserved for a later phase.** Exercises, scoring, statistics, dark theme, configurable grid size, and JSON save/load are also reserved for later phases. Phase 1 does not contain pretend exercise buttons or saved progress.

No new design screenshot was present with the project request. The interface follows the written design requirements; we can compare it with the teacher's image when that reference is available.

## Open and run in VS Code

1. Choose **File → Open Folder**.
2. Open `C:\Marwat-241-3210-mobile\GeometryTrainer`.
3. Open **Run and Debug** with **Ctrl+Shift+D**.
4. Select **GeometryTrainer — build + run** in the top dropdown.
5. Press **F5**, or click the green triangle. A separate desktop window opens.
6. Change **Ширина** or **Высота** and watch the shape, perimeter and area change.

After the first build, use **GeometryTrainer — fast launch** when you have not changed the code. Use **build + run** after editing code. The build only compiles the GeometryTrainer application and its library, not the other homework projects.

You can also keep the entire repository open: the repository's `.vscode` folder contains the same two GeometryTrainer launch options alongside the earlier homework launchers. The project also has its own configuration, so it can be opened separately.

Install the Microsoft **C#** extension if it is missing. **C# Dev Kit** is optional. Avalonia's editor extension is optional for running the application; it can help when editing XAML.

## Environment check and installation commands

The development computer has .NET SDK **10.0.400** and **10.0.403**. Avalonia templates were initially absent. We installed **Avalonia.Templates 12.1.6** into the repository's local `.dotnet` tooling area, and the desktop project pins Avalonia packages to **12.1.6**.

On another computer, check:

```powershell
dotnet --list-sdks
dotnet new list avalonia
```

If .NET 10 is missing on Windows, install it, then reopen VS Code:

```powershell
winget install --id Microsoft.DotNet.SDK.10 --exact
```

If the Avalonia templates are missing:

```powershell
dotnet new install Avalonia.Templates@12.1.6
```

Templates are only needed to create new Avalonia projects. Running this existing project requires the .NET SDK and its NuGet packages. See the official [Avalonia installation guide](https://docs.avaloniaui.net/docs/get-started) and [Microsoft .NET downloads](https://dotnet.microsoft.com/download/dotnet/10.0).

## Build, test and run from a terminal

These commands assume the terminal is inside the **GeometryTrainer** folder:

```powershell
dotnet restore GeometryTrainer.slnx
dotnet build GeometryTrainer.slnx --no-restore
dotnet test GeometryTrainer.Tests/GeometryTrainer.Tests.csproj --no-build --no-restore
dotnet run --project GeometryTrainer.Desktop --no-build
```

The included `NuGet.Config` downloads public packages from NuGet and caches them under the repository's ignored `.dotnet/packages` folder. Internet is needed for the first restore. Future builds normally use the cache. On a restricted environment where .NET build-server connections are blocked, use:

```powershell
$env:MSBuildEnableWorkloadResolver = 'false'
dotnet build GeometryTrainer.slnx --no-restore -m:1 -nr:false -p:UseSharedCompilation=false
```

The VS Code task already includes those build options. It does not require MAUI or a browser.

## A simple demonstration for your teacher

| Action | Width × height | Perimeter | Area |
| --- | --- | --- | --- |
| Start or reset | 6 × 4 | 20 units | 24 square units |
| Set width 8, height 3 | 8 × 3 | 22 units | 24 square units |
| Try a square | 4 × 4 | 16 units | 16 square units |
| Set both sides to 12 | 12 × 12 | 48 units | 144 square units |

Swapping the sides changes the orientation but preserves perimeter and area. Click the grid and press the arrow keys: left/right adjust width; up/down adjust height. Numeric fields and sliders support their own normal keyboard controls. F1 opens the explanation; Esc closes it. Enter-to-check belongs to the later testing mode.

## Structure

```text
GeometryTrainer/
  GeometryTrainer.Core/
    Models/RectangleShape.cs
    Services/GeometryCalculator.cs
  GeometryTrainer.Desktop/
    Views/MainWindow.axaml
    Views/MainWindow.axaml.cs
    Views/RectangleGrid.cs
    ViewModels/
    Styles/Theme.axaml
  GeometryTrainer.Tests/GeometryTests.cs
  .vscode/
  GeometryTrainer.slnx
  README.md
  LEARNING.md
```

Read [LEARNING.md](LEARNING.md) for a beginner explanation of the C# classes, events, commands and XAML bindings. Validation results and any environment limitations are recorded in [VERIFICATION.md](VERIFICATION.md).

The project folder can be committed with the rest of this Git repository. Build output and package caches are ignored. Phase 2 begins only after your approval.
