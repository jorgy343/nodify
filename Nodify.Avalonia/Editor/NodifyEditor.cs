using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Media;
using System;

namespace Nodify;

/// <summary>
/// Groups <see cref="ItemContainer"/>s and connections in an area that you can drag, zoom and select.
/// This is a modernized Avalonia port of the WPF NodifyEditor control.
/// </summary>
public partial class NodifyEditor : TemplatedControl
{
    protected const string ElementItemsHost = "PART_ItemsHost";
    protected const string ElementConnectionsHost = "PART_ConnectionsHost";

    /// <summary>
    /// Gets or sets the threshold distance (in pixels) for suppressing mouse actions like context menus during drag operations.
    /// </summary>
    public static double MouseActionSuppressionThreshold { get; set; } = 12d;

    #region Viewport Properties

    /// <summary>
    /// Defines the <see cref="ViewportZoom"/> property.
    /// </summary>
    public static readonly StyledProperty<double> ViewportZoomProperty =
        AvaloniaProperty.Register<NodifyEditor, double>(
            nameof(ViewportZoom),
            defaultValue: 1.0,
            defaultBindingMode: Avalonia.Data.BindingMode.TwoWay,
            coerce: CoerceViewportZoom);

    /// <summary>
    /// Defines the <see cref="MinViewportZoom"/> property.
    /// </summary>
    public static readonly StyledProperty<double> MinViewportZoomProperty =
        AvaloniaProperty.Register<NodifyEditor, double>(
            nameof(MinViewportZoom),
            defaultValue: 0.1,
            coerce: CoerceMinViewportZoom);

    /// <summary>
    /// Defines the <see cref="MaxViewportZoom"/> property.
    /// </summary>
    public static readonly StyledProperty<double> MaxViewportZoomProperty =
        AvaloniaProperty.Register<NodifyEditor, double>(
            nameof(MaxViewportZoom),
            defaultValue: 2.0,
            coerce: CoerceMaxViewportZoom);

    /// <summary>
    /// Defines the <see cref="ViewportLocation"/> property.
    /// </summary>
    public static readonly StyledProperty<Point> ViewportLocationProperty =
        AvaloniaProperty.Register<NodifyEditor, Point>(
            nameof(ViewportLocation),
            defaultValue: default,
            defaultBindingMode: Avalonia.Data.BindingMode.TwoWay);

    /// <summary>
    /// Defines the <see cref="ViewportSize"/> property.
    /// </summary>
    public static readonly StyledProperty<Size> ViewportSizeProperty =
        AvaloniaProperty.Register<NodifyEditor, Size>(
            nameof(ViewportSize),
            defaultValue: default);

    /// <summary>
    /// Defines the <see cref="ItemsExtent"/> property.
    /// </summary>
    public static readonly StyledProperty<Rect> ItemsExtentProperty =
        AvaloniaProperty.Register<NodifyEditor, Rect>(
            nameof(ItemsExtent),
            defaultValue: default);

    /// <summary>
    /// Defines the <see cref="DecoratorsExtent"/> property.
    /// </summary>
    public static readonly StyledProperty<Rect> DecoratorsExtentProperty =
        AvaloniaProperty.Register<NodifyEditor, Rect>(
            nameof(DecoratorsExtent),
            defaultValue: default);

    /// <summary>
    /// Defines the <see cref="ViewportTransform"/> property.
    /// </summary>
    public static readonly DirectProperty<NodifyEditor, ITransform?> ViewportTransformProperty =
        AvaloniaProperty.RegisterDirect<NodifyEditor, ITransform?>(
            nameof(ViewportTransform),
            o => o.ViewportTransform);

    private ITransform? _viewportTransform;

    #endregion

    #region Routed Events

    /// <summary>
    /// Defines the <see cref="ViewportUpdated"/> event.
    /// </summary>
    public static readonly RoutedEvent<RoutedEventArgs> ViewportUpdatedEvent =
        RoutedEvent.Register<NodifyEditor, RoutedEventArgs>(
            nameof(ViewportUpdated),
            RoutingStrategies.Bubble);

    /// <summary>
    /// Occurs whenever the viewport updates.
    /// </summary>
    public event EventHandler<RoutedEventArgs>? ViewportUpdated
    {
        add => AddHandler(ViewportUpdatedEvent, value);
        remove => RemoveHandler(ViewportUpdatedEvent, value);
    }

    #endregion

    #region Editor State Properties

    public static readonly StyledProperty<bool> HasCustomContextMenuProperty =
        AvaloniaProperty.Register<NodifyEditor, bool>(nameof(HasCustomContextMenu), defaultValue: false);

    private static readonly DirectProperty<NodifyEditor, bool> IsSelectingPropertyKey =
        AvaloniaProperty.RegisterDirect<NodifyEditor, bool>(
            nameof(IsSelecting),
            o => o.IsSelecting);

    private static readonly DirectProperty<NodifyEditor, bool> IsBulkUpdatingItemsPropertyKey =
        AvaloniaProperty.RegisterDirect<NodifyEditor, bool>(
            nameof(IsBulkUpdatingItems),
            o => o.IsBulkUpdatingItems);

