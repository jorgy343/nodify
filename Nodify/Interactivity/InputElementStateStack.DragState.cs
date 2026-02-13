using Avalonia.Controls;
using Avalonia.Interactivity;

namespace Nodify.Interactivity
{
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

            private readonly RoutedEventArgs _dummyEventArgs = new RoutedEventArgs();

            /// <summary>
            /// Initializes a new instance of the <see cref="DragState"/> class.
            /// </summary>
            public DragState(InputElementStateStack<TElement> stack, InputGesture exitGesture, InputGesture cancelGesture)
                : base(stack.Element, exitGesture, cancelGesture)
            {
                PositionElement = stack.Element;
                Stack = stack;
            }

            /// <summary>
            /// Initializes a new instance of the <see cref="DragState"/> class with an optional cancel gesture.
            /// </summary>
            public DragState(InputElementStateStack<TElement> stack, InputGesture exitGesture)
                : base(stack.Element, exitGesture)
            {
                PositionElement = stack.Element;
                Stack = stack;
            }

            public void Enter(IInputElementState? from)
                => BeginDrag(_dummyEventArgs);

            public void Exit()
            {
            }

            /// <summary>
            /// Pushes a new state onto the stack.
            /// </summary>
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
}
