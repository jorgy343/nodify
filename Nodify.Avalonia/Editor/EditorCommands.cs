using System;
using System.Windows.Input;

namespace Nodify;

/// <summary>
/// Provides common commands for the <see cref="NodifyEditor"/>.
/// </summary>
public static class EditorCommands
{
    /// <summary>
    /// Zoom in relative to the editor's viewport center.
    /// </summary>
    public static ICommand ZoomIn { get; } = new SimpleCommand(
        nameof(ZoomIn),
        "Zoom in");

    /// <summary>
    /// Zoom out relative to the editor's viewport center.
    /// </summary>
    public static ICommand ZoomOut { get; } = new SimpleCommand(
        nameof(ZoomOut),
        "Zoom out");

    /// <summary>
    /// Select all <see cref="ItemContainer"/>s in the <see cref="NodifyEditor"/>.
    /// </summary>
    public static ICommand SelectAll { get; } = new SimpleCommand(
        nameof(SelectAll),
        "Select All");

    /// <summary>
    /// Moves the <see cref="NodifyEditor"/> viewport to the specified location.
    /// Parameter is a <see cref="Avalonia.Point"/> or a string that can be converted to a point.
    /// </summary>
    public static ICommand BringIntoView { get; } = new SimpleCommand(
        nameof(BringIntoView),
        "Bring location into view");

    /// <summary>
    /// Scales the editor's viewport to fit all the <see cref="ItemContainer"/>s if that's possible.
    /// </summary>
    public static ICommand FitToScreen { get; } = new SimpleCommand(
        nameof(FitToScreen),
        "Fit to screen");

    /// <summary>
    /// Aligns the selected containers using the specified alignment method.
    /// Parameter is of type <see cref="Alignment"/> or a string that can be converted to an alignment.
    /// </summary>
    public static ICommand Align { get; } = new SimpleCommand(
        nameof(Align),
        "Align");

    /// <summary>
    /// Locks the position of the selected containers.
    /// </summary>
    public static ICommand LockSelection { get; } = new SimpleCommand(
        nameof(LockSelection),
        "Lock selection");

    /// <summary>
    /// Unlocks the position of the selected containers.
    /// </summary>
    public static ICommand UnlockSelection { get; } = new SimpleCommand(
        nameof(UnlockSelection),
        "Unlock selection");

    /// <summary>
    /// Simple command implementation for editor commands.
    /// </summary>
    private class SimpleCommand : ICommand
    {
        private readonly string _name;
        private readonly string _text;

        public SimpleCommand(string name, string text)
        {
            _name = name;
            _text = text;
        }

        public event EventHandler? CanExecuteChanged
        {
            add { }
            remove { }
        }

        public bool CanExecute(object? parameter) => true;

        public void Execute(object? parameter)
        {
            // Commands are executed through the editor's command handlers
            // This is a placeholder implementation
        }

        public override string ToString() => $"{_name}: {_text}";
    }
}
