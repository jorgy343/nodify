using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.VisualTree;
using Nodify.Events;
using System;
using System.Diagnostics;
using System.Windows.Input;

namespace Nodify;

/// <summary>
/// Represents a connector control that can start and complete a <see cref="PendingConnection"/>.
/// Has a template part that the <see cref="Anchor"/> is calculated from for the <see cref="PendingConnection"/>. Center of this control is used if missing.
/// </summary>
/// <remarks>
/// NOTE: This is a partial implementation. Full functionality requires:
/// - ItemContainer with Location, PreviewLocationChanged, LocationChanged properties
/// - NodifyEditor with HasCustomContextMenuProperty, SelectedContainersCount, ItemsHost properties
/// - Full Interactivity system (InputProcessor, gesture handlers)
/// These will be implemented when those components are ported.
/// </remarks>
public class Connector : TemplatedControl
{
    protected const string ElementConnector = "PART_Connector";

    #region Routed Events

    public static readonly RoutedEvent<PendingConnectionEventArgs> PendingConnectionStartedEvent =
        RoutedEvent.Register<Connector, PendingConnectionEventArgs>(
            nameof(PendingConnectionStarted),
            RoutingStrategies.Bubble);

    public static readonly RoutedEvent<PendingConnectionEventArgs> PendingConnectionCompletedEvent =
        RoutedEvent.Register<Connector, PendingConnectionEventArgs>(
            nameof(PendingConnectionCompleted),
            RoutingStrategies.Bubble);

    public static readonly RoutedEvent<PendingConnectionEventArgs> PendingConnectionDragEvent =
        RoutedEvent.Register<Connector, PendingConnectionEventArgs>(
            nameof(PendingConnectionDrag),
            RoutingStrategies.Bubble);

    public static readonly RoutedEvent<ConnectorEventArgs> DisconnectEvent =
        RoutedEvent.Register<Connector, ConnectorEventArgs>(
            nameof(Disconnect),
            RoutingStrategies.Bubble);

    /// <summary>Triggered by connector connect gestures.</summary>
    public event EventHandler<PendingConnectionEventArgs> PendingConnectionStarted
    {
        add => AddHandler(PendingConnectionStartedEvent, value);
        remove => RemoveHandler(PendingConnectionStartedEvent, value);
    }

    /// <summary>Triggered by connector connect gestures.</summary>
    public event EventHandler<PendingConnectionEventArgs> PendingConnectionCompleted
    {
        add => AddHandler(PendingConnectionCompletedEvent, value);
        remove => RemoveHandler(PendingConnectionCompletedEvent, value);
    }

    /// <summary>
    /// Occurs when the pointer is changing position and the <see cref="Connector"/> has pointer capture.
    /// </summary>
    public event EventHandler<PendingConnectionEventArgs> PendingConnectionDrag
    {
        add => AddHandler(PendingConnectionDragEvent, value);
        remove => RemoveHandler(PendingConnectionDragEvent, value);
    }

    /// <summary>Triggered by connector disconnect gestures.</summary>
    public event EventHandler<ConnectorEventArgs> Disconnect
    {
        add => AddHandler(DisconnectEvent, value);
        remove => RemoveHandler(DisconnectEvent, value);
    }

    #endregion

    #region Styled Properties

    public static readonly StyledProperty<Point> AnchorProperty =
        AvaloniaProperty.Register<Connector, Point>(nameof(Anchor), defaultValue: default(Point));

    public static readonly StyledProperty<bool> IsConnectedProperty =
        AvaloniaProperty.Register<Connector, bool>(nameof(IsConnected), defaultValue: false);

    public static readonly StyledProperty<ICommand?> DisconnectCommandProperty =
        AvaloniaProperty.Register<Connector, ICommand?>(nameof(DisconnectCommand));

    private static readonly DirectProperty<Connector, bool> IsPendingConnectionPropertyKey =
        AvaloniaProperty.RegisterDirect<Connector, bool>(
            nameof(IsPendingConnection),
            o => o.IsPendingConnection);

