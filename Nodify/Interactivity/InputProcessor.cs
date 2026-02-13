using System.Collections.Generic;
using Avalonia.Interactivity;

namespace Nodify.Interactivity
{
    /// <summary>
    /// Processes input events and delegates them to registered handlers.
    /// </summary>
    public partial class InputProcessor
    {
        private readonly List<IInputHandler> _handlers = new List<IInputHandler>();

        /// <summary>
        /// Gets a value indicating whether the processor has ongoing interactions that require input capture to remain active.
        /// </summary>
        public bool RequiresInputCapture { get; private set; }

        /// <summary>
        /// Adds an input handler to the processor.
        /// </summary>
        public void AddHandler(IInputHandler handler)
            => _handlers.Add(handler);

        /// <summary>
        /// Removes all handlers of the specified type from the processor.
        /// </summary>
        public void RemoveHandlers<T>() where T : IInputHandler
            => _handlers.RemoveAll(x => x.GetType() == typeof(T));

        /// <summary>
        /// Clears all registered handlers.
        /// </summary>
        public void Clear()
            => _handlers.Clear();

        /// <summary>
        /// Processes an input event and delegates it to the registered handlers.
        /// </summary>
        public void ProcessEvent(RoutedEventArgs e)
        {
            RequiresInputCapture = false;

            for (int i = 0; i < _handlers.Count; i++)
            {
                IInputHandler handler = _handlers[i];
                if (!e.Handled || handler.ProcessHandledEvents)
                {
                    handler.HandleEvent(e);
                    RequiresInputCapture |= handler.RequiresInputCapture;
                }
            }
        }
    }
}
