using Nodify.Interactivity;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Controls.Selection;
using Avalonia.Data;
using Avalonia.Input;
using Avalonia.VisualTree;

namespace Nodify
{
    public class ConnectionsMultiSelector : ListBox, IKeyboardNavigationLayer
    {
        #region Styled Properties

        public static readonly StyledProperty<IList?> SelectedItemsProperty =
            NodifyEditor.SelectedItemsProperty.AddOwner<ConnectionsMultiSelector>();
        public static readonly StyledProperty<bool> CanSelectMultipleItemsProperty =
            NodifyEditor.CanSelectMultipleItemsProperty.AddOwner<ConnectionsMultiSelector>();

        private static void OnCanSelectMultipleItemsChanged(ConnectionsMultiSelector d, AvaloniaPropertyChangedEventArgs e)
            => d.SelectionMode = e.GetNewValue<bool>() ? SelectionMode.Multiple : SelectionMode.Single;

        /// <summary>
        /// Gets or sets the selected connections in the <see cref="NodifyEditor"/>.
        /// </summary>
        public new IList? SelectedItems
        {
            get => (IList?)GetValue(SelectedItemsProperty);
            set => SetValue(SelectedItemsProperty, value);
        }

        /// <summary>
        /// Gets or sets whether multiple connections can be selected.
        /// </summary>
        public new bool CanSelectMultipleItems
        {
            get => (bool)GetValue(CanSelectMultipleItemsProperty);
            set => SetValue(CanSelectMultipleItemsProperty, value);
        }

        #endregion

        /// <summary>
        /// Gets the <see cref="NodifyEditor"/> that owns this <see cref="ConnectionsMultiSelector"/>.
        /// </summary>
        public NodifyEditor? Editor { get; private set; }

        /// <summary>
        /// Gets a list of all <see cref="ConnectionContainer"/>s.
        /// </summary>
        /// <remarks>Cache the result before using it to avoid extra allocations.</remarks>
        protected internal IReadOnlyCollection<ConnectionContainer> ConnectionContainers
        {
            get
            {
                var count = ItemCount;
                var containers = new List<ConnectionContainer>(count);

                for (var i = 0; i < count; i++)
                {
                    containers.Add((ConnectionContainer)ContainerFromIndex(i)!);
                }

                return containers;
            }
        }

        static ConnectionsMultiSelector()
        {
            FocusableProperty.OverrideDefaultValue<ConnectionsMultiSelector>(false);
            KeyboardNavigation.TabNavigationProperty.OverrideDefaultValue<ConnectionsMultiSelector>(KeyboardNavigationMode.None);
            KeyboardNavigation.DirectionalNavigationProperty.OverrideDefaultValue<ConnectionsMultiSelector>(KeyboardNavigationMode.None);
            SelectedItemsProperty.Changed.AddClassHandler<ConnectionsMultiSelector>(
                (d, e) => d.OnSelectedItemsSourceChanged((IList?)e.OldValue, (IList?)e.NewValue));
            CanSelectMultipleItemsProperty.Changed.AddClassHandler<ConnectionsMultiSelector>(OnCanSelectMultipleItemsChanged);
        }

        public ConnectionsMultiSelector()
        {
            _focusNavigator = new StatefulFocusNavigator<ConnectionContainer>(OnElementFocused);
            SelectionMode = SelectionMode.Multiple;
        }

        protected override bool NeedsContainerOverride(object? item, int index, out object? recycleKey)
        {
            if (item is ConnectionContainer)
            {
                recycleKey = null;
                return false;
            }
            recycleKey = typeof(ConnectionContainer);
            return true;
        }

        protected override Control CreateContainerForItemOverride(object? item, int index, object? recycleKey)
            => new ConnectionContainer(this);

        protected override void OnApplyTemplate(TemplateAppliedEventArgs e)
        {
            base.OnApplyTemplate(e);

            Editor = this.GetParentOfType<NodifyEditor>();

            if (NodifyEditor.AutoRegisterConnectionsLayer)
            {
                Editor?.RegisterNavigationLayer(this);
            }
        }

        #region Keyboard Navigation

        public KeyboardNavigationLayerId Id { get; } = KeyboardNavigationLayerId.Connections;
        public IKeyboardFocusTarget<Control>? LastFocusedElement => _focusNavigator.LastFocusedElement;

