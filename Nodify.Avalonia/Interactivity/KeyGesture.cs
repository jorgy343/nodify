using Avalonia.Input;
using Avalonia.Interactivity;

namespace Nodify.Interactivity;

/// <summary>
/// Represents a keyboard gesture that can be used to trigger input interactions.
/// </summary>
public class KeyGesture : Gesture
{
    /// <summary>
    /// Gets the key associated with this gesture.
    /// </summary>
    public Key Key { get; }

    /// <summary>
    /// Gets the modifier keys required for this gesture.
    /// </summary>
    public KeyModifiers Modifiers { get; }

    /// <summary>
    /// Initializes a new instance of the <see cref="KeyGesture"/> class.
    /// </summary>
    /// <param name="key">The key for this gesture.</param>
    /// <param name="modifiers">The modifier keys for this gesture.</param>
    public KeyGesture(Key key, KeyModifiers modifiers = KeyModifiers.None)
    {
        Key = key;
        Modifiers = modifiers;
    }

    /// <summary>
    /// Determines whether the specified event matches this gesture.
    /// </summary>
    /// <param name="source">The source of the event.</param>
    /// <param name="e">The event arguments.</param>
    /// <returns>True if the event matches this gesture; otherwise, false.</returns>
    public override bool Matches(object? source, RoutedEventArgs e)
    {
        if (e is KeyEventArgs keyArgs)
        {
            return keyArgs.Key == Key &&
                   (Modifiers == KeyModifiers.None || keyArgs.KeyModifiers.HasFlag(Modifiers));
        }

        return false;
    }
}
