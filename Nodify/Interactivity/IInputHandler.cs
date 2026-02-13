using Avalonia.Interactivity;

namespace Nodify.Interactivity
{
    /// <summary>
    /// Defines a contract for handling input events within an element or system.
    /// </summary>
    public interface IInputHandler
    {
        /// <summary>
        /// Handles a given input event, such as a pointer or keyboard interaction.
        /// </summary>
        /// <param name="e">The <see cref="RoutedEventArgs"/> representing the input event.</param>
        void HandleEvent(RoutedEventArgs e);

        /// <summary>
        /// Gets a value indicating whether the handler requires input capture to remain active.
        /// </summary>
        bool RequiresInputCapture { get; }

        /// <summary>
        /// Gets or sets a value indicating whether events that have been handled should be processed too.
        /// </summary>
        bool ProcessHandledEvents { get; }
    }
}
