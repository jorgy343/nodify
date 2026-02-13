using Avalonia.Input;
using Avalonia.Interactivity;

namespace Nodify.Interactivity;

/// <summary>
/// Represents a pointer gesture that can be used to trigger input interactions.
/// </summary>
public class PointerGesture
{
    /// <summary>
    /// Gets the mouse button associated with this gesture.
    /// </summary>
    public MouseButton MouseButton { get; }

    /// <summary>
    /// Gets the modifier keys required for this gesture.
    /// </summary>
    public KeyModifiers Modifiers { get; }

    /// <summary>
    /// Gets a value indicating whether modifier keys should be ignored when releasing the pointer button.
    /// </summary>
    public bool IgnoreModifiersOnRelease { get; }

    /// <summary>
    /// Initializes a new instance of the <see cref="PointerGesture"/> class.
    /// </summary>
    /// <param name="button">The mouse button for this gesture.</param>
    /// <param name="modifiers">The modifier keys for this gesture.</param>
    /// <param name="ignoreModifiersOnRelease">Whether to ignore modifiers on release.</param>
    public PointerGesture(MouseButton button, KeyModifiers modifiers = KeyModifiers.None, bool ignoreModifiersOnRelease = false)
    {
        MouseButton = button;
        Modifiers = modifiers;
        IgnoreModifiersOnRelease = ignoreModifiersOnRelease;
    }

    /// <summary>
    /// Determines whether the specified event matches this gesture.
    /// </summary>
    /// <param name="source">The source of the event.</param>
    /// <param name="e">The event arguments.</param>
    /// <returns>True if the event matches this gesture; otherwise, false.</returns>
    public virtual bool Matches(object? source, RoutedEventArgs e)
    {
        if (e is PointerPressedEventArgs pressedArgs)
        {
            var point = pressedArgs.GetCurrentPoint(null);
            var currentButton = GetPressedButton(point.Properties);
            var currentModifiers = pressedArgs.KeyModifiers;

            return currentButton == MouseButton &&
                   (Modifiers == KeyModifiers.None || currentModifiers.HasFlag(Modifiers));
        }

        if (e is PointerReleasedEventArgs releasedArgs)
        {
            var point = releasedArgs.GetCurrentPoint(null);
            var releasedButton = GetReleasedButton(point.Properties);
            var currentModifiers = releasedArgs.KeyModifiers;

            if (IgnoreModifiersOnRelease)
            {
                return releasedButton == MouseButton;
            }

            return releasedButton == MouseButton &&
                   (Modifiers == KeyModifiers.None || currentModifiers.HasFlag(Modifiers));
        }

        return false;
    }

    private MouseButton GetPressedButton(PointerPointProperties props)
    {
        if (props.IsLeftButtonPressed) return MouseButton.Left;
        if (props.IsRightButtonPressed) return MouseButton.Right;
        if (props.IsMiddleButtonPressed) return MouseButton.Middle;
        if (props.IsXButton1Pressed) return MouseButton.XButton1;
        if (props.IsXButton2Pressed) return MouseButton.XButton2;
        return MouseButton.None;
    }

    private MouseButton GetReleasedButton(PointerPointProperties props)
    {
        // Determine which button was released by checking the PointerUpdateKind
        return props.PointerUpdateKind switch
        {
            PointerUpdateKind.LeftButtonReleased => MouseButton.Left,
            PointerUpdateKind.RightButtonReleased => MouseButton.Right,
            PointerUpdateKind.MiddleButtonReleased => MouseButton.Middle,
            PointerUpdateKind.XButton1Released => MouseButton.XButton1,
            PointerUpdateKind.XButton2Released => MouseButton.XButton2,
            _ => MouseButton.None
        };
    }
}
