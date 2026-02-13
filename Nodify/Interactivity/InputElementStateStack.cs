using System.Collections.Generic;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;

namespace Nodify.Interactivity
{
    /// <summary>
    /// Manages a stack of input states for a control, enabling complex input interactions.
    /// </summary>
    /// <typeparam name="TElement">The type of the associated Control.</typeparam>
    public partial class InputElementStateStack<TElement> : IInputHandler
        where TElement : Control
    {
        private readonly Stack<IInputElementState> _states = new Stack<IInputElementState>();

        /// <summary>
        /// Gets the associated element for which this state stack is managing input states.
        /// </summary>
        protected TElement Element { get; }

        public bool RequiresInputCapture => State.RequiresInputCapture;

        public bool ProcessHandledEvents => State.ProcessHandledEvents;

        /// <summary>
        /// Initializes a new instance of the <see cref="InputElementStateStack{TElement}"/> class.
        /// </summary>
        public InputElementStateStack(TElement element)
        {
            Element = element;
        }

        /// <summary>
        /// Gets the current state at the top of the stack.
        /// </summary>
        public IInputElementState State => _states.Peek();

        /// <summary>Pushes a new state into the stack.</summary>
        public void PushState(IInputElementState newState)
        {
            var prev = _states.Count > 0 ? State : null;
            _states.Push(newState);
            newState.Enter(prev);
        }

        /// <summary>Pops the current state from the stack.</summary>
        public void PopState()
        {
            if (_states.Count > 1)
            {
                IInputElementState prev = _states.Pop();
                prev.Exit();
                State.Enter(prev);
            }
        }

        /// <summary>Pops all states from the stack.</summary>
        public void PopAllStates()
        {
            while (_states.Count > 1)
            {
                PopState();
            }
        }

        public void HandleEvent(RoutedEventArgs e)
        {
            State.HandleEvent(e);

            if (e.RoutedEvent == InputElement.PointerCaptureLostEvent)
            {
                PopAllStates();
            }
        }

        /// <summary>
        /// Interface representing a state in the input state stack.
        /// </summary>
        public interface IInputElementState : IInputHandler
        {
            /// <summary>
            /// Invoked when entering this state from another state.
            /// </summary>
            void Enter(IInputElementState? from);

            /// <summary>
            /// Invoked when exiting this state.
            /// </summary>
            void Exit();
        }
    }
}
