# Understanding the Phase 1 code

## 1. A class describes an object

`RectangleShape` is a **class**: it describes what a rectangle knows and what it can do. This line creates one rectangle object:

```csharp
var rectangle = new RectangleShape(6, 4);
```

`new` creates the object. The **constructor**, `RectangleShape(int width, int height)`, receives the starting width and height. Both values must be positive integers.

`Width` and `Height` are **properties**. Other code can read them, but `private set` prevents arbitrary code from changing them directly. This is **encapsulation**: the class controls its own valid state.

To change the rectangle, call a **method**:

```csharp
rectangle.Resize(8, 3);
```

`Resize` validates both numbers before changing either property. If a number is zero or negative, it throws an `ArgumentOutOfRangeException`; the old rectangle stays unchanged. The tests demonstrate how exception handling catches invalid values. The desktop controls restrict inputs to the visible grid's range of 1–12.

The reusable model does not know about a 12 × 12 screen. It can represent any positive integer sides. The desktop ViewModel applies the screen's limit.

## 2. Mathematical calculations belong in Core

`GeometryCalculator` is a **static class**. It has no changing state, so you call its methods without creating a calculator object:

```csharp
long perimeter = GeometryCalculator.CalculatePerimeter(rectangle);
long area = GeometryCalculator.CalculateArea(rectangle);
```

The formulas are implemented entirely in C#:

```csharp
return 2L * ((long)rectangle.Width + rectangle.Height); // perimeter
return (long)rectangle.Width * rectangle.Height;       // area
```

`long` is a larger integer type than `int`. The casts happen before multiplication/addition so very large side lengths do not overflow a 32-bit integer. For the starting 6 × 4 rectangle, perimeter is 20 and area is 24.

`GeometryTrainer.Core` references no Avalonia packages. A future Console or another desktop application can reuse this library.

## 3. An event announces a change

The rectangle declares:

```csharp
public event EventHandler? Changed;
```

After a successful resize, it raises the event. The ViewModel subscribes to that event and announces that its displayed values have changed. Resizing to the same dimensions does not send a redundant notification.

## 4. The ViewModel connects C# to the screen

`MainWindowViewModel` holds the rectangle and exposes properties the screen needs: width, height, perimeter, area and formula text. It asks `GeometryCalculator` for the answers.

It inherits from `ViewModelBase`. This base class contains the common property-notification code. This is useful **inheritance**, because future ViewModels can share the same notification behaviour.

`ViewModelBase` implements the standard **INotifyPropertyChanged interface**. An interface describes a contract. Avalonia understands this contract and refreshes bindings when a property changes.

`RelayCommand` implements the standard **ICommand interface**. It connects a button click to a C# action. For example, the reset command calls `Resize(6, 4)` on the same rectangle. We do not invent extra interfaces or base classes without a practical need.

The grid stores its four drawing positions in a small `Point[]` collection and loops over it to draw all four corner markers. Exercise collections will be added when exercise functionality exists.

## 5. XAML describes the interface

`Views/MainWindow.axaml` contains Avalonia's XAML markup: grids, cards, buttons, labels, numeric controls and sliders. `Styles/Theme.axaml` defines reusable colours and styles.

For example:

```xml
<NumericUpDown Minimum="1" Maximum="12"
               Value="{Binding RectangleWidth, Mode=TwoWay}" />
```

`Binding` connects the control to the ViewModel property. `TwoWay` means both directions:

1. Editing the numeric control updates `RectangleWidth` in C#.
2. Changing `RectangleWidth` in C# updates the numeric control.

The slider uses the same property, so it stays in sync. The rectangle drawing also binds to that width and height. Calculated text binds to `Perimeter`, `Area`, and the formula properties.

In `App.axaml.cs`, the line below creates the relationship:

```csharp
desktop.MainWindow = new MainWindow { DataContext = new MainWindowViewModel() };
```

`DataContext` tells the window where to find the properties named in its bindings. `x:DataType` lets Avalonia check those bindings when compiling the XAML.

The complete flow is:

```text
Numeric control or slider
  → ViewModel property
  → RectangleShape.Resize
  → Changed event
  → PropertyChanged notifications
  → Labels and rectangle drawing update
```

## 6. Drawing and keyboard code stay in Desktop

`RectangleGrid` inherits from Avalonia's `Control`. Its `Render` method draws actual lines, a filled blue rectangle, labels and four blue circles. It calculates screen positions, not mathematical perimeter or area. Both screen axes use the same cell size, so one square unit stays square.

`SideA` and `SideB` are Avalonia **styled properties**. Their bindings receive the ViewModel dimensions. `AffectsRender` asks Avalonia to redraw when either one changes.

`MainWindow.axaml.cs` handles a few keyboard events. It forwards dimension changes to the ViewModel and opens/closes help. It contains no geometry formulas. This small amount of view-specific code is appropriate with MVVM.

## 7. Tests give repeatable evidence

`GeometryTests` uses xUnit. `[Fact]` describes a single test; `[Theory]` runs one test method with several sets of `[InlineData]`.

The tests cover known rectangle/square results, invalid sides, atomic resizing, events, swapping sides, null arguments and large integers. Run them with:

```powershell
dotnet test GeometryTrainer.Tests/GeometryTrainer.Tests.csproj
```

Phase 1 stops here. JSON state, exercises, statistics and corner dragging will be designed after you approve the foundation.
