using Avalonia.Interactivity;

namespace Nodify.Interactivity;

/// <summary>
/// A gesture that allows changing its logic at runtime without changing its reference.
/// Useful for classes that capture the object reference without the possibility of updating it. (e.g. <see cref="EditorCommands"/>)
/// </summary>
public sealed class GestureRef : Gesture
{
    /// <summary>The referenced gesture.</summary>
    public Gesture Value { get; set; } = MultiGesture.None;

    private GestureRef() { }

    internal GestureRef(Gesture gesture)
    {
        Value = gesture;
    }

    public override bool Matches(object? targetElement, RoutedEventArgs inputEventArgs)
    {
        return Value.Matches(targetElement, inputEventArgs);
    }

    public static implicit operator GestureRef(PointerGesture gesture)
        => new GestureRef { Value = gesture };

    public static implicit operator GestureRef(KeyGesture gesture)
        => new GestureRef { Value = gesture };

    public static implicit operator GestureRef(MultiGesture gesture)
        => new GestureRef { Value = gesture };

    /// <summary>
    /// Unbinds the current gesture.
    /// </summary>
    public void Unbind()
        => Value = MultiGesture.None;
}

/// <summary>
/// Extension methods for the <see cref="GestureRef"/> class.
/// </summary>
public static class GestureRefExtensions
{
    /// <summary>
    /// Creates a new <see cref="GestureRef"/> from the specified gesture.
    /// </summary>
    public static GestureRef AsRef(this Gesture gesture)
    {
        return new GestureRef(gesture);
    }
}