    // TODO: Add back when NodifyEditor.HasCustomContextMenuProperty is implemented
    // public static readonly StyledProperty<bool> HasCustomContextMenuProperty =
    //     NodifyEditor.HasCustomContextMenuProperty.AddOwner<Connector>();

    public static readonly StyledProperty<bool> HasCustomContextMenuProperty =
        AvaloniaProperty.Register<Connector, bool>(nameof(HasCustomContextMenu), defaultValue: false);

    /// <summary>
    /// Gets the location in graph space coordinates where <see cref="Connection"/>s can be attached to.
    /// Bind with <see cref="Avalonia.Data.BindingMode.OneWayToSource"/>
    /// </summary>
    public Point Anchor
    {
        get => GetValue(AnchorProperty);
        set => SetValue(AnchorProperty, value);
    }

    /// <summary>
    /// If this is set to false, the <see cref="Disconnect"/> event will not be invoked and the connector will stop updating its <see cref="Anchor"/> when moved, resized etc.
    /// </summary>
    public bool IsConnected
    {
        get => GetValue(IsConnectedProperty);
        set => SetValue(IsConnectedProperty, value);
    }

    private bool _isPendingConnection;
    /// <summary>
    /// Gets a value that indicates whether a <see cref="PendingConnection"/> is in progress for this <see cref="Connector"/>.
    /// </summary>
    public bool IsPendingConnection
    {
        get => _isPendingConnection;
        protected set => SetAndRaise(IsPendingConnectionPropertyKey, ref _isPendingConnection, value);
    }

    /// <summary>
    /// Invoked if the <see cref="Disconnect"/> event is not handled.
    /// Parameter is the <see cref="StyledElement.DataContext"/> of this control.
    /// </summary>
    public ICommand? DisconnectCommand
    {
        get => GetValue(DisconnectCommandProperty);
        set => SetValue(DisconnectCommandProperty, value);
    }

    /// <summary>
    /// Gets or sets a value indicating whether the connector uses a custom context menu.
    /// </summary>
    /// <remarks>When set to true, the connector handles the right-click event for specific interactions.</remarks>
    public bool HasCustomContextMenu
    {
        get => GetValue(HasCustomContextMenuProperty);
        set => SetValue(HasCustomContextMenuProperty, value);
    }

    /// <summary>
    /// Gets a value indicating whether the connector has a context menu.
    /// </summary>
    public bool HasContextMenu => ContextMenu != null || HasCustomContextMenu;

    #endregion

    #region Fields

    private Control? _thumb;
    /// <summary>
    /// Gets the <see cref="Control"/> used to calculate the <see cref="Anchor"/>.
    /// </summary>
    protected internal Control Thumb => _thumb ??= this.FindNameScope()?.Find<Control>(ElementConnector) ?? this;

    /// <summary>
    /// Gets the <see cref="ItemContainer"/> that contains this <see cref="Connector"/>.
    /// </summary>
    public ItemContainer? Container { get; private set; }

    /// <summary>
    /// Gets the <see cref="NodifyEditor"/> that owns this <see cref="Container"/>.
    /// </summary>
    public NodifyEditor? Editor { get; private set; }

    /// <summary>
    /// Gets or sets the safe zone outside the editor's viewport that will not trigger optimizations.
    /// </summary>
    public static double OptimizeSafeZone = 1000d;

    /// <summary>
    /// Gets or sets the minimum selected items needed to trigger optimizations when outside of the <see cref="OptimizeSafeZone"/>.
    /// </summary>
    public static uint OptimizeMinimumSelectedItems = 100;

    /// <summary>
    /// Gets or sets if <see cref="Connector"/>s should enable optimizations based on <see cref="OptimizeSafeZone"/> and <see cref="OptimizeMinimumSelectedItems"/>.
    /// </summary>
    public static bool EnableOptimizations = false;

    /// <summary>
    /// Gets or sets whether cancelling a pending connection is allowed.
    /// </summary>
    public static bool AllowPendingConnectionCancellation { get; set; } = true;

    private Point _pendingConnectionEndPosition;

    #endregion

    static Connector()
    {
        FocusableProperty.OverrideDefaultValue<Connector>(true);
        IsConnectedProperty.Changed.AddClassHandler<Connector>((connector, e) => connector.OnIsConnectedChanged(e));
    }