        private readonly StatefulFocusNavigator<ConnectionContainer> _focusNavigator;

        public bool TryMoveFocus(TraversalRequest request)
        {
            return _focusNavigator.TryMoveFocus(request, TryFindContainerToFocus);
        }

        public bool TryRestoreFocus()
        {
            return _focusNavigator.TryRestoreFocus();
        }

        private bool TryFindContainerToFocus(ConnectionContainer? currentElement, TraversalRequest request, out ConnectionContainer? containerToFocus)
        {
            containerToFocus = null;

            if (currentElement is ConnectionContainer focusedContainer)
            {
                containerToFocus = FindNextFocusTarget(focusedContainer, request);
            }
            else if (currentElement is Control elem && elem.GetParentOfType<ConnectionContainer>() is ConnectionContainer parentContainer)
            {
                containerToFocus = parentContainer;
            }
            else if (ItemCount > 0 && Editor != null)
            {
                var viewport = new Rect(Editor.ViewportLocation, Editor.ViewportSize);
                var containers = ConnectionContainers;
                containerToFocus = containers.FirstOrDefault(container => viewport.Intersects(((IKeyboardFocusTarget<ConnectionContainer>)container).Bounds))
                    ?? containers.First();
            }

            return containerToFocus != null;
        }

        protected virtual ConnectionContainer? FindNextFocusTarget(ConnectionContainer currentContainer, TraversalRequest request)
        {
            var focusNavigator = new DirectionalFocusNavigator<ConnectionContainer>(ConnectionContainers);
            var result = focusNavigator.FindNextFocusTarget(currentContainer, request);

            return result?.Element;
        }

        protected virtual void OnElementFocused(IKeyboardFocusTarget<ConnectionContainer> target)
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

        public void Select(ConnectionContainer container)
        {
            var baseSelected = base.SelectedItems;
            if (baseSelected == null)
                return;

            using (Selection.BatchUpdate())
            {
                baseSelected.Clear();
                baseSelected.Add(container.DataContext);
                container.IsSelected = true;
            }

            Editor?.UnselectAll();
        }

        #region Selection Handlers

        private void OnSelectedItemsSourceChanged(IList? oldValue, IList? newValue)
        {
            if (oldValue is INotifyCollectionChanged oc)
            {
                oc.CollectionChanged -= OnSelectedItemsChanged;
            }

            if (newValue is INotifyCollectionChanged nc)
            {
                nc.CollectionChanged += OnSelectedItemsChanged;
            }

            var baseSelected = base.SelectedItems;
            if (baseSelected == null)
                return;

            using (Selection.BatchUpdate())
            {
                baseSelected.Clear();
                if (newValue != null)
                {
                    for (var i = 0; i < newValue.Count; i++)
                    {
                        baseSelected.Add(newValue[i]);
                    }
                }
            }
        }

        private void OnSelectedItemsChanged(object? sender, NotifyCollectionChangedEventArgs e)
        {
            if (!CanSelectMultipleItems)
                return;

            var baseSelected = base.SelectedItems;
            if (baseSelected == null)
                return;

            switch (e.Action)
            {
                case NotifyCollectionChangedAction.Reset:
                    baseSelected.Clear();
                    break;

                case NotifyCollectionChangedAction.Add:
                    IList? newItems = e.NewItems;
                    if (newItems != null)
                    {
                        for (var i = 0; i < newItems.Count; i++)
                        {
                            baseSelected.Add(newItems[i]);
                        }
                    }
                    break;

                case NotifyCollectionChangedAction.Remove:
                    IList? oldItems = e.OldItems;
                    if (oldItems != null)
                    {
                        for (var i = 0; i < oldItems.Count; i++)
                        {
                            baseSelected.Remove(oldItems[i]);
                        }
                    }
                    break;
            }
        }

        protected override void OnSelectionChanged(SelectionChangedEventArgs e)
        {
            base.OnSelectionChanged(e);

            IList? selected = SelectedItems;
            if (selected != null)
            {
                IList added = e.AddedItems;
                for (var i = 0; i < added.Count; i++)
                {
                    // Ensure no duplicates are added
                    if (!selected.Contains(added[i]))
                    {
                        selected.Add(added[i]);
                    }
                }

                IList removed = e.RemovedItems;
                for (var i = 0; i < removed.Count; i++)
                {
                    selected.Remove(removed[i]);
                }
            }
        }

        #endregion
    }
}
