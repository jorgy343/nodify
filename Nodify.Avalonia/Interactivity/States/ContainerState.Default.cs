using Avalonia;
using Avalonia.Input;
using System;

namespace Nodify.Interactivity;

public static partial class ContainerState
{
    /// <summary>The default state of the <see cref="ItemContainer"/>.</summary>
    public sealed class Default : InputElementStateStack<ItemContainer>
    {
        public Default(ItemContainer container) : base(container)
        {
            // TODO: Implement full state machine with selecting and dragging
            // For now, this is a placeholder to allow ItemContainer to compile
            // PushState(new SelectingState(this));
        }

        // TODO: Port ContainerState.SelectingState from WPF
        // This will handle:
        // - Mouse down for selection type detection
        // - Mouse move for drag threshold checking
        // - Mouse up for committing selection
        // - Transition to Dragging state when threshold is exceeded
    }
}
