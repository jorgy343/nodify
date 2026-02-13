using Avalonia.Input;
using Avalonia.Interactivity;

namespace Nodify.Interactivity
{
    /// <summary>
    /// Represents a keyboard gesture that matches key press events.
    /// </summary>
    public class KeyGesture : InputGesture
    {
        /// <summary>Gets the key associated with this gesture.</summary>
        public Key Key { get; }

        /// <summary>Gets the modifier keys associated with this gesture.</summary>
        public KeyModifiers Modifiers { get; }

        /// <summary>
        /// Initializes a new instance of <see cref="KeyGesture"/> with the specified key and optional modifiers.
        /// </summary>
        public KeyGesture(Key key, KeyModifiers modifiers = KeyModifiers.None)
        {
            Key = key;
            Modifiers = modifiers;
        }

        /// <inheritdoc />
        public override bool Matches(object? targetElement, RoutedEventArgs inputEventArgs)
        {
            if (inputEventArgs is KeyEventArgs ke)
            {
                return ke.Key == Key && ke.KeyModifiers == Modifiers;
            }
            return false;
        }
    }
}
