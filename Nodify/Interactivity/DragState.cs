using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;

namespace Nodify.Interactivity
{
    /// <summary>
    /// Represents an abstract base class for managing drag interactions within a control.
    /// </summary>
    /// <typeparam name="TElement">The type of <see cref="Control"/> that owns the state.</typeparam>
    public abstract class DragState<TElement> : InputElementState<TElement>, IInputHandler
        where TElement : Control
    {
        private enum InteractionState
        {
            Ready,
            InProgress,
            Ending
        }

        /// <summary>Gets the gesture used to cancel the drag interaction, if defined.</summary>
        protected InputGesture? CancelGesture { get; }

        /// <summary>Gets the gesture used to begin the drag interaction.</summary>
        protected InputGesture BeginGesture { get; }

        /// <summary>Indicates whether the element has a context menu associated with it.</summary>
        protected virtual bool HasContextMenu => Element.ContextMenu != null;

        /// <summary>Determines if the drag interaction can begin.</summary>
        protected virtual bool CanBegin { get; } = true;

        /// <summary>Determines if the drag interaction can be canceled.</summary>
        protected virtual bool CanCancel { get; } = true;

        /// <summary>Indicates if the drag gesture is a toggle.</summary>
        protected virtual bool IsToggle { get; }

        /// <summary>Gets or sets the control used to calculate the pointer position during the drag interaction.</summary>
        protected Control PositionElement { get; set; }

        private InteractionState _interactionState;
        private Point _initialPosition;

        public DragState(TElement element, InputGesture beginGesture) : base(element)
        {
            BeginGesture = beginGesture;
            PositionElement = element;
        }

        public DragState(TElement element, InputGesture beginGesture, InputGesture cancelGesture)
            : this(element, beginGesture)
        {
            CancelGesture = cancelGesture;
        }

        void IInputHandler.HandleEvent(RoutedEventArgs e)
        {
            if (_interactionState == InteractionState.Ready && TryBeginDragging(e))
            {
                return;
            }

            if (_interactionState == InteractionState.Ending && TryEndDragging(e))
            {
                return;
            }

            if (_interactionState == InteractionState.InProgress)
            {
                if (TryEndDragging(e) || TryCancelDragging(e) || TrySuppressContextMenu(e))
                {
                    return;
                }
            }

            TryHandleEvent(e);
        }

        #region Interaction logic

        private bool TryBeginDragging(RoutedEventArgs e)
        {
            if (IsInputEventPressed(e) && CanBegin && BeginGesture.Matches(e.Source, e))
            {
                BeginDrag(e);
                return true;
            }

            return false;
        }

        private bool TryEndDragging(RoutedEventArgs e)
        {
            if (IsInputCaptureLost(e))
            {
                EndDrag(e);
                return true;
            }

            if (IsToggle && _interactionState == InteractionState.InProgress)
            {
                return TryDeferToggleInteractionEnd(e);
            }

            return TryEndInteraction(e);
        }

        private bool TryDeferToggleInteractionEnd(RoutedEventArgs e)
        {
            if (IsInputEventPressed(e) && BeginGesture.Matches(e.Source, e))
            {
                _interactionState = InteractionState.Ending;
                HandleEvent(e);
                return true;
            }

            return false;
        }

        private bool TryEndInteraction(RoutedEventArgs e)
        {
            if (IsInputEventReleased(e) && BeginGesture.Matches(e.Source, e))
            {
                EndDrag(e);
                return true;
            }

            return false;
        }

        private bool TryCancelDragging(RoutedEventArgs e)
        {
            if (CanCancel && IsInputEventReleased(e) && CancelGesture?.Matches(e.Source, e) is true)
            {
                CancelDrag(e);
                return true;
            }

            return false;
        }

        private bool TrySuppressContextMenu(RoutedEventArgs e)
        {
            if (IsToggle && e is PointerPressedEventArgs ppe
                && ppe.GetCurrentPoint(null).Properties.IsRightButtonPressed)
            {
                e.Handled = true;
                HandleEvent(e);
                return true;
            }

            return false;
        }

        private void TryHandleEvent(RoutedEventArgs e)
        {
            if (_interactionState == InteractionState.InProgress || _interactionState == InteractionState.Ending)
            {
                HandleEvent(e);
            }
        }

        internal void BeginDrag(RoutedEventArgs e)
        {
            if (CanCaptureInput(e))
            {
                RequiresInputCapture = IsToggle;

                _interactionState = InteractionState.InProgress;
                _initialPosition = GetInitialPosition(e);

                HandleEvent(e);
                OnBegin(e);

                e.Handled = true;

                Element.Focus();
                CaptureInput(e);
            }
        }

        private void EndDrag(RoutedEventArgs e)
        {
            _interactionState = InteractionState.Ready;
            HandleEvent(e);

            if (HasContextMenu && e is PointerReleasedEventArgs pre
                && pre.InitialPressMouseButton == MouseButton.Right)
            {
                var pos = pre.GetPosition(PositionElement);
                double dragThreshold = NodifyEditor.MouseActionSuppressionThreshold * NodifyEditor.MouseActionSuppressionThreshold;
                var v = pos - _initialPosition;
                double dragDistance = v.X * v.X + v.Y * v.Y;

                if (dragDistance > dragThreshold)
                {
                    OnEnd(e);
                    e.Handled = true;
                }
                else
                {
                    OnCancel(e);
                }
            }
            else
            {
                OnEnd(e);
                e.Handled = true;
            }

            RequiresInputCapture = false;
        }

        private void CancelDrag(RoutedEventArgs e)
        {
            _interactionState = InteractionState.Ready;
            HandleEvent(e);
            OnCancel(e);

            e.Handled = true;
            RequiresInputCapture = false;
        }

        #endregion

        /// <summary>
        /// Retrieves the initial pointer position relative to the <see cref="PositionElement"/>.
        /// </summary>
        protected virtual Point GetInitialPosition(RoutedEventArgs e)
        {
            if (e is PointerEventArgs pe)
            {
                return pe.GetPosition(PositionElement);
            }

            return default;
        }

        /// <summary>
        /// Determines whether input capture can be acquired for the element.
        /// </summary>
        protected virtual bool CanCaptureInput(RoutedEventArgs e)
        {
            if (e is PointerEventArgs pe)
                return pe.Pointer.Captured == null || pe.Pointer.Captured == Element;
            return true;
        }

        /// <summary>
        /// Captures pointer input for the element.
        /// </summary>
        protected virtual void CaptureInput(RoutedEventArgs e)
        {
            if (e is PointerEventArgs pe)
                pe.Pointer.Capture(Element);
        }

        /// <summary>
        /// Determines whether pointer capture has been lost.
        /// </summary>
        protected virtual bool IsInputCaptureLost(RoutedEventArgs e)
            => e.RoutedEvent == InputElement.PointerCaptureLostEvent;

        /// <summary>
        /// Determines if the given event represents the release of an input gesture.
        /// </summary>
        protected virtual bool IsInputEventReleased(RoutedEventArgs e)
        {
            if (e is PointerReleasedEventArgs)
                return true;

            if (e is KeyEventArgs && e.RoutedEvent == InputElement.KeyUpEvent)
                return true;

            return false;
        }

        /// <summary>
        /// Determines if the given event represents the press of an input gesture.
        /// </summary>
        protected virtual bool IsInputEventPressed(RoutedEventArgs e)
        {
            if (e is PointerPressedEventArgs)
                return true;

            if (e is KeyEventArgs && e.RoutedEvent == InputElement.KeyDownEvent)
                return true;

            return false;
        }

        /// <summary>Called when the drag interaction begins.</summary>
        protected virtual void OnBegin(RoutedEventArgs e) { }

        /// <summary>Called when the drag interaction ends.</summary>
        protected virtual void OnEnd(RoutedEventArgs e) { }

        /// <summary>Called when the drag interaction is canceled.</summary>
        protected virtual void OnCancel(RoutedEventArgs e) { }
    }
}
