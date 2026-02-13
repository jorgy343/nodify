using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;

namespace Nodify.Interactivity;

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
    /// <param name="element">The control that owns this state.</param>
    protected InputElementState(TElement element)
    {
        Element = element;
    }

    /// <inheritdoc cref="InputElement.OnPointerPressed(PointerPressedEventArgs)"/>
    protected virtual void OnPointerPressed(PointerPressedEventArgs e) { }

    /// <inheritdoc cref="InputElement.OnPointerReleased(PointerReleasedEventArgs)"/>
    protected virtual void OnPointerReleased(PointerReleasedEventArgs e) { }

    /// <inheritdoc cref="InputElement.OnPointerMoved(PointerEventArgs)"/>
    protected virtual void OnPointerMoved(PointerEventArgs e) { }

    /// <inheritdoc cref="InputElement.OnPointerWheelChanged(PointerWheelEventArgs)"/>
    protected virtual void OnPointerWheelChanged(PointerWheelEventArgs e) { }

    /// <inheritdoc cref="InputElement.OnKeyUp(KeyEventArgs)"/>
    protected virtual void OnKeyUp(KeyEventArgs e) { }

    /// <inheritdoc cref="InputElement.OnKeyDown(KeyEventArgs)"/>
    protected virtual void OnKeyDown(KeyEventArgs e) { }

    /// <inheritdoc cref="InputElement.OnPointerCaptureLost(PointerCaptureLostEventArgs)"/>
    protected virtual void OnPointerCaptureLost(PointerCaptureLostEventArgs e) { }

    /// <summary>
    /// Called for any input event that is not explicitly handled by other methods.
    /// </summary>
    /// <param name="e">The input event arguments.</param>
    protected virtual void OnEvent(RoutedEventArgs e) { }

    /// <summary>
    /// Processes the input event by invoking the appropriate handler method based on the routed event.
    /// </summary>
    /// <param name="e">The input event arguments.</param>
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
            OnPointerWheelChanged((PointerWheelEventArgs)e);
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
