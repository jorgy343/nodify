# Nodify.Avalonia

A modernized port of the Nodify node-based editor library for Avalonia UI 11.3.12.

## Overview

Nodify.Avalonia is a complete rewrite of the WPF-based Nodify library, targeting Avalonia UI framework with .NET 10 and C# 14. This port provides a cross-platform node-based editor with a modernized API that is not backwards compatible with the WPF version.

## Features

- **Cross-platform**: Runs on Windows, macOS, and Linux
- **Modern .NET**: Built for .NET 10 with C# 14 features
- **Avalonia 11.3.12**: Uses the latest stable Avalonia UI framework
- **Modernized API**: Redesigned API taking advantage of newer C# language features
- **Same Functionality**: Maintains the core functionality of the WPF version

## Architecture Changes

### WPF to Avalonia Conversions

- **DependencyProperty → StyledProperty/DirectProperty**: All WPF dependency properties have been converted to Avalonia's property system
- **RoutedEvent → RoutedEvent**: Routed events adapted to Avalonia's event system
- **Visual Tree Helpers**: Custom visual tree helper extensions for Avalonia
- **Styling**: XAML converted to AXAML with Avalonia-specific styling

### Key Differences from WPF Version

1. **Nullable Reference Types**: Full nullable annotations throughout the codebase
2. **Modern C# Patterns**: Use of pattern matching, records where appropriate, and other C# 14 features
3. **Avalonia Controls**: Built on Avalonia's control hierarchy instead of WPF
4. **No Backwards Compatibility**: This is a clean port, not a compatibility layer

## Project Structure

```
Nodify.Avalonia/
├── Connections/       - Connection line controls
├── Connectors/        - Connector controls for nodes
├── Containers/        - Container controls (ItemContainer, etc.)
├── Editor/            - Main NodifyEditor control
├── Interactivity/     - Input handling and gestures
├── Minimap/           - Minimap control
├── Nodes/             - Node controls
├── Themes/            - Avalonia themes and styles
└── Utilities/         - Helper classes and extensions
```

## Getting Started

### Installation

```bash
dotnet add package Nodify.Avalonia
```

### Basic Usage

```csharp
// Example usage will be added as more controls are ported
```

## Building from Source

```bash
dotnet build Nodify.Avalonia/Nodify.Avalonia.csproj
```

## Contributing

This is an active port of the WPF version. Contributions are welcome!

## License

MIT License - Same as the original Nodify project

## Credits

- Original Nodify WPF library by Miroiu Emanuel
- Avalonia port development

## Status

This project is currently in active development. Core utilities and base classes have been implemented. Additional controls are being ported incrementally.

### Completed
- ✅ Project structure and build system
- ✅ Core utility classes (BoxValue, MathExtensions, SelectionHelper, etc.)
- ✅ Visual tree extension methods
- ✅ Value converters
- ✅ Basic Node control
- ✅ Editor controls (NodifyEditor, NodifyCanvas, EditorCommands)

### In Progress
- 🚧 Connection controls
- 🚧 Connector controls
- 🚧 Interactivity system
- 🚧 Themes and styles

### Planned
- ⏳ Complete control library
- ⏳ Sample applications
- ⏳ Documentation
- ⏳ Unit tests