    public Connector()
    {
        // Note: InputProcessor and gesture handling will be implemented when Interactivity system is ported
        AttachedToVisualTree += OnConnectorLoaded;
        DetachedFromVisualTree += OnConnectorUnloaded;
    }

    /// <inheritdoc />
    protected override void OnApplyTemplate(TemplateAppliedEventArgs e)
    {
        base.OnApplyTemplate(e);

        Container = this.FindAncestorOfType<ItemContainer>();
        Editor = this.FindAncestorOfType<NodifyEditor>();
    }

    #region Update Anchor

    private void OnConnectorLoaded(object? sender, VisualTreeAttachmentEventArgs e)
    {
        // TODO: Subscribe to ItemContainer events when fully implemented
    }

    private void OnConnectorUnloaded(object? sender, VisualTreeAttachmentEventArgs e)
    {
        // TODO: Unsubscribe from ItemContainer events when fully implemented
    }

    private void OnIsConnectedChanged(AvaloniaPropertyChangedEventArgs e)
    {
        if ((bool)e.NewValue!)
        {
            UpdateAnchor();
        }
    }

    /// <summary>
    /// Updates the <see cref="Anchor"/> based on <see cref="Container"/>'s location.
    /// </summary>
    public void UpdateAnchor()
    {
        // TODO: Implement full anchor update logic when ItemContainer is complete
        if (Thumb != null)
        {
            var thumbSize = Thumb.Bounds.Size;
            Point thumbCenter = new Point(thumbSize.Width / 2, thumbSize.Height / 2);

            // For now, use the thumb's center in its own coordinate space
            var transformedPoint = Thumb.TranslatePoint(thumbCenter, this.GetVisualParent()) ?? thumbCenter;
            Anchor = transformedPoint;
        }
    }

    #endregion

    #region Methods

    /// <summary>
    /// Initiates a new pending connection from this connector (see <see cref="IsPendingConnection"/>).
    /// </summary>
    /// <remarks>This method has no effect if a pending connection is already in progress.</remarks>
    public void BeginConnecting()
        => BeginConnecting(new Vector(0, 0));

    /// <summary>
    /// Initiates a new pending connection from this connector with the specified offset (see <see cref="IsPendingConnection"/>).
    /// </summary>
    /// <remarks>This method has no effect if a pending connection is already in progress.</remarks>
    public void BeginConnecting(Vector offset)
    {
        if (IsPendingConnection)
        {
            return;
        }

        UpdateAnchor();
        _pendingConnectionEndPosition = Anchor + offset;

        var args = new PendingConnectionEventArgs(DataContext!)
        {
            RoutedEvent = PendingConnectionStartedEvent,
            Anchor = Anchor,
            Source = this
        };

        RaiseEvent(args);
        IsPendingConnection = !args.Canceled;
    }

    /// <summary>
    /// Updates the endpoint of the pending connection by adjusting its position with the specified offset.
    /// </summary>
    /// <param name="offset">The amount to adjust the pending connection's endpoint.</param>
    public void UpdatePendingConnection(Vector offset)
        => UpdatePendingConnection(_pendingConnectionEndPosition + offset);

    /// <summary>
    /// Updates the endpoint of the pending connection to the specified position.
    /// </summary>
    /// <param name="position">The new position for the connection's endpoint.</param>
    public void UpdatePendingConnection(Point position)
    {
        Debug.Assert(IsPendingConnection);

        _pendingConnectionEndPosition = position;

        var args = new PendingConnectionEventArgs(DataContext!)
        {
            RoutedEvent = PendingConnectionDragEvent,
            OffsetX = _pendingConnectionEndPosition.X - Anchor.X,
            OffsetY = _pendingConnectionEndPosition.Y - Anchor.Y,
            Anchor = Anchor,
            Source = this
        };

        RaiseEvent(args);
    }

