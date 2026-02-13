using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Styling;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Diagnostics;
using System.Linq;
using System.Windows.Input;

namespace Nodify;

public partial class NodifyEditor
{
    #region Selection Properties

    public static readonly StyledProperty<ICommand?> ItemsSelectStartedCommandProperty =
        AvaloniaProperty.Register<NodifyEditor, ICommand?>(nameof(ItemsSelectStartedCommand));

    public static readonly StyledProperty<ICommand?> ItemsSelectCompletedCommandProperty =
        AvaloniaProperty.Register<NodifyEditor, ICommand?>(nameof(ItemsSelectCompletedCommand));

    public static readonly StyledProperty<IStyle?> SelectionRectangleStyleProperty =
        AvaloniaProperty.Register<NodifyEditor, IStyle?>(nameof(SelectionRectangleStyle));

    private static readonly DirectProperty<NodifyEditor, Rect> SelectedAreaPropertyKey =
        AvaloniaProperty.RegisterDirect<NodifyEditor, Rect>(
            nameof(SelectedArea),
            o => o._selectedArea,
            (o, v) => o._selectedArea = v);

    private Rect _selectedArea;

    public static readonly StyledProperty<bool> EnableRealtimeSelectionProperty =
        AvaloniaProperty.Register<NodifyEditor, bool>(nameof(EnableRealtimeSelection), defaultValue: false);

    public static readonly StyledProperty<bool> CanSelectMultipleConnectionsProperty =
        AvaloniaProperty.Register<NodifyEditor, bool>(nameof(CanSelectMultipleConnections), defaultValue: true);

    public static readonly StyledProperty<bool> CanSelectMultipleItemsProperty =
        AvaloniaProperty.Register<NodifyEditor, bool>(nameof(CanSelectMultipleItems), defaultValue: true);

    public static readonly StyledProperty<IList?> SelectedItemsProperty =
        AvaloniaProperty.Register<NodifyEditor, IList?>(nameof(SelectedItems), defaultValue: null);

    public static readonly StyledProperty<IList?> SelectedConnectionsProperty =
        AvaloniaProperty.Register<NodifyEditor, IList?>(nameof(SelectedConnections), defaultValue: null);

    public static readonly StyledProperty<object?> SelectedConnectionProperty =
        AvaloniaProperty.Register<NodifyEditor, object?>(
            nameof(SelectedConnection),
            defaultValue: null,
            defaultBindingMode: Avalonia.Data.BindingMode.TwoWay);

    /// <summary>Invoked when a selection operation is started (see <see cref="BeginSelecting(SelectionType)"/>).</summary>
    public ICommand? ItemsSelectStartedCommand
    {
        get => GetValue(ItemsSelectStartedCommandProperty);
        set => SetValue(ItemsSelectStartedCommandProperty, value);
    }

    /// <summary>Invoked when a selection operation is completed (see <see cref="EndSelecting"/>).</summary>
    public ICommand? ItemsSelectCompletedCommand
    {
        get => GetValue(ItemsSelectCompletedCommandProperty);
        set => SetValue(ItemsSelectCompletedCommandProperty, value);
    }

    /// <summary>
    /// Gets or sets whether multiple connections can be selected.
    /// </summary>
    public bool CanSelectMultipleConnections
    {
        get => GetValue(CanSelectMultipleConnectionsProperty);
        set => SetValue(CanSelectMultipleConnectionsProperty, value);
    }

    /// <summary>
    /// Gets or sets whether multiple <see cref="ItemContainer" />s can be selected.
    /// </summary>
    public bool CanSelectMultipleItems
    {
        get => GetValue(CanSelectMultipleItemsProperty);
        set => SetValue(CanSelectMultipleItemsProperty, value);
    }

    /// <summary>
    /// Enables selecting and deselecting items while the <see cref="SelectedArea"/> changes.
    /// Disable for maximum performance when hundreds of items are generated.
    /// </summary>
    public bool EnableRealtimeSelection
    {
        get => GetValue(EnableRealtimeSelectionProperty);
        set => SetValue(EnableRealtimeSelectionProperty, value);
    }

    /// <summary>
    /// Gets or sets the selected connection.
    /// </summary>
    public object? SelectedConnection
    {
        get => GetValue(SelectedConnectionProperty);
        set => SetValue(SelectedConnectionProperty, value);
    }

    /// <summary>
    /// Gets or sets the selected connections in the <see cref="NodifyEditor"/>.
    /// </summary>
    public IList? SelectedConnections
    {
        get => GetValue(SelectedConnectionsProperty);
        set => SetValue(SelectedConnectionsProperty, value);
    }

    /// <summary>
    /// Gets or sets the selected items in the <see cref="NodifyEditor"/>.
    /// </summary>
    public IList? SelectedItems
    {
        get => GetValue(SelectedItemsProperty);
        set => SetValue(SelectedItemsProperty, value);
    }

