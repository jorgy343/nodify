using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;

namespace Nodify.Interactivity;

public partial class InputElementStateStack<TElement> where TElement : Control
{
    /// <summary>
    /// Represents a specialized state for handling drag interactions.
    /// </summary>
    public abstract class DragState : DragState<TElement>, IInputElementState, IInputHandler
    {
        /// <summary>
        /// Gets the state stack managing this state.
        /// </summary>
        public InputElementStateStack<TElement> Stack { get; }

        // Dummy event args for triggering BeginDrag when entering the state
        private readonly RoutedEventArgs _dummyEventArgs;

        /// <summary>
        /// Initializes a new instance of the <see cref="DragState"/> class.
        /// </summary>
        /// <param name="stack">The state stack managing this state.</param>
        /// <param name="exitGesture">The gesture used to exit the drag state.</param>
        /// <param name="cancelGesture">The gesture used to cancel the drag state.</param>
        public DragState(InputElementStateStack<TElement> stack, Gesture exitGesture, Gesture cancelGesture)
            : base(stack.Element, exitGesture, cancelGesture)
        {
            PositionElement = stack.Element;
            Stack = stack;
            _dummyEventArgs = new RoutedEventArgs(NodifyEditor.ViewportUpdatedEvent);
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="DragState"/> class with an optional cancel gesture.
        /// </summary>
        /// <param name="stack">The state stack managing this state.</param>
        /// <param name="exitGesture">The gesture used to exit the drag state.</param>
        public DragState(InputElementStateStack<TElement> stack, Gesture exitGesture)
            : base(stack.Element, exitGesture)
        {
            PositionElement = stack.Element;
            Stack = stack;
            _dummyEventArgs = new RoutedEventArgs(NodifyEditor.ViewportUpdatedEvent);
        }

        public void Enter(IInputElementState? from)
            => BeginDrag(_dummyEventArgs);

        public void Exit()
        {
        }

        /// <summary>
        /// Pushes a new state onto the stack.
        /// </summary>
        /// <param name="newState">The new state to push.</param>
        public void PushState(IInputElementState newState)
            => Stack.PushState(newState);

        /// <summary>
        /// Pops the current state from the stack.
        /// </summary>
        public void PopState()
            => Stack.PopState();

        protected override void OnCancel(RoutedEventArgs e)
            => PopState();

        protected override void OnEnd(RoutedEventArgs e)
            => PopState();
    }
}
