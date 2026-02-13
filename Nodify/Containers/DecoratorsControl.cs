using Nodify.Interactivity;
using System.Collections.Generic;
using System.Linq;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Input;
using Avalonia.VisualTree;

namespace Nodify
{
    /// <summary>
    /// An <see cref="ItemsControl"/> that works with <see cref="DecoratorContainer"/>s.
    /// </summary>
    public class DecoratorsControl : ItemsControl, IKeyboardNavigationLayer
    {
        /// <summary>
        /// Gets the <see cref="NodifyEditor"/> that owns this <see cref="DecoratorsControl"/>.
        /// </summary>
        public NodifyEditor? Editor { get; private set; }

        /// <summary>
        /// Gets a list of all <see cref="DecoratorContainer"/>s.
        /// </summary>
        /// <remarks>Cache the result before using it to avoid extra allocations.</remarks>
        protected internal IReadOnlyCollection<DecoratorContainer> DecoratorContainers
        {
            get
            {
                var count = ItemCount;
                var containers = new List<DecoratorContainer>(count);

                for (var i = 0; i < count; i++)
                {
                    containers.Add((DecoratorContainer)ContainerFromIndex(i)!);
                }

                return containers;
            }
        }

        static DecoratorsControl()
        {
            FocusableProperty.OverrideDefaultValue<DecoratorsControl>(false);
            KeyboardNavigation.TabNavigationProperty.OverrideDefaultValue<DecoratorsControl>(KeyboardNavigationMode.None);
            KeyboardNavigation.DirectionalNavigationProperty.OverrideDefaultValue<DecoratorsControl>(KeyboardNavigationMode.None);
        }

        public DecoratorsControl()
        {
            _focusNavigator = new StatefulFocusNavigator<DecoratorContainer>(OnElementFocused);
        }

        /// <inheritdoc />
        protected override bool NeedsContainerOverride(object? item, int index, out object? recycleKey)
        {
            if (item is DecoratorContainer)
            {
                recycleKey = null;
                return false;
            }
            recycleKey = typeof(DecoratorContainer);
            return true;
        }

        /// <inheritdoc />
        protected override Control CreateContainerForItemOverride(object? item, int index, object? recycleKey)
            => new DecoratorContainer(this);

        protected override void OnApplyTemplate(TemplateAppliedEventArgs e)
        {
            base.OnApplyTemplate(e);

            Editor = this.GetParentOfType<NodifyEditor>();

            if (NodifyEditor.AutoRegisterDecoratorsLayer)
            {
                Editor?.RegisterNavigationLayer(this);
            }
        }

        #region Keyboard Navigation

        public KeyboardNavigationLayerId Id { get; } = KeyboardNavigationLayerId.Decorators;
        public IKeyboardFocusTarget<Control>? LastFocusedElement => _focusNavigator.LastFocusedElement;

        private readonly StatefulFocusNavigator<DecoratorContainer> _focusNavigator;

        public bool TryMoveFocus(TraversalRequest request)
        {
            return _focusNavigator.TryMoveFocus(request, TryFindContainerToFocus);
        }

        public bool TryRestoreFocus()
        {
            return _focusNavigator.TryRestoreFocus();
        }

        private bool TryFindContainerToFocus(DecoratorContainer? currentElement, TraversalRequest request, out DecoratorContainer? containerToFocus)
        {
            containerToFocus = null;

            if (currentElement is DecoratorContainer focusedContainer)
            {
                containerToFocus = FindNextFocusTarget(focusedContainer, request);
            }
            else if (currentElement is Control elem && elem.GetParentOfType<DecoratorContainer>() is DecoratorContainer parentContainer)
            {
                containerToFocus = parentContainer;
            }
            else if (ItemCount > 0 && Editor != null)
            {
                var viewport = new Rect(Editor.ViewportLocation, Editor.ViewportSize);
                var containers = DecoratorContainers;
                containerToFocus = containers.FirstOrDefault(container => viewport.Intersects(((IKeyboardFocusTarget<DecoratorContainer>)container).Bounds))
                    ?? containers.First();
            }

            return containerToFocus != null;
        }

        protected virtual DecoratorContainer? FindNextFocusTarget(DecoratorContainer currentContainer, TraversalRequest request)
        {
            var focusNavigator = new DirectionalFocusNavigator<DecoratorContainer>(DecoratorContainers);
            var result = focusNavigator.FindNextFocusTarget(currentContainer, request);

            return result?.Element;
        }

        protected virtual void OnElementFocused(IKeyboardFocusTarget<DecoratorContainer> target)
        {
            if (NodifyEditor.AutoPanOnNodeFocus)
            {
                Editor?.BringIntoView(target.Bounds, NodifyEditor.BringIntoViewEdgeOffset);
            }
        }

        void IKeyboardNavigationLayer.OnActivated()
        {
            TryRestoreFocus();
        }

        void IKeyboardNavigationLayer.OnDeactivated()
        {
        }

        #endregion
    }
}