    /// <summary>
    /// Gets the currently selected area while <see cref="IsSelecting"/> is true.
    /// </summary>
    public Rect SelectedArea
    {
        get => _selectedArea;
        private set => SetAndRaise(SelectedAreaPropertyKey, ref _selectedArea, value);
    }

    /// <summary>
    /// Gets or sets the style to use for the selection rectangle.
    /// </summary>
    public IStyle? SelectionRectangleStyle
    {
        get => GetValue(SelectionRectangleStyleProperty);
        set => SetValue(SelectionRectangleStyleProperty, value);
    }

    #endregion

    /// <summary>
    /// Gets a list of <see cref="ItemContainer"/>s that are selected (see <see cref="SelectedContainersCount"/>).
    /// </summary>
    /// <remarks>Cache the result before using it to avoid extra allocations.</remarks>
    protected internal IReadOnlyList<ItemContainer> SelectedContainers
    {
        get
        {
            // TODO: Implement once ItemsControl infrastructure is in place
            // For now, return empty list
            return Array.Empty<ItemContainer>();
        }
    }

    /// <summary>
    /// Gets or sets whether cancelling a selection operation is allowed (see <see cref="EditorGestures.SelectionGestures.Cancel"/>).
    /// </summary>
    public static bool AllowSelectionCancellation { get; set; } = true;

    /// <summary>The selection helper.</summary>
    private readonly SelectionHelper _selection = new SelectionHelper();

    // TODO: Add property change handlers in static constructor for IsSelecting
    // to call OnItemsSelectStarted/OnItemsSelectCompleted

    private void OnItemsSelectCompleted()
    {
        if (ItemsSelectCompletedCommand?.CanExecute(DataContext) ?? false)
            ItemsSelectCompletedCommand.Execute(DataContext);
    }

    private void OnItemsSelectStarted()
    {
        if (ItemsSelectStartedCommand?.CanExecute(DataContext) ?? false)
            ItemsSelectStartedCommand.Execute(DataContext);
    }

    #region Selection

    /// <summary>
    /// Inverts the <see cref="ItemContainer"/>s selection in the specified <paramref name="area"/>.
    /// </summary>
    /// <param name="area">The area to look for <see cref="ItemContainer"/>s.</param>
    /// <param name="fit">True to check if the <paramref name="area"/> contains the <see cref="ItemContainer"/>. <br />False to check if <paramref name="area"/> intersects the <see cref="ItemContainer"/>.</param>
    public void InvertSelection(Rect area, bool fit = false)
    {
        // TODO: Implement once ItemsControl infrastructure is in place
        IsSelecting = true;
        // BeginUpdateSelectedItems();
        // ... iterate through items and invert selection
        // EndUpdateSelectedItems();
        IsSelecting = false;
    }

    /// <summary>
    /// Selects the <see cref="ItemContainer"/>s in the specified <paramref name="area"/>.
    /// </summary>
    /// <param name="area">The area to look for <see cref="ItemContainer"/>s.</param>
    /// <param name="append">If true, it will add to the existing selection.</param>
    /// <param name="fit">True to check if the <paramref name="area"/> contains the <see cref="ItemContainer"/>. <br />False to check if <paramref name="area"/> intersects the <see cref="ItemContainer"/>.</param>
    public void SelectArea(Rect area, bool append = false, bool fit = false)
    {
        IsSelecting = true;
        // BeginUpdateSelectedItems();

        IList? selected = SelectedItems;
        if (selected != null && !append)
        {
            selected.Clear();
        }

        // TODO: Implement once ItemsControl infrastructure is in place
        // ... iterate through items and select those in area

        // EndUpdateSelectedItems();
        IsSelecting = false;
    }

    /// <summary>
    /// Clears the current selection and selects the specified <see cref="ItemContainer"/> within the same selection transaction.
    /// </summary>
    /// <param name="container"></param>
    public void Select(ItemContainer container)
    {
        // BeginUpdateSelectedItems();
        var selected = SelectedItems;
        if (selected != null)
        {
            selected.Clear();
            selected.Add(container.DataContext!);
        }
        // EndUpdateSelectedItems();

        UnselectAllConnections();
    }

    /// <summary>
    /// Unselect the <see cref="ItemContainer"/>s in the specified <paramref name="area"/>.
    /// </summary>
    /// <param name="area">The area to look for <see cref="ItemContainer"/>s.</param>
    /// <param name="fit">True to check if the <paramref name="area"/> contains the <see cref="ItemContainer"/>. <br />False to check if <paramref name="area"/> intersects the <see cref="ItemContainer"/>.</param>
    public void UnselectArea(Rect area, bool fit = false)
    {
        IList? items = SelectedItems;

        IsSelecting = true;
        // BeginUpdateSelectedItems();
        // TODO: Implement once ItemsControl infrastructure is in place
        // EndUpdateSelectedItems();
        IsSelecting = false;
    }

    /// <summary>
    /// Unselect all <see cref="Connections"/>.
    /// </summary>
    public void UnselectAllConnections()
    {
        // TODO: Implement when ConnectionsHost is added
        // if (ConnectionsHost is some selector)
        // {
        //     selector.UnselectAll();
        // }
    }

