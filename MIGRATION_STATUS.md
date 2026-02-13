# WPF to Avalonia 11.3 Migration Status

## Completed Work

### ✅ Phase 1: Project Infrastructure (Complete)
- Converted `Nodify.csproj` to `Microsoft.NET.Sdk` targeting `net10.0`
- Added Avalonia 11.3.11 NuGet packages
- Updated `AssemblyInfo.cs` to use Avalonia.Metadata
- Created `NodifyTheme.cs` for Avalonia style loading
- Set `LangVersion` to 14

### ✅ Phase 2: XAML File Renaming (Complete)
- Renamed all 22 `.xaml` files to `.axaml`
- Created `NodifyTheme.axaml` matching the C# class

### ✅ Phase 3: XAML Namespace Updates (Complete)
- Updated all xmlns to `https://github.com/avaloniaui`
- Converted `pack://` URIs to `avares://` URIs
- Removed WPF-specific `o:Freeze` attributes
- Updated all `.xaml` references to `.axaml`

### ✅ Phase 4: C# Namespace Imports (Complete)
- Replaced `System.Windows.*` with `Avalonia.*` across 93 C# files
- Updated: Controls, Input, Media, Shapes, Threading, Documents namespaces

### 🔄 Phase 5: API-Level Conversions (In Progress)
- ✅ Converted `DependencyObjectExtensions` → `VisualExtensions`
- ✅ Replaced `VisualTreeHelper` with Avalonia visual tree extensions
- ⏳ ~1662 compilation errors remain

## Remaining Work (Extensive)

### Critical API Conversions Needed

#### 1. DependencyProperty → AvaloniaProperty (141+ registrations)
All `DependencyProperty.Register` calls need conversion to:
```csharp
// WPF
public static readonly DependencyProperty MyProperty =
    DependencyProperty.Register(nameof(MyProp), typeof(string), typeof(MyControl));

// Avalonia
public static readonly StyledProperty<string> MyProperty =
    AvaloniaProperty.Register<MyControl, string>(nameof(MyProp));
```

#### 2. DependencyProperty Metadata
- `FrameworkPropertyMetadata(AffectsRender)` → Call `AffectsRender<T>(property)` in static constructor
- `FrameworkPropertyMetadata(BindsTwoWayByDefault)` → Pass `defaultBindingMode: BindingMode.TwoWay`
- `CoerceValueCallback` → Use `.AddCoerce()` or validate in property changed handler

#### 3. Read-Only Properties
```csharp
// WPF
DependencyProperty.RegisterReadOnly(...)

// Avalonia
AvaloniaProperty.RegisterDirect<TOwner, TValue>(
    nameof(Prop),
    o => o._field,
    (o, v) => o._field = v)
```

#### 4. Mouse Events → Pointer Events
All mouse event handlers need conversion:
- `OnMouseDown` → `OnPointerPressed`
- `OnMouseUp` → `OnPointerReleased`
- `OnMouseMove` → `OnPointerMoved`
- `OnMouseWheel` → `OnPointerWheelChanged`
- `MouseButtonEventArgs` → `PointerPressedEventArgs/PointerReleasedEventArgs`
- `Mouse.Capture(control)` → `e.Pointer.Capture(control)`
- `IsMouseOver` → `IsPointerOver`

#### 5. Type Replacements
- `DependencyObject` → `AvaloniaObject` or `StyledElement`
- `UIElement` → `Control` or `Visual`
- `FrameworkElement` → `Control`
- `RoutedEventArgs` → `RoutedEventArgs` (exists in Avalonia.Interactivity)
- `ICommand` → `System.Windows.Input.ICommand` (add using)

#### 6. Control Base Classes
- `MultiSelector` → Custom selection with `SelectingItemsControl`
- `HeaderedContentControl` → `HeaderedContentControl` (exists in Avalonia)
- `ContentControl` → `ContentControl` (exists in Avalonia)

#### 7. WPF-Specific Features to Remove/Replace
- `TemplatePartAttribute` → Different approach in Avalonia
- `StyleTypedPropertyAttribute` → Not applicable
- `AdornerLayer` → Use overlays or different approach
- `DefaultStyleKeyProperty.OverrideMetadata` → Not needed in Avalonia
- Animations (`BeginAnimation`, `PointAnimation`, etc.) → Avalonia.Animation API

#### 8. Rendering
- `OnRender(DrawingContext)` → `public override void Render(DrawingContext)`
- `Shape.DefiningGeometry` → `protected abstract Geometry CreateDefiningGeometry()`

#### 9. Routed Events
```csharp
// WPF
public static readonly RoutedEvent MyEvent =
    EventManager.RegisterRoutedEvent(nameof(MyEvent), RoutingStrategy.Bubble, ...);

// Avalonia
public static readonly RoutedEvent<RoutedEventArgs> MyEvent =
    RoutedEvent.Register<MyControl, RoutedEventArgs>(nameof(MyEvent), RoutingStrategies.Bubble);
```

#### 10. Visual Tree Operations
Already started in `VisualExtensions.cs`, but need to update all usage sites.

## Files Requiring Major Changes

### High Priority (Core Infrastructure)
1. `Containers/ItemContainer.cs` - 18 DependencyProperty registrations
2. `Editor/NodifyEditor.cs` - Main editor control, many properties
3. `Connections/BaseConnection.cs` - Base connection class
4. `Connectors/Connector.cs` - Connector control
5. `Nodes/Node.cs` - Node control base class

### Medium Priority
6. `Minimap/Minimap.cs`
7. `Nodes/GroupingNode.cs`
8. `Connectors/PendingConnection.cs`
9. `Containers/DecoratorContainer.cs`

### All Controls Need
- DependencyProperty → AvaloniaProperty conversions
- Mouse → Pointer event conversions
- Static constructor updates for property metadata
- Removal of WPF-specific attributes

## Build Status
- Current errors: ~1662
- Most errors are from:
  - Unresolved type names (DependencyProperty, RoutedEvent, UIElement, etc.)
  - Missing method signatures (Mouse events)
  - WPF-specific attributes

## Recommended Approach

### Option 1: Systematic File-by-File
1. Start with `BoxValue.cs` and utility classes (minimal dependencies)
2. Move to base classes like `BaseConnection`
3. Then concrete controls
4. Finally, complex controls like `NodifyEditor`

### Option 2: Feature-by-Feature
1. Convert all DependencyProperty registrations first (all files)
2. Convert all event handlers (all files)
3. Remove WPF attributes (all files)
4. Fix remaining type issues

### Option 3: Minimal Viable
1. Focus on just 1-2 core controls to make fully functional
2. Leave others non-functional temporarily
3. Provides proof of concept

## Notes
- Examples directory is out of scope per original plan
- Animation system needs full custom implementation
- Some WPF features (like Adorners) may need architectural changes
- Testing will be required after compilation succeeds

## Time Estimate
Based on complexity, completing this migration properly would require:
- 20-40 hours for systematic conversion
- 10-20 hours for testing and refinement
- Total: 30-60 hours of focused work