    private static readonly DirectProperty<NodifyEditor, uint> SelectedContainersCountPropertyKey =
        AvaloniaProperty.RegisterDirect<NodifyEditor, uint>(
            nameof(SelectedContainersCount),
            o => o.SelectedContainersCount);

    private bool _isSelecting;
    private bool _isBulkUpdatingItems;
    private uint _selectedContainersCount;

    /// <summary>
    /// Gets or sets a value indicating whether the editor uses a custom context menu.
    /// </summary>
    public bool HasCustomContextMenu
    {
        get => GetValue(HasCustomContextMenuProperty);
        set => SetValue(HasCustomContextMenuProperty, value);
    }

    /// <summary>
    /// Gets a value indicating whether a selection operation is currently in progress.
    /// </summary>
    public bool IsSelecting
    {
        get => _isSelecting;
        internal set => SetAndRaise(IsSelectingPropertyKey, ref _isSelecting, value);
    }

    /// <summary>
    /// Gets a value indicating whether the editor is bulk updating items (batch operation in progress).
    /// </summary>
    public bool IsBulkUpdatingItems
    {
        get => _isBulkUpdatingItems;
        internal set => SetAndRaise(IsBulkUpdatingItemsPropertyKey, ref _isBulkUpdatingItems, value);
    }

    /// <summary>
    /// Gets the number of currently selected containers.
    /// </summary>
    public uint SelectedContainersCount
    {
        get => _selectedContainersCount;
        private set => SetAndRaise(SelectedContainersCountPropertyKey, ref _selectedContainersCount, value);
    }

    /// <summary>
    /// Gets the items host panel (where ItemContainers are arranged).
    /// </summary>
    public Panel? ItemsHost { get; private set; }

    #endregion

    #region Properties

    /// <summary>
    /// Gets the transform used to offset the viewport.
    /// </summary>
    protected readonly TranslateTransform TranslateTransform = new();

    /// <summary>
    /// Gets the transform used to zoom on the viewport.
    /// </summary>
    protected readonly ScaleTransform ScaleTransform = new() { ScaleX = 1.0, ScaleY = 1.0 };

    /// <summary>
    /// Gets the transform that is applied to all child controls.
    /// </summary>
    public ITransform? ViewportTransform
    {
        get => _viewportTransform;
        private set => SetAndRaise(ViewportTransformProperty, ref _viewportTransform, value);
    }

    /// <summary>
    /// Gets the size of the viewport in graph space (scaled by the <see cref="ViewportZoom"/>).
    /// </summary>
    public Size ViewportSize
    {
        get => GetValue(ViewportSizeProperty);
        set => SetValue(ViewportSizeProperty, value);
    }

    /// <summary>
    /// Gets or sets the viewport's top-left coordinates in graph space coordinates.
    /// </summary>
    public Point ViewportLocation
    {
        get => GetValue(ViewportLocationProperty);
        set => SetValue(ViewportLocationProperty, value);
    }

    /// <summary>
    /// Gets or sets the zoom factor of the viewport.
    /// </summary>
    public double ViewportZoom
    {
        get => GetValue(ViewportZoomProperty);
        set => SetValue(ViewportZoomProperty, value);
    }

    /// <summary>
    /// Gets or sets the minimum zoom factor of the viewport.
    /// </summary>
    public double MinViewportZoom
    {
        get => GetValue(MinViewportZoomProperty);
        set => SetValue(MinViewportZoomProperty, value);
    }

    /// <summary>
    /// Gets or sets the maximum zoom factor of the viewport.
    /// </summary>
    public double MaxViewportZoom
    {
        get => GetValue(MaxViewportZoomProperty);
        set => SetValue(MaxViewportZoomProperty, value);
    }

    /// <summary>
    /// Gets or sets the area covered by the <see cref="ItemContainer"/>s.
    /// </summary>
    public Rect ItemsExtent
    {
        get => GetValue(ItemsExtentProperty);
        set => SetValue(ItemsExtentProperty, value);
    }

    /// <summary>
    /// Gets or sets the area covered by the decorators.
    /// </summary>
    public Rect DecoratorsExtent
    {
        get => GetValue(DecoratorsExtentProperty);
        set => SetValue(DecoratorsExtentProperty, value);
    }

    #endregion

    #region Coercion

    private static double CoerceMinViewportZoom(AvaloniaObject sender, double value)
    {
        return value > 0.1 ? value : 0.1;
    }

    private static double CoerceMaxViewportZoom(AvaloniaObject sender, double value)
    {
        if (sender is NodifyEditor editor)
        {
            double min = editor.MinViewportZoom;
            return value < min ? min : value;
        }
        return value;
    }

    private static double CoerceViewportZoom(AvaloniaObject sender, double value)
    {
        if (sender is NodifyEditor editor)
        {
            double minimum = editor.MinViewportZoom;
            if (value < minimum)
            {
                return minimum;
            }

            double maximum = editor.MaxViewportZoom;
            return value > maximum ? maximum : value;
        }
        return value;
    }

    #endregion