    /// <summary>
    /// Select all <see cref="Connections"/>.
    /// </summary>
    public void SelectAllConnections()
    {
        // TODO: Implement when ConnectionsHost is added
        // if (ConnectionsHost is some selector)
        // {
        //     selector.SelectAll();
        // }
    }

    /// <summary>
    /// Initiates a selection operation from the current mouse/pointer location.
    /// </summary>
    /// <remarks>This method has no effect if a selection operation is already in progress.</remarks>
    /// <param name="type">The type of selection to perform. Defaults to <see cref="SelectionType.Replace"/>.</param>
    public void BeginSelecting(SelectionType type = SelectionType.Replace)
    {
        // TODO: Get actual pointer location when pointer event handling is added
        Point location = new Point(0, 0);
        BeginSelecting(location, type);
    }

    /// <summary>
    /// Initiates a selection operation from the specified location.
    /// </summary>
    /// <remarks>This method has no effect if a selection operation is already in progress.</remarks>
    /// <param name="location">The starting point for the selection, in graph space coordinates.</param>
    /// <param name="type">The type of selection to perform. Defaults to <see cref="SelectionType.Replace"/>.</param>
    public void BeginSelecting(Point location, SelectionType type = SelectionType.Replace)
    {
        if (IsSelecting)
        {
            return;
        }

        // TODO: Pass ItemContainers once available
        SelectedArea = _selection.Start(Array.Empty<ItemContainer>(), location, type, EnableRealtimeSelection);
        IsSelecting = true;
    }

    /// <summary>
    /// Expands or modifies the selection area by the specified amount.
    /// </summary>
    /// <param name="amount">Represents the change to apply to the selection area.</param>
    public void UpdateSelection(Vector amount)
    {
        Debug.Assert(IsSelecting);
        SelectedArea = _selection.Update(amount);
    }

    /// <summary>
    /// Expands or modifies the selection area to the specified location.
    /// </summary>
    /// <param name="location">The point, in graph space coordinates, to extend or adjust the selection area to.</param>
    public void UpdateSelection(Point location)
    {
        Debug.Assert(IsSelecting);
        SelectedArea = _selection.Update(location);
    }

    /// <summary>
    /// Completes the selection operation and applies any pending changes.
    /// </summary>
    /// <remarks>This method has no effect if there's no selection operation in progress.</remarks>
    public void EndSelecting()
    {
        if (!IsSelecting)
        {
            return;
        }

        if (_selection.Type == SelectionType.Replace)
        {
            UnselectAllConnections();
        }

        SelectedArea = _selection.End();
        IsSelecting = false;
        ApplyPreviewingSelection();
    }

    /// <summary>
    /// Cancels the current selection operation and reverts any changes made during the selection process if <see cref="AllowSelectionCancellation"/> is true.
    /// Otherwise, it ends the selection operation by calling <see cref="EndSelecting"/>.
    /// </summary>
    /// <remarks>This method has no effect if there's no selection operation in progress.</remarks>
    public void CancelSelecting()
    {
        if (!AllowSelectionCancellation)
        {
            EndSelecting();
            return;
        }

        if (IsSelecting)
        {
            _selection.Cancel();
            IsSelecting = false;
        }
    }

    private void ApplyPreviewingSelection()
    {
        // TODO: Implement once ItemsControl infrastructure is in place
        // IList? selected = SelectedItems;
        // if (selected == null)
        //     return;
        //
        // BeginUpdateSelectedItems();
        // ... iterate through containers and apply IsPreviewingSelection states
        // EndUpdateSelectedItems();
    }

    #endregion

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

        IList? selectedItems = SelectedItems;

        // BeginUpdateSelectedItems();
        selectedItems?.Clear();
        if (newValue != null && selectedItems != null)
        {
            for (var i = 0; i < newValue.Count; i++)
            {
                selectedItems.Add(newValue[i]);
            }
        }
        // EndUpdateSelectedItems();
    }

    private void OnSelectedItemsChanged(object? sender, NotifyCollectionChangedEventArgs e)
    {
        if (!CanSelectMultipleItems)
            return;

        IList? selectedItems = SelectedItems;
        if (selectedItems == null)
            return;

        switch (e.Action)
        {
            case NotifyCollectionChangedAction.Reset:
                selectedItems.Clear();
                break;

            case NotifyCollectionChangedAction.Add:
                IList? newItems = e.NewItems;
                if (newItems != null)
                {
                    for (var i = 0; i < newItems.Count; i++)
                    {
                        selectedItems.Add(newItems[i]);
                    }
                }
                break;

            case NotifyCollectionChangedAction.Remove:
                IList? oldItems = e.OldItems;
                if (oldItems != null)
                {
                    for (var i = 0; i < oldItems.Count; i++)
                    {
                        selectedItems.Remove(oldItems[i]);
                    }
                }
                break;
        }
    }

    #endregion
}
