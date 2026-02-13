using Avalonia.Interactivity;

namespace Nodify.Interactivity
{
    /// <summary>
    /// Abstract base class for input gestures used in the Nodify interactivity framework.
    /// </summary>
    public abstract class InputGesture
    {
        /// <summary>
        /// Determines whether this gesture matches the given routed event.
        /// </summary>
        /// <param name="targetElement">The target element of the event.</param>
        /// <param name="inputEventArgs">The routed event arguments.</param>
        /// <returns>True if the gesture matches; otherwise, false.</returns>
        public abstract bool Matches(object? targetElement, RoutedEventArgs inputEventArgs);
    }
}