    static NodifyEditor()
    {
        ViewportZoomProperty.Changed.AddClassHandler<NodifyEditor>((editor, e) => editor.OnViewportZoomChanged(e));
        ViewportLocationProperty.Changed.AddClassHandler<NodifyEditor>((editor, e) => editor.OnViewportLocationChanged(e));
        MinViewportZoomProperty.Changed.AddClassHandler<NodifyEditor>((editor, e) => editor.OnMinViewportZoomChanged(e));
        MaxViewportZoomProperty.Changed.AddClassHandler<NodifyEditor>((editor, e) => editor.OnMaxViewportZoomChanged(e));
        ItemsExtentProperty.Changed.AddClassHandler<NodifyEditor>((editor, e) => editor.OnItemsExtentChanged(e));

        // Panning property handlers
        DisablePanningProperty.Changed.AddClassHandler<NodifyEditor>((editor, e) =>
        {
            editor.OnDisableAutoPanningChanged(editor.DisableAutoPanning || editor.DisablePanning);
        });

        DisableAutoPanningProperty.Changed.AddClassHandler<NodifyEditor>((editor, e) =>
        {
            editor.OnDisableAutoPanningChanged((bool)e.NewValue!);
        });
    }

    public NodifyEditor()
    {
        // Initialize viewport transform
        var transformGroup = new TransformGroup();
        transformGroup.Children.Add(ScaleTransform);
        transformGroup.Children.Add(TranslateTransform);
        ViewportTransform = transformGroup;
    }

    /// <inheritdoc />
    protected override void OnApplyTemplate(TemplateAppliedEventArgs e)
    {
        base.OnApplyTemplate(e);

        // Get the items host panel from the template
        ItemsHost = e.NameScope.Find<Panel>(ElementItemsHost);
    }

    #region Property Changed Handlers

    private void OnItemsExtentChanged(AvaloniaPropertyChangedEventArgs e)
    {
        // Placeholder for UpdateScrollbars
    }

    private void OnViewportLocationChanged(AvaloniaPropertyChangedEventArgs e)
    {
        if (e.NewValue is Point translate)
        {
            TranslateTransform.X = -translate.X * ViewportZoom;
            TranslateTransform.Y = -translate.Y * ViewportZoom;

            OnViewportUpdated();
        }
    }

    private void OnViewportZoomChanged(AvaloniaPropertyChangedEventArgs e)
    {
        if (e.NewValue is double zoom)
        {
            ScaleTransform.ScaleX = zoom;
            ScaleTransform.ScaleY = zoom;

            ViewportSize = new Size(Bounds.Width / zoom, Bounds.Height / zoom);

            OnViewportUpdated();
        }
    }

    private void OnMinViewportZoomChanged(AvaloniaPropertyChangedEventArgs e)
    {
        CoerceValue(MaxViewportZoomProperty);
        CoerceValue(ViewportZoomProperty);
    }

    private void OnMaxViewportZoomChanged(AvaloniaPropertyChangedEventArgs e)
    {
        CoerceValue(ViewportZoomProperty);
    }

    #endregion

    /// <summary>
    /// Updates the <see cref="ViewportSize"/> and raises the <see cref="ViewportUpdatedEvent"/>.
    /// Called when the render size or <see cref="ViewportZoom"/> is changed.
    /// </summary>
    protected void OnViewportUpdated()
    {
        RaiseEvent(new RoutedEventArgs(ViewportUpdatedEvent, this));
    }

    /// <summary>
    /// Zooms in at the viewport center.
    /// </summary>
    public void ZoomIn()
    {
        double newZoom = ViewportZoom * 1.1;
        ViewportZoom = Math.Min(newZoom, MaxViewportZoom);
    }

    /// <summary>
    /// Zooms out at the viewport center.
    /// </summary>
    public void ZoomOut()
    {
        double newZoom = ViewportZoom / 1.1;
        ViewportZoom = Math.Max(newZoom, MinViewportZoom);
    }

    /// <summary>
    /// Resets the viewport to the default location and zoom.
    /// </summary>
    public void ResetViewport()
    {
        ViewportLocation = new Point(0, 0);
        ViewportZoom = 1.0;
    }

    /// <summary>
    /// Brings the specified location into view.
    /// </summary>
    public void BringIntoView(Point location)
    {
        ViewportLocation = location;
    }

    /// <summary>
    /// Fits all items to screen.
    /// </summary>
    public void FitToScreen()
    {
        if (ItemsExtent.Width > 0 && ItemsExtent.Height > 0)
        {
            double zoomX = Bounds.Width / ItemsExtent.Width;
            double zoomY = Bounds.Height / ItemsExtent.Height;
            double zoom = Math.Min(zoomX, zoomY) * 0.9; // 90% to add some padding

            ViewportZoom = Math.Clamp(zoom, MinViewportZoom, MaxViewportZoom);
            ViewportLocation = new Point(
                ItemsExtent.X + (ItemsExtent.Width - Bounds.Width / ViewportZoom) / 2,
                ItemsExtent.Y + (ItemsExtent.Height - Bounds.Height / ViewportZoom) / 2);
        }
    }
}
