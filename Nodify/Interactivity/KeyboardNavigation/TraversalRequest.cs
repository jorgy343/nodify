using Avalonia.Input;

namespace Nodify.Interactivity
{
    /// <summary>
    /// Represents a request to move keyboard focus in a specific direction.
    /// </summary>
    public class TraversalRequest
    {
        /// <summary>Gets or sets the direction of focus movement.</summary>
        public NavigationDirection FocusNavigationDirection { get; set; }

        /// <summary>Gets or sets a value indicating whether focus wrapped around.</summary>
        public bool Wrapped { get; set; }

        /// <summary>
        /// Initializes a new instance of <see cref="TraversalRequest"/> with the specified direction.
        /// </summary>
        public TraversalRequest(NavigationDirection direction)
        {
            FocusNavigationDirection = direction;
        }
    }
}