    /// <summary>
    /// Cancels the current pending connection without completing it if <see cref="AllowPendingConnectionCancellation"/> is true.
    /// Otherwise, it completes the pending connection by calling <see cref="EndConnecting()"/>.
    /// </summary>
    /// <remarks>This method has no effect if there's no pending connection.</remarks>
    public void CancelConnecting()
    {
        if (!AllowPendingConnectionCancellation)
        {
            EndConnecting();
            return;
        }

        if (IsPendingConnection)
        {
            var args = new PendingConnectionEventArgs(DataContext!)
            {
                RoutedEvent = PendingConnectionCompletedEvent,
                Anchor = Anchor,
                Source = this,
                Canceled = true
            };
            RaiseEvent(args);

            IsPendingConnection = false;
        }
    }

    /// <summary>
    /// Completes the current pending connection.
    /// </summary>
    /// <remarks>
    /// Attempts to identify a target connector near the connection's endpoint and completes the pending connection.
    /// If no target connector is found, the connection may be completed without a valid target.
    /// This method has no effect if there's no pending connection.
    /// </remarks>
    public void EndConnecting()
    {
        if (!IsPendingConnection)
        {
            return;
        }

        Control? elem = FindConnectionTarget(_pendingConnectionEndPosition);
        EndConnecting(elem?.DataContext);
    }

    /// <summary>
    /// Completes the current pending connection using the specified connector as the target.
    /// </summary>
    /// <param name="connector">The connector to use as the connection target.</param>
    /// <remarks>This method has no effect if there's no pending connection.</remarks>
    public void EndConnecting(Connector connector)
        => EndConnecting(connector.DataContext);

    private void EndConnecting(object? targetDataContext)
    {
        if (!IsPendingConnection)
        {
            return;
        }

        var args = new PendingConnectionEventArgs(DataContext!)
        {
            TargetConnector = targetDataContext,
            RoutedEvent = PendingConnectionCompletedEvent,
            Anchor = Anchor,
            Source = this
        };
        RaiseEvent(args);

        IsPendingConnection = false;
        _pendingConnectionEndPosition = Anchor;
    }

    /// <summary>
    /// Removes all connections associated with this connector.
    /// </summary>
    /// <remarks>This method has no effect if a pending connection is already in progress or the connector is not connected (see <see cref="IsConnected"/>).</remarks>
    public void RemoveConnections()
    {
        if (!IsConnected || IsPendingConnection)
        {
            return;
        }

        object? connector = DataContext;
        var args = new ConnectorEventArgs(connector!)
        {
            RoutedEvent = DisconnectEvent,
            Anchor = Anchor,
            Source = this
        };

        RaiseEvent(args);

        // Raise DisconnectCommand if event is not handled
        if (!args.Handled && (DisconnectCommand?.CanExecute(connector) ?? false))
        {
            DisconnectCommand.Execute(connector);
        }
    }

    /// <summary>
    /// Translates the event location to graph space coordinates (relative to the <see cref="NodifyEditor.ItemsHost" />).
    /// </summary>
    /// <param name="e">The pointer event.</param>
    /// <remarks>
    /// Call <see cref="UpdateAnchor()"/> before calling this method if the <see cref="Anchor"/> is not up-to-date.
    /// </remarks>
    internal Point GetLocationInsideEditor(PointerEventArgs e)
    {
        Point position = e.GetPosition(Thumb);
        Vector thumbOffset = position - new Point(Thumb.Bounds.Width / 2, Thumb.Bounds.Height / 2);
        return Anchor + thumbOffset;
    }

    /// <summary>
    /// Searches for a <see cref="Connector"/> at the specified position.
    /// </summary>
    /// <param name="position">The position in the editor to check for a connector.</param>
    public Connector? FindTargetConnector(Point position)
    {
        if (Editor != null)
        {
            return (Connector?)PendingConnection.GetPotentialConnector(Editor, position, true);
        }

        return null;
    }

    /// <summary>
    /// Searches for a potential <see cref="Connector"/> or <see cref="ItemContainer"/> at the specified position within the editor.
    /// </summary>
    /// <param name="position">The position in the editor to check for a potential connection target.</param>
    public Control? FindConnectionTarget(Point position)
    {
        if (Editor != null)
        {
            return PendingConnection.GetPotentialConnector(Editor, position);
        }

        return null;
    }

    #endregion
}
