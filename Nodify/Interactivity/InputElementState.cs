using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;

namespace Nodify.Interactivity
{
    /// <summary>
    /// Represents a base class for handling input events in a specific state for a control.
    /// </summary>
    /// <typeparam name="TElement">The type of the control that owns this state.</typeparam>
    public abstract class InputElementState<TElement> : IInputHandler
        where TElement : Control
    {
        /// <summary>
        /// Gets the owner of the state.
        /// </summary>
        protected TElement Element { get; }

        public bool RequiresInputCapture { get; protected set; }
        public bool ProcessHandledEvents { get; protected set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="InputElementState{TElement}"/> class.
        /// </summary>
        protected InputElementState(TElement element)
        {
            Element = element;
        }

        /// <summary>Called when a pointer button is pressed.</summary>
        protected virtual void OnPointerPressed(PointerPressedEventArgs e) { }

        /// <summary>Called when a pointer button is released.</summary>
        protected virtual void OnPointerReleased(PointerReleasedEventArgs e) { }

        /// <summary>Called when the pointer moves.</summary>
        protected virtual void OnPointerMoved(PointerEventArgs e) { }

        /// <summary>Called when the mouse wheel changes.</summary>
        protected virtual void OnPointerWheelChanged(PointerWheelChangedEventArgs e) { }

        /// <summary>Called when a key is released.</summary>
        protected virtual void OnKeyUp(KeyEventArgs e) { }

        /// <summary>Called when a key is pressed.</summary>
        protected virtual void OnKeyDown(KeyEventArgs e) { }

        /// <summary>Called when pointer capture is lost.</summary>
        protected virtual void OnPointerCaptureLost(PointerCaptureLostEventArgs e) { }

        /// <summary>
        /// Called for any input event that is not explicitly handled by other methods.
        /// </summary>
        protected virtual void OnEvent(RoutedEventArgs e) { }

        /// <summary>
        /// Processes the input event by invoking the appropriate handler method based on the routed event.
        /// </summary>
        public void HandleEvent(RoutedEventArgs e)
        {
            if (e.RoutedEvent == InputElement.PointerMovedEvent)
            {
                OnPointerMoved((PointerEventArgs)e);
            }
            else if (e.RoutedEvent == InputElement.PointerPressedEvent)
            {
                OnPointerPressed((PointerPressedEventArgs)e);
            }
            else if (e.RoutedEvent == InputElement.PointerReleasedEvent)
            {
                OnPointerReleased((PointerReleasedEventArgs)e);
            }
            else if (e.RoutedEvent == InputElement.PointerWheelChangedEvent)
            {
                OnPointerWheelChanged((PointerWheelChangedEventArgs)e);
            }
            else if (e.RoutedEvent == InputElement.PointerCaptureLostEvent)
            {
                OnPointerCaptureLost((PointerCaptureLostEventArgs)e);
            }
            else if (e.RoutedEvent == InputElement.KeyDownEvent)
            {
                OnKeyDown((KeyEventArgs)e);
            }
            else if (e.RoutedEvent == InputElement.KeyUpEvent)
            {
                OnKeyUp((KeyEventArgs)e);
            }

            OnEvent(e);
        }
    }
}
