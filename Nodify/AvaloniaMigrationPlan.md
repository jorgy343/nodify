# Avalonia migration plan (Nodify project)

## Goals and constraints
- Port the Nodify control library in place from WPF to Avalonia 11.3.11.
- Support only .NET 10 with C# 14 features; drop older target frameworks and WPF-specific SDK usage.
- Modernize APIs where useful (no backward compatibility requirement).
- Scope: Nodify project directory only; Examples will be migrated later.

## Current snapshot (WPF)
- Project file: `Nodify.csproj` targets multiple `net*-windows` frameworks with `<UseWPF>true>`; build fails on non-Windows with NETSDK1100.
- Code footprint: 93 `.cs` files, 22 `.xaml` theme files, ~236 `DependencyProperty` registrations.
- WPF-only dependencies and patterns: `MultiSelector`, `Adorner/AdornerLayer`, `StyleTypedProperty`, `pack://` URIs, `Mouse*` events, `FrameworkElement` base usage, routed events, and behaviors for keyboard navigation.
- Packaging: signed assembly with `Nodify.snk`, NuGet metadata already present.

## Phase plan
1) Project system and tooling (net10/C#14/Avalonia SDK)
   - Replace `Microsoft.NET.Sdk.WindowsDesktop` with `Avalonia.NET.Sdk/11.3.11`.
   - Target `net10.0`; set `<LangVersion>preview` (for C# 14) and remove `UseWPF`.
   - Add Avalonia packages: `Avalonia`, `Avalonia.Controls`, `Avalonia.Controls.DataGrid` (if needed), `Avalonia.Themes.Fluent`, `Avalonia.Diagnostics` (debug only), `Avalonia.Headless` (tests).
   - Update resource build actions (`AvaloniaResource`, `AvaloniaXaml`) and keep signing metadata.
   - Adjust solution configuration to skip Examples during the first pass.

2) XAML to AXAML conversion
   - Rename all 22 theme files to `.axaml`; update `ResourceDictionary` sources to `avares://Nodify/` URIs.
   - Swap default namespaces to Avalonia (`https://github.com/avaloniaui`) and remove WPF-specific XML namespaces.
   - Replace pack URIs (`pack://application...`) with `avares://` and adjust `BuildAction` to `AvaloniaResource`.
   - Convert control styles/templates to Avalonia equivalents (e.g., `ContentPresenter` -> Avalonia variant, `Grid` definitions kept, `Setter.Value` types updated).

3) DependencyProperty and routed event migration
   - Replace every `DependencyProperty.Register` with `AvaloniaProperty.Register`/`StyledProperty`/`DirectProperty`; move metadata callbacks to Avalonia patterns.
   - Swap `GetValue/SetValue` with `GetValue/SetValue` on `AvaloniaObject`; use `SetCurrentValue` equivalents where needed.
   - Replace routed events with Avalonia `RoutedEvent` registrations where applicable or use standard .NET events.

4) Control base and input model updates
   - Change base types from `FrameworkElement/Control` hierarchy to Avalonia `Control`/`TemplatedControl`; remove metadata overrides (`OverrideMetadata`, `StyleTypedProperty`).
   - Convert `Mouse*` and `Stylus*` events/gestures to `Pointer*` (`PointerPressed/Released/WheelChanged/Moved/CapturedLost`).
   - Update focus and keyboard navigation to Avalonia (`Focusable`, `FocusManager.Instance`, `KeyGesture` with `InputGestureCollection` equivalents).
   - Replace `MultiSelector` with Avalonia selection primitives (`ListBox`/`ItemsControl` + `SelectionModel`), updating `SelectedItems` handling and keyboard layer logic.

5) Overlay/adorner and visual tree features
   - Replace `Adorner` usage with Avalonia `AdornerLayer`/`AdornerDecorator` or custom overlay panels; migrate focus visuals to templated adorners or pseudo-classes.
   - Substitute `VisualTreeHelper` calls with `Visual`/`IVisual` traversal helpers (`GetVisualParent`, `GetVisualChildren` or logical tree).
   - Replace `Popup`/`ToolTip` behaviors with Avalonia counterparts where used.

6) Theming, resources, and assets
   - Introduce `App.axaml`-style theme resources for controls; ensure `Generic.axaml` merges `Styles/Controls.axaml` via `avares://`.
   - Convert `StyleTypedProperty` attributes into dedicated style resources documented in `Generic.axaml`.
   - Define brushes, pens, geometries using Avalonia types; update focus visuals, animations, and storyboards to Avalonia transitions.

7) Control-specific rewrites
   - NodifyEditor & selection: reimplement selection rubber-band, keyboard layers, and auto-pan using Avalonia pointer capture and `Bounds` APIs.
   - Connections & decorators: migrate connection rendering (possibly to `Control` with `OnRender` using `DrawingContext`), replace hotkey adorners with overlay controls.
   - Minimap & viewport: use `RenderTransform`/`Matrix` in Avalonia; update zoom/pan helpers and mouse wheel handling.
   - Cutting line/pushing items: migrate geometry hit-testing to Avalonia `Geometry`/`Rect`/`Bounds` utilities.

8) Testing and validation
   - Add headless UI smoke tests with `Avalonia.Headless` for key controls (NodifyEditor, Connection, PendingConnection) to ensure property bindings and templates load.
   - Run `dotnet build Nodify/Nodify.csproj` on Linux/macOS CI, then targeted headless tests.
   - Manual validation checklist: resource lookup, pointer interactions (drag/select), keyboard navigation layers, zoom/pan, hotkey overlays.

9) Packaging and documentation
   - Update `README` and nuspec metadata to reflect Avalonia-only support and net10 requirement.
   - Verify signing with existing `Nodify.snk`; publish package version bump once migration stabilizes.

## WPF → Avalonia mapping highlights
- DependencyProperty → AvaloniaProperty (StyledProperty/DirectProperty); `FrameworkPropertyMetadata` → property registration metadata.
- pack:// URIs → avares:// URIs.
- Mouse events → Pointer events; `MouseWheelDelta` → `PointerWheelEventArgs.Delta`.
- MultiSelector/SelectedItems → SelectionModel or `ListBox` with extended selection.
- Adorner/AdornerLayer → AdornerLayer/AdornerDecorator or overlay controls/pseudo-classes.
- `StyleTypedProperty` → explicit styles in `Generic.axaml`.
- `FrameworkElement.Loaded/Unloaded` → `AttachedToVisualTree`/`DetachedFromVisualTree`.
- `DispatcherTimer/Dispatcher` → Avalonia `Dispatcher.UIThread`.

## Execution order
1) Convert project file and references (Phase 1).
2) Rename XAML to AXAML and fix resources (Phase 2).
3) Bulk migrate dependency properties and events (Phase 3–4).
4) Rewrite control-specific behaviors (Phase 5–7) with focused tests.
5) Final theming, packaging, and docs (Phase 8–9).
