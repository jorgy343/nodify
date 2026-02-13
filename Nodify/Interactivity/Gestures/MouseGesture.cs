using Avalonia.Input;
using Avalonia.Interactivity;

namespace Nodify.Interactivity
{
    /// <summary>
    /// Specifies the mouse actions that can trigger a gesture.
    /// </summary>
    public enum MouseAction
    {
        None,
        LeftClick,
        RightClick,
        MiddleClick,
        LeftDoubleClick,
        RightDoubleClick,
        MiddleDoubleClick,
        WheelClick
    }

    /// <summary>
    /// Represents a mouse gesture that optionally includes modifier keys and a specific key press.
    /// </summary>
    public sealed class MouseGesture : InputGesture
    {
        /// <summary>Gets or sets the mouse action associated with this gesture.</summary>
        public MouseAction Action { get; set; }

        /// <summary>Gets or sets the modifier keys required for this gesture.</summary>
        public KeyModifiers Modifiers { get; set; }

        /// <summary>Gets or sets the key that must be pressed to match this gesture.</summary>
        public Key Key { get; set; }

        /// <summary>Whether to ignore modifier keys when releasing the mouse button.</summary>
        public bool IgnoreModifierKeysOnRelease { get; set; }

        public MouseGesture(MouseAction action, KeyModifiers modifiers, Key key) : this(action, modifiers)
        {
            Key = key;
        }

        public MouseGesture(MouseAction action, Key key) : this(action)
        {
            Key = key;
        }

        public MouseGesture(MouseAction action, KeyModifiers modifiers)
        {
            Action = action;
            Modifiers = modifiers;
        }

        public MouseGesture(MouseAction action, KeyModifiers modifiers, bool ignoreModifierKeysOnRelease)
            : this(action, modifiers)
        {
            IgnoreModifierKeysOnRelease = ignoreModifierKeysOnRelease;
        }

        public MouseGesture(MouseAction action)
        {
            Action = action;
        }

        public MouseGesture()
        {
        }

        /// <inheritdoc />
        public override bool Matches(object? targetElement, RoutedEventArgs inputEventArgs)
        {
            if (inputEventArgs is PointerPressedEventArgs pressed)
            {
                var props = pressed.GetCurrentPoint(null).Properties;
                if (!MatchesButtonPressed(props, pressed.ClickCount))
                    return false;
                return pressed.KeyModifiers == Modifiers;
            }

            if (inputEventArgs is PointerReleasedEventArgs released)
            {
                if (!MatchesButtonReleased(released.InitialPressMouseButton))
                    return false;
                if (IgnoreModifierKeysOnRelease)
                    return released.KeyModifiers == Modifiers || released.KeyModifiers == KeyModifiers.None;
                return released.KeyModifiers == Modifiers;
            }

            if (inputEventArgs is PointerWheelChangedEventArgs wheel)
            {
                if (Action != MouseAction.WheelClick)
                    return false;
                return wheel.KeyModifiers == Modifiers;
            }

            return false;
        }

        private bool MatchesButtonPressed(PointerPointProperties props, int clickCount)
        {
            return Action switch
            {
                MouseAction.LeftClick => props.PointerUpdateKind == PointerUpdateKind.LeftButtonPressed,
                MouseAction.RightClick => props.PointerUpdateKind == PointerUpdateKind.RightButtonPressed,
                MouseAction.MiddleClick => props.PointerUpdateKind == PointerUpdateKind.MiddleButtonPressed,
                MouseAction.LeftDoubleClick => props.PointerUpdateKind == PointerUpdateKind.LeftButtonPressed && clickCount >= 2,
                MouseAction.RightDoubleClick => props.PointerUpdateKind == PointerUpdateKind.RightButtonPressed && clickCount >= 2,
                MouseAction.MiddleDoubleClick => props.PointerUpdateKind == PointerUpdateKind.MiddleButtonPressed && clickCount >= 2,
                MouseAction.None => true,
                _ => false
            };
        }

        private bool MatchesButtonReleased(MouseButton button)
        {
            return Action switch
            {
                MouseAction.LeftClick or MouseAction.LeftDoubleClick => button == MouseButton.Left,
                MouseAction.RightClick or MouseAction.RightDoubleClick => button == MouseButton.Right,
                MouseAction.MiddleClick or MouseAction.MiddleDoubleClick => button == MouseButton.Middle,
                MouseAction.None => true,
                _ => false
            };
        }
    }
}
