using Avalonia;
using Avalonia.Input;

namespace Nodify.Interactivity
{
    public static partial class ContainerState
    {
        /// <summary>The default state of the <see cref="ItemContainer"/>.</summary>
        public sealed class Default : InputElementStateStack<ItemContainer>
        {
            public Default(ItemContainer container) : base(container)
            {
                PushState(new SelectingState(this));
            }

            private sealed class SelectingState : InputElementState
            {
                private Point _initialPosition;
                private SelectionType? _selectionType;
                private bool _isDragging;

                private bool PreserveSelectionOnRightClick => Element.HasContextMenu || ItemContainer.PreserveSelectionOnRightClick;

                /// <summary>Creates a new instance of the <see cref="SelectingState"/>.</summary>
                /// <param name="stack">The owner of the state.</param>
                public SelectingState(InputElementStateStack<ItemContainer> stack) : base(stack)
                {
                }

                /// <inheritdoc />
                public override void Enter(IInputElementState? from)
                {
                    _isDragging = false;
                    _selectionType = null;
                    _initialPosition = Element.Editor.MouseLocation;
                }

                protected override void OnPointerPressed(PointerPressedEventArgs e)
                {
                    if (!Element.IsSelectableLocation(e.GetPosition(Element)))
                    {
                        return;
                    }

                    EditorGestures.ItemContainerGestures gestures = EditorGestures.Mappings.ItemContainer;
                    if (gestures.Drag.Matches(e.Source, e))
                    {
                        // Dragging requires pointer capture
                        _isDragging = Element.IsDraggable && CanCapturePointer(e);
                    }

                    if (gestures.Selection.Select.Matches(e.Source, e))
                    {
                        _selectionType = gestures.Selection.GetSelectionType(e);
                    }
                    // Replaces the current selection when right-clicking on an element that has a context menu and is not selected.
                    // Applies only when the select gesture is not right click.
                    else if (e.GetCurrentPoint(null).Properties.PointerUpdateKind == PointerUpdateKind.RightButtonPressed && PreserveSelectionOnRightClick)
                    {
                        _selectionType = Element.IsSelected ? SelectionType.Append : SelectionType.Replace;
                    }

                    _initialPosition = Element.Editor.MouseLocation;

                    if (_isDragging || _selectionType.HasValue)
                    {
                        e.Handled = true;
                        CapturePointerSafe(e);
                    }
                }

                /// <inheritdoc />
                protected override void OnPointerMoved(PointerEventArgs e)
                {
                    double dragThreshold = NodifyEditor.MouseActionSuppressionThreshold * NodifyEditor.MouseActionSuppressionThreshold;
                    var delta = Element.Editor.MouseLocation - _initialPosition;
                    double dragDistance = delta.X * delta.X + delta.Y * delta.Y;

                    if (_isDragging && (dragDistance > dragThreshold))
                    {
                        if (!Element.IsSelected)
                        {
                            var selectionType = GetSelectionTypeForDragging(_selectionType);
                            Element.Select(selectionType);
                        }

                        PushState(new Dragging(Stack));
                    }
                }

                /// <inheritdoc />
                protected override void OnPointerReleased(PointerReleasedEventArgs e)
                {
                    if (_selectionType.HasValue)
                    {
                        // Determine whether the current selection should remain intact or be replaced by the clicked item.
                        // If the right mouse button is pressed on an already selected item, and the item either has an
                        // explicit context menu or is configured to preserve the selection on right-click, the selection
                        // remains unchanged. This ensures that the context menu applies to the entire selection rather
                        // than only the clicked item.
                        bool allowContextMenu = e.InitialPressMouseButton == MouseButton.Right && Element.IsSelected && PreserveSelectionOnRightClick;
                        if (!allowContextMenu)
                        {
                            Element.Select(_selectionType.Value);
                        }
                    }

                    _isDragging = false;
                    _selectionType = null;
                }

                private void CapturePointerSafe(PointerPressedEventArgs e)
                {
                    // Avoid stealing pointer capture from other elements
                    if (CanCapturePointer(e))
                    {
                        Element.Focus();
                        e.Pointer.Capture(Element);
                    }
                }

                private bool CanCapturePointer(PointerPressedEventArgs e)
                    => e.Pointer.Captured == null || e.Pointer.Captured == Element;

                private static SelectionType GetSelectionTypeForDragging(SelectionType? selectionType)
                {
                    // Always select the container when dragging
                    return selectionType == SelectionType.Remove
                        ? SelectionType.Replace
                        : selectionType.GetValueOrDefault(SelectionType.Replace);
                }
            }
        }
    }
}
