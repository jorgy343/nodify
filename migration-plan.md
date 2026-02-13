# Nodify: WPF → Avalonia 11.3 Migration Plan

## Context

Nodify is a WPF control library (88 C# files, 22 XAML files, ~182 DependencyProperties, 17+ custom controls) for building MVVM node-based editors. It has **zero external dependencies** — only WPF.

We are converting it **in-place** on the `refactor` branch to target **Avalonia 11.3.11**, **.NET 10**, **C# 14**. Full API modernization is permitted. The Examples directory is out of scope.

---

## Key Technical Decisions

| WPF Concept | Avalonia Equivalent |
|---|---|
| `DependencyProperty.Register` | `AvaloniaProperty.Register<TOwner, TValue>()` → `StyledProperty<T>` |
| `DependencyProperty.RegisterReadOnly` | `AvaloniaProperty.RegisterDirect<TOwner, TValue>()` → `DirectProperty<T>` with `SetAndRaise` |
| `DependencyProperty.RegisterAttached` | `AvaloniaProperty.RegisterAttached<TOwner, THost, TValue>()` → `AttachedProperty<T>` |
| `FrameworkPropertyMetadata(AffectsRender)` | Call `AffectsRender<TControl>(property)` in static constructor |
| `FrameworkPropertyMetadata(BindsTwoWayByDefault)` | Pass `defaultBindingMode: BindingMode.TwoWay` in registration |
| `CoerceValueCallback` | Use `.AddCoerce()` or validate in property changed handler |
| `EventManager.RegisterRoutedEvent` | `RoutedEvent.Register<TOwner, TArgs>()` with `RoutingStrategies.Bubble` |
| `MultiSelector` (base class) | `SelectingItemsControl` + custom selection logic |
| `Shape` (base class) | Avalonia `Shape` — has `CreateDefiningGeometry()` and `Render(DrawingContext)` |
| `OnRender(DrawingContext)` | `public override void Render(DrawingContext context)` |
| `DefiningGeometry` property | `protected abstract Geometry CreateDefiningGeometry()` |
| `StreamGeometry` / `StreamGeometryContext` | Same classes exist in `Avalonia.Media` |
| `OnMouseDown/Up/Move/Wheel` | `OnPointerPressed/Released/Moved/WheelChanged` |
| `MouseButtonEventArgs` | `PointerPressedEventArgs` / `PointerReleasedEventArgs` |
| `Mouse.Capture` / `CaptureMouse()` | `e.Pointer.Capture(control)` / `e.Pointer.Capture(null)` |
| `IsMouseOver` | `IsPointerOver` |
| `Mouse.MouseWheelDeltaForOneLine` | Constant `120` (or use `e.Delta.Y`) |
| `VisualTreeHelper.GetParent()` | `visual.GetVisualParent()` extension |
| `VisualTreeHelper.GetChildrenCount/GetChild` | `visual.GetVisualChildren()` |
| `VisualTreeHelper.HitTest()` | Custom implementation using `InputHitTest` or visual tree walk |
| `VisualTreeHelper.GetDpi()` | Not needed — Avalonia uses device-independent pixels |
| `RoutedUICommand` + `CommandManager` | Replace with simple `ICommand` static properties + handle in control |
| `DefaultStyleKeyProperty.OverrideMetadata` | Not needed — Avalonia uses explicit style inclusion |
| `Triggers` in XAML | Avalonia style selectors + pseudo-classes |
| `TemplateBinding` | `TemplateBinding` (works in Avalonia) |
| `BooleanToVisibilityConverter` | Use `IsVisible` property directly (it's `bool` in Avalonia) |
| `Visibility.Collapsed` | `IsVisible = false` |
| `pack://application:` URIs | `avares://Nodify/` URIs |
| `o:Freeze="True"` | Remove — not needed in Avalonia |
| `BeginAnimation` / `DoubleAnimation` | Avalonia `Transitions` API or `Animatable.Animate()` |
| `BitmapCache` | Remove — Avalonia has different rendering optimization |
| `Adorner` / `AdornerLayer` | Avalonia `AdornerLayer` (different API) or popup/overlay |
| `DispatcherTimer` | `DispatcherTimer` (exists in `Avalonia.Threading`) |
| `FormattedText` | `FormattedText` exists in Avalonia; use `TextLayout` for drawing |
| `Geometry.Combine` / `GetWidenedPathGeometry` | May need alternative approach for focus visuals |
| `ComponentResourceKey` | Standard string-based resource keys |
| `ContentPresenter` | Exists in Avalonia |
| `HeaderedContentControl` | Exists in Avalonia |
| `ItemContainerGenerator.ContainerFromIndex` | `ItemsControl.ContainerFromIndex()` (built-in in Avalonia 11) |
| `Thumb` | Exists in Avalonia (`Avalonia.Controls.Primitives.Thumb`) |
| `.xaml` files | Rename to `.axaml` |
| `Generic.xaml` auto-loading | Create explicit `NodifyTheme : Styles` class |
| `DataTemplateSelector` | `IDataTemplate` with `Match()` method |

---

## Migration Phases

### Phase 1: Project Infrastructure
**Files:** `Nodify.csproj`, `Properties/AssemblyInfo.cs`

1. **Convert `Nodify.csproj`:**
   - Change SDK from `Microsoft.NET.Sdk.WindowsDesktop` → `Microsoft.NET.Sdk`
   - Set `<TargetFramework>net10.0</TargetFramework>` (single target)
   - Remove `<UseWPF>true</UseWPF>`
   - Add `<LangVersion>14</LangVersion>`
   - Add Avalonia NuGet packages:
     ```xml
     <PackageReference Include="Avalonia" Version="11.3.11" />
     ```
   - Remove strong-naming (`AssemblyOriginatorKeyFile`, `SignAssembly`)
   - Remove old TFM-conditional `PropertyGroup`s
   - Update package tags

2. **Convert `AssemblyInfo.cs`:**
   - Remove `using System.Windows` and `System.Windows.Markup`
   - Remove `[assembly: ThemeInfo(...)]`
   - Replace `[assembly: XmlnsDefinition(...)]` with Avalonia equivalent:
     ```csharp
     using Avalonia.Metadata;
     [assembly: XmlNamespace("https://miroiu.github.io/nodify", "Nodify")]
     ```
   - Remove `[assembly: AssemblyKeyFile(...)]`

3. **Create theme infrastructure class** `NodifyTheme.cs`:
   ```csharp
   public class NodifyTheme : Styles
   {
       public NodifyTheme(IServiceProvider? sp = null)
       {
           AvaloniaXamlLoader.Load(sp, this);
       }
   }
   ```

---

### Phase 2: Core Utilities
**Files:** `Utilities/BoxValue.cs`, `Utilities/MathExtensions.cs`, `Utilities/SelectionHelper.cs`, `Utilities/WeakReferenceCollection.cs`, `Utilities/DependencyObjectExtensions.cs`, `Utilities/UnscaleTransformConverter.cs`, `Utilities/EditorGesturesExtensions.cs`

1. **`BoxValue.cs`**: ✅ Already converted to Avalonia types

2. **`MathExtensions.cs`**: Update `System.Windows.Point`/`Vector`/`Rect` → `Avalonia.Point`/`Avalonia.Vector`/`Avalonia.Rect`

3. **`SelectionHelper.cs`**: ✅ Already converted to Avalonia types

4. **`WeakReferenceCollection.cs`**: Likely no changes (pure C#)

5. **`DependencyObjectExtensions.cs` → rename to `VisualExtensions.cs`**:
   - `GetParentOfType<T>` → use `visual.GetVisualParent()` in loop
   - `GetChildOfType<T>` → use `visual.GetVisualChildren()`
   - `GetElementAtPosition<T>` → rewrite using `InputHitTest()` or visual tree walk
   - `GetIntersectingElements` (Geometry overload) → rewrite using visual tree walk + bounds checking
   - `GetIntersectingElements<T>` (Rect overload) → update tree traversal API
   - **Animation methods** (`StartAnimation`, `CancelAnimation`, `StartLoopingAnimation`):
     - Rewrite using Avalonia's animation system (`Animation` class or `Transitions`)
     - `BeginAnimation(dp, animation)` → Use Avalonia's `Animatable` or custom tween approach

6. **`UnscaleTransformConverter.cs`**: Update to implement `Avalonia.Data.Converters.IValueConverter` and `IMultiValueConverter`

7. **`EditorGesturesExtensions.cs`**: Update after interactivity framework conversion

---

### Phase 3: Interactivity Framework
**Files:** All files in `Interactivity/` (18 files)

1. **`IInputHandler.cs`**: Change `InputEventArgs` → Avalonia equivalent. In Avalonia, input events use `RoutedEventArgs` subtypes (`PointerPressedEventArgs`, `KeyEventArgs`, etc.). Create a common processing approach — either use `RoutedEventArgs` base or create an adapter.

2. **`InputProcessor.cs`**: Update `ProcessEvent(InputEventArgs e)` to accept Avalonia event types. The `InputEventArgs` base doesn't exist in Avalonia — use `RoutedEventArgs` instead.

3. **`InputProcessor.Shared.cs`**: Update handler registrations for Avalonia event types.

4. **`DragState.cs`**: Mouse capture → Pointer capture. `Mouse.GetPosition()` → `e.GetPosition()`.

5. **`InputElementState.cs` / `InputElementStateStack.cs`**: Update mouse/keyboard event handling to Avalonia pointer/key events.

6. **Gesture classes** (`MouseGesture.cs`, `AnyGesture.cs`, `AllGestures.cs`, `MultiGesture.cs`, `InputGestureRef.cs`, `KeyComboGesture.cs`):
   - WPF `System.Windows.Input.MouseGesture` → Custom gesture matching against Avalonia pointer events
   - WPF `MouseButton` → Avalonia `PointerUpdateKind` or check `Properties.IsLeftButtonPressed`
   - WPF `ModifierKeys` → Avalonia `KeyModifiers`
   - WPF `MouseAction` → Custom enum or direct pointer event checking

7. **`EditorGestures.cs`**: Update all gesture definitions for Avalonia input types. The `KeyGesture` class exists in Avalonia.

8. **Keyboard navigation** (`DirectionalFocusNavigator.cs`, `LinearFocusNavigator.cs`, `StatefulFocusNavigator.cs`, `IKeyboardNavigationLayer.cs`):
   - `Keyboard.Focus()` → `control.Focus()`
   - `FocusManager` → Avalonia `FocusManager`

---

### Phase 4: Event Types
**Files:** All `Events/` subfolders (6 files)

1. **`ConnectionEventArgs.cs`**: Base class `RoutedEventArgs` exists in Avalonia
2. **`ConnectorEventArgs.cs`**: Same
3. **`PendingConnectionEventArgs.cs`**: Same
4. **`ItemsMovedEventArgs.cs`**: Same
5. **`ResizeEventArgs.cs`**: Same
6. **`ZoomEventArgs.cs`**: Same
7. **`PreviewLocationChanged.cs`**: Same

All need `System.Windows.RoutedEventArgs` → `Avalonia.Interactivity.RoutedEventArgs`. Custom handler delegate types may need updating.

---

### Phase 5: Containers & Canvas
**Files:** `Editor/NodifyCanvas.cs`, `Containers/ItemContainer.cs`, `Containers/DecoratorContainer.cs`, `Containers/DecoratorsControl.cs`, `Connections/ConnectionContainer.cs`, `Connections/ConnectionsMultiSelector.cs`, plus their state files

1. **`NodifyCanvas.cs`** (Panel):
   - `System.Windows.Controls.Panel` → `Avalonia.Controls.Panel`
   - `MeasureOverride` / `ArrangeOverride` → Same method signatures in Avalonia
   - `INodifyCanvasItem` interface: Update property types
   - `Canvas.Left`/`Canvas.Top` attached properties → Use Avalonia `Canvas.SetLeft/SetTop` or custom positioning

2. **`ItemContainer.cs`** (ContentControl):
   - Convert all ~10 DPs to `StyledProperty<T>` or `DirectProperty<T>`
   - `DefaultStyleKeyProperty.OverrideMetadata` → Remove
   - `OnRenderSizeChanged` → Subscribe to `SizeChanged` event or override
   - Add pseudo-classes for states: `:selected`, `:dragging`, `:previewingselection`, etc.
   - Mouse events → Pointer events

3. **`DecoratorContainer.cs`**: Similar to ItemContainer

4. **`DecoratorsControl.cs`** (ItemsControl):
   - `System.Windows.Controls.ItemsControl` → `Avalonia.Controls.ItemsControl`
   - `GetContainerForItemOverride()` → `CreateContainerForItemOverride()`
   - `IsItemItsOwnContainerOverride()` → `NeedsContainerOverride()`

5. **`ConnectionContainer.cs`** (ContentPresenter):
   - `System.Windows.Controls.ContentPresenter` → `Avalonia.Controls.Presenters.ContentPresenter`

6. **`ConnectionsMultiSelector.cs`** (MultiSelector):
   - `MultiSelector` → `SelectingItemsControl` or `ListBox`
   - Implement selection management using Avalonia's `Selection` model
   - `BeginUpdateSelectedItems()` / `EndUpdateSelectedItems()` → Avalonia batch update equivalent

7. **Container state files**: Update event handling for pointer events

---

### Phase 6: Connections
**Files:** `Connections/BaseConnection.cs`, `Connection.cs`, `LineConnection.cs`, `CircuitConnection.cs`, `StepConnection.cs`, `CuttingLine/CuttingLine.cs`, plus state files

1. **`BaseConnection.cs`** (~1037 lines, most complex file):
   - Keep as `Shape` subclass — Avalonia has `Avalonia.Controls.Shapes.Shape`
   - `DefiningGeometry` → `CreateDefiningGeometry()` (method instead of property)
   - `OnRender(DrawingContext)` → `public override void Render(DrawingContext context)`
   - Convert ~24 DPs to `StyledProperty<T>`
   - `AddOwner` pattern: In Avalonia, use `StyledProperty.AddOwner<T>()` where applicable, or re-register
   - `Stroke`, `StrokeThickness`, `Fill` → Inherited from Avalonia `Shape`
   - `VisualTreeHelper.GetDpi()` → Remove (not needed)
   - `FormattedText` → Use `FormattedText` in Avalonia or `TextLayout`
   - `FocusVisualAdorner` inner class → Rework using Avalonia `AdornerLayer` API or custom overlay
   - `ComponentResourceKey` (`FocusVisualPenKey`) → Use standard string resource key
   - `Geometry.Combine`, `GetWidenedPathGeometry` → Research Avalonia alternatives; may need to simplify focus visual rendering
   - Mouse/keyboard event handlers → Pointer/key handlers
   - Add pseudo-classes: `:selected`, `:pointerover`, `:cuttingline-over`

2. **`Connection.cs`** (Bezier): Update `DrawLineGeometry()` — `StreamGeometryContext` API should be very similar

3. **`LineConnection.cs`**, **`CircuitConnection.cs`**, **`StepConnection.cs`**: Same pattern — update drawing methods

4. **`CuttingLine.cs`**: Same Shape pattern; convert `OnRender` → `Render`

5. **Connection state files**: Update event handling

---

### Phase 7: Connectors
**Files:** `Connectors/Connector.cs`, `Connectors/PendingConnection.cs`, `Connectors/HotKeyControl.cs`, plus state files

1. **`Connector.cs`**:
   - `System.Windows.Controls.Control` → `Avalonia.Controls.Primitives.TemplatedControl`
   - Convert DPs and routed events
   - Mouse → Pointer events
   - `OnApplyTemplate` → `OnApplyTemplate` (exists in Avalonia)
   - `GetTemplateChild` → `this.FindNameScope()?.Find<T>(name)` or `e.NameScope.Find<T>(name)`
   - Add pseudo-classes: `:connected`, `:pendingconnection`

2. **`PendingConnection.cs`**:
   - Convert `ContentControl` base
   - **`HotKeyAdorner` inner class** → Rework:
     - Option A: Use Avalonia's `AdornerLayer.SetAdornedElement` API
     - Option B: Use a popup/overlay approach
     - Option C: Add adorner children via `AdornerLayer.Children`
   - `AllowOnlyConnectorsAttachedProperty` → `AttachedProperty<bool>`

3. **`HotKeyControl.cs`**: Simple control conversion

4. **Connector state files**: Update event handling

---

### Phase 8: Nodes
**Files:** `Nodes/Node.cs`, `NodeInput.cs`, `NodeOutput.cs`, `GroupingNode.cs`, `StateNode.cs`, `KnotNode.cs`, `Events/ResizeEventArgs.cs`

1. **`Node.cs`** (HeaderedContentControl):
   - `HeaderedContentControl` exists in Avalonia
   - Convert ~13 DPs
   - `OnApplyTemplate` / `GetTemplateChild` → Avalonia equivalents
   - `DataTemplateSelector` properties → Replace with `IDataTemplate` or `FuncDataTemplate`

2. **`NodeInput.cs` / `NodeOutput.cs`**: Extend converted `Connector`

3. **`GroupingNode.cs`**:
   - `Thumb` → `Avalonia.Controls.Primitives.Thumb`
   - `DragDelta` → `Thumb.DragDelta` exists in Avalonia
   - Resize event handling

4. **`StateNode.cs`**: Extends `Connector`

5. **`KnotNode.cs`**: Simple `ContentControl` conversion

---

### Phase 9: NodifyEditor (Main Control)
**Files:** All `Editor/NodifyEditor*.cs` partials (8 files), `EditorCommands.cs`, state files (7), utility files (4)

This is the most complex phase due to `MultiSelector` replacement.

1. **`NodifyEditor.cs` (main partial)**:
   - Change base class: `MultiSelector` → `SelectingItemsControl`
   - `GetContainerForItemOverride()` → `CreateContainerForItemOverride(object? item, int index, object? recycleKey)`
   - `IsItemItsOwnContainerOverride()` → `NeedsContainerOverride(object? item, int index, out object? recycleKey)`
   - Convert all ~29 DPs
   - `ViewportTransform` (TransformGroup) → Avalonia `TransformGroup`/`ScaleTransform`/`TranslateTransform` (same API)
   - `BitmapCache` optimization → Remove or replace with Avalonia rendering hints
   - `Mouse.MouseWheelDeltaForOneLine` → Use constant or get from event delta
   - `ContentProperty` / `DefaultProperty` attributes → Avalonia `[Content]` attribute
   - `StyleTypedProperty` / `TemplatePart` → Avalonia `[TemplatePart]` attribute
   - `SetCurrentValue` → `SetValue` (no `SetCurrentValue` in Avalonia; handle binding preservation differently)
   - Mouse event handlers → Pointer event handlers

2. **`NodifyEditor.Selecting.cs`**:
   - `base.SelectedItems` → Use Avalonia's `Selection` model or `SelectedItems` from `SelectingItemsControl`
   - `BeginUpdateSelectedItems()` / `EndUpdateSelectedItems()` → `BeginBatchUpdate()` / `EndBatchUpdate()` or equivalent
   - `ItemContainerGenerator.ContainerFromIndex()` → `ContainerFromIndex()` (method on ItemsControl in Avalonia 11)
   - `ItemContainerGenerator.ContainerFromItem()` → Find equivalent in Avalonia 11
   - `OnSelectionChanged(SelectionChangedEventArgs)` → Override exists in Avalonia

3. **`NodifyEditor.Dragging.cs`**: Convert pointer events, update drag state handling

4. **`NodifyEditor.Panning.cs`**:
   - `DispatcherTimer` → `Avalonia.Threading.DispatcherTimer`
   - `DispatcherPriority.Background` → `DispatcherPriority.Background` (exists)

5. **`NodifyEditor.Cutting.cs`**: Convert event handling

6. **`NodifyEditor.PushingItems.cs`**: Convert DPs and event handling

7. **`NodifyEditor.Scrolling.cs`**: Convert scrollbar logic

8. **`NodifyEditor.KeyboardNavigation.cs`**: Update keyboard focus API

9. **`EditorCommands.cs`**:
   - Remove `RoutedUICommand` entirely
   - Remove `CommandManager.RegisterClassCommandBinding`
   - Replace with simple `ICommand` implementations or handle directly in the editor
   - Move execute/canExecute logic into `NodifyEditor` methods
   - Key bindings → Register via Avalonia `KeyBinding` in the control template or control code

10. **Editor state files**: Update for Avalonia input events

11. **Editor utilities** (`AlignmentExtensions.cs`, `DraggingOptimized.cs`, `DraggingSimple.cs`, `PushItemsStrategy.cs`):
    - `RenderTransform` → `RenderTransform` (exists in Avalonia)
    - Type updates

---

### Phase 10: Minimap
**Files:** `Minimap/Minimap.cs`, `MinimapItem.cs`, `MinimapPanel.cs`, `SubtractConverter.cs`, plus state files

1. **`Minimap.cs`**: ItemsControl conversion, convert DPs
2. **`MinimapItem.cs`**: ContentControl conversion
3. **`MinimapPanel.cs`**: Panel with custom `MeasureOverride`/`ArrangeOverride`
4. **`SubtractConverter.cs`**: Update to Avalonia `IMultiValueConverter`
5. **Minimap state files**: Update event handling

---

### Phase 11: XAML → AXAML Theme Conversion
**Files:** All 22 XAML files in `Themes/` and `Themes/Styles/`

1. **Rename all `.xaml` → `.axaml`**

2. **Update XML namespaces** in every file:
   ```xml
   <!-- WPF -->
   <ResourceDictionary xmlns="http://schemas.microsoft.com/winfx/2006/xaml/presentation"
                       xmlns:x="http://schemas.microsoft.com/winfx/2006/xaml"
                       xmlns:local="clr-namespace:Nodify">

   <!-- Avalonia -->
   <ResourceDictionary xmlns="https://github.com/avaloniaui"
                       xmlns:x="http://schemas.microsoft.com/winfx/2006/xaml"
                       xmlns:local="clr-namespace:Nodify">
   ```

3. **Update URI scheme**: `pack://application:,,,/Nodify;component/` → `avares://Nodify/`

4. **Remove `o:Freeze="True"`** and `xmlns:o` declaration

5. **Convert Styles and ControlTemplates:**
   - WPF implicit style: `<Style TargetType="{x:Type local:Foo}">` → `<Style Selector="local|Foo">`
   - WPF `ControlTemplate TargetType` → `<ControlTemplate TargetType="local:Foo">`
   - `BasedOn="{StaticResource {x:Type local:Foo}}"` → Not needed (Avalonia styles cascade)
   - `TemplateBinding Property` → `{TemplateBinding PropertyName}` (same syntax)

6. **Convert Triggers → Styles with Pseudo-classes:**
   ```xml
   <!-- WPF Trigger -->
   <Trigger Property="IsMouseOver" Value="True">
       <Setter Property="Fill" Value="Red" />
   </Trigger>

   <!-- Avalonia: Add pseudo-class styles inside the Style -->
   <Style Selector="local|Connector:pointerover /template/ Ellipse#PART_Connector">
       <Setter Property="Fill" Value="Red" />
   </Style>
   ```

   Custom property triggers need pseudo-classes added in C# code:
   ```csharp
   PseudoClasses.Set(":selected", value);
   PseudoClasses.Set(":connected", value);
   PseudoClasses.Set(":over-element", value);
   ```

7. **Convert MultiTrigger / DataTrigger:**
   - `MultiTrigger` → Multiple nested style selectors or combined pseudo-classes
   - `DataTrigger` → Binding with converter or pseudo-class set from code

8. **Convert `BooleanToVisibilityConverter`** → Use `IsVisible` directly with binding

9. **Convert `MultiBinding`** → Avalonia `MultiBinding` (exists but syntax differs slightly)

10. **Remove `ComponentResourceKey`** references → Use standard string keys

11. **Update `Brushes.axaml`:**
    - Remove `o:Freeze`
    - `DynamicResource` / `StaticResource` → Same in Avalonia
    - `DrawingBrush` → Check Avalonia support (may need `VisualBrush` or `TileBrush` alternative)
    - `SolidColorBrush` → Same

12. **Update individual style files** (14 files in `Themes/Styles/`):
    - Each file: update xmlns, convert ControlTemplates, convert triggers to pseudo-class styles
    - `NodifyEditor.axaml` — most complex (6 MultiBindings, DataTrigger, selection rectangle)
    - `Connector.axaml`, `NodeInput.axaml`, `NodeOutput.axaml` — triggers for connected/hover states
    - `ItemContainer.axaml` — selection triggers
    - `GroupingNode.axaml` — Thumb, resize handling

13. **Create theme entry point** `NodifyTheme.axaml`:
    ```xml
    <Styles xmlns="https://github.com/avaloniaui" ...>
        <StyleInclude Source="avares://Nodify/Themes/Styles/Controls.axaml" />
    </Styles>
    ```

14. **Update `Controls.axaml`** (master merged dictionary):
    - `MergedDictionaries` → `ResourceInclude` or `StyleInclude`

15. **Update color themes** (`Dark.axaml`, `Light.axaml`, `Nodify.axaml`):
    - Preserve the 3-layer architecture (Colors → Brushes → Styles)
    - Convert to Avalonia ResourceDictionary format

---

### Phase 12: Build, Fix & Verify

1. **Iterative compilation:** Build after each phase and fix errors
2. **Verify all pseudo-classes** are properly set in C# code for AXAML styles
3. **Test theme switching** (Dark/Light/Nodify)
4. **Build a minimal test harness** (simple window with NodifyEditor) to verify:
   - Editor renders with items
   - Pan/zoom works
   - Item selection works
   - Connections render between nodes
   - Drag operations work
   - Minimap renders
   - Theme colors apply correctly

---

## Critical Files (in dependency order)

1. `Nodify.csproj` — Project conversion
2. `Properties/AssemblyInfo.cs` — Assembly metadata
3. `Utilities/BoxValue.cs` — Used by every control
4. `Utilities/MathExtensions.cs` — Math helpers
5. `Utilities/DependencyObjectExtensions.cs` → `VisualExtensions.cs` — Tree traversal + animations
6. `Interactivity/InputProcessor.cs` — Core input routing
7. `Interactivity/Gestures/EditorGestures.cs` — All gesture definitions
8. `Editor/NodifyCanvas.cs` — Layout panel
9. `Containers/ItemContainer.cs` — Item wrapper
10. `Connections/BaseConnection.cs` — Rendering pipeline
11. `Editor/NodifyEditor.cs` + partials — Main control
12. `Editor/EditorCommands.cs` — Command system replacement
13. All `Themes/Styles/*.axaml` — Visual templates

---

## Verification Plan

1. **After Phase 1**: `dotnet build` should resolve all Avalonia packages
2. **After Phase 11**: Full build with no errors
