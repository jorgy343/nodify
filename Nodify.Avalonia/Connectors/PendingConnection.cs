using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Controls.Shapes;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Media;
using Avalonia.VisualTree;
using Nodify.Events;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Input;

namespace Nodify;

/// <summary>
/// Specifies how hotkeys are displayed for a pending connection.
/// </summary>
public enum HotKeysDisplayMode
{
    /// <summary>
    /// No hotkeys will be displayed for the pending connection.
    /// </summary>
    None,

    /// <summary>
    /// Display hotkeys for keyboard only.
    /// </summary>
    Keyboard,

    /// <summary>
    /// Display hotkeys for both mouse and keyboard.
    /// </summary>
    All
}

/// <summary>
/// Represents a pending connection usually started by a <see cref="Connector"/> which invokes the <see cref="CompletedCommand"/> when completed.
/// </summary>
public class PendingConnection : ContentControl
{
    #region Styled Properties

    public static readonly StyledProperty<Point> SourceAnchorProperty =
        AvaloniaProperty.Register<PendingConnection, Point>(nameof(SourceAnchor), defaultValue: default(Point));

    public static readonly StyledProperty<Point> TargetAnchorProperty =
        AvaloniaProperty.Register<PendingConnection, Point>(nameof(TargetAnchor), defaultValue: default(Point));

    public static readonly StyledProperty<object?> SourceProperty =
        AvaloniaProperty.Register<PendingConnection, object?>(nameof(Source));

    public static readonly StyledProperty<object?> TargetProperty =
        AvaloniaProperty.Register<PendingConnection, object?>(nameof(Target));

    public static readonly StyledProperty<object?> PreviewTargetProperty =
        AvaloniaProperty.Register<PendingConnection, object?>(nameof(PreviewTarget));

    public static readonly StyledProperty<bool> EnablePreviewProperty =
        AvaloniaProperty.Register<PendingConnection, bool>(nameof(EnablePreview), defaultValue: false);

    public static readonly StyledProperty<double> StrokeThicknessProperty =
        Shape.StrokeThicknessProperty.AddOwner<PendingConnection>();

    public static readonly StyledProperty<IBrush?> StrokeProperty =
        Shape.StrokeProperty.AddOwner<PendingConnection>();

    public static readonly StyledProperty<bool> AllowOnlyConnectorsProperty =
        AvaloniaProperty.Register<PendingConnection, bool>(nameof(AllowOnlyConnectors), defaultValue: true);

    public static readonly StyledProperty<bool> EnableSnappingProperty =
        AvaloniaProperty.Register<PendingConnection, bool>(nameof(EnableSnapping), defaultValue: false);

    public static readonly StyledProperty<ConnectionDirection> DirectionProperty =
        BaseConnection.DirectionProperty.AddOwner<PendingConnection>();

    public static new readonly StyledProperty<bool> IsVisibleProperty =
        AvaloniaProperty.Register<PendingConnection, bool>(
            nameof(IsVisible),
            defaultValue: false,
            defaultBindingMode: Avalonia.Data.BindingMode.TwoWay);

    /// <summary>
    /// Gets or sets the starting point for the connection.
    /// </summary>
    public Point SourceAnchor
    {
        get => GetValue(SourceAnchorProperty);
        set => SetValue(SourceAnchorProperty, value);
    }

    /// <summary>
    /// Gets or sets the end point for the connection.
    /// </summary>
    public Point TargetAnchor
    {
        get => GetValue(TargetAnchorProperty);
        set => SetValue(TargetAnchorProperty, value);
    }

    /// <summary>
    /// Gets or sets the <see cref="Connector"/>'s <see cref="StyledElement.DataContext"/> that started this pending connection.
    /// </summary>
    public object? Source
    {
        get => GetValue(SourceProperty);
        set => SetValue(SourceProperty, value);
    }

    /// <summary>
    /// Gets or sets the <see cref="Connector"/>'s <see cref="StyledElement.DataContext"/> (or potentially an <see cref="ItemContainer"/>'s <see cref="StyledElement.DataContext"/> if <see cref="AllowOnlyConnectors"/> is false) that the <see cref="Source"/> can connect to.
    /// Only set when the connection is completed (see <see cref="CompletedCommand"/>).
    /// </summary>
    public object? Target
    {
        get => GetValue(TargetProperty);
        set => SetValue(TargetProperty, value);
    }

    /// <summary>
    /// <see cref="PreviewTarget"/> will be updated with a potential <see cref="Connector"/>'s <see cref="StyledElement.DataContext"/> if this is true.
    /// </summary>
    /// <remarks>Requires <see cref="EnableHitTesting"/> to be true.</remarks>
    public bool EnablePreview
    {
        get => GetValue(EnablePreviewProperty);
        set => SetValue(EnablePreviewProperty, value);
    }

    /// <summary>
    /// Gets or sets the <see cref="Connector"/> or the <see cref="ItemContainer"/> (if <see cref="AllowOnlyConnectors"/> is false) that we're previewing. See <see cref="EnablePreview"/>.
    /// </summary>
    public object? PreviewTarget
    {
        get => GetValue(PreviewTargetProperty);
        set => SetValue(PreviewTargetProperty, value);
    }

    /// <summary>
    /// Enables snapping the <see cref="TargetAnchor"/> to a possible <see cref="Target"/> connector.
    /// </summary>
    /// <remarks>Requires <see cref="EnableHitTesting"/> to be true.</remarks>
    public bool EnableSnapping
    {
        get => GetValue(EnableSnappingProperty);
        set => SetValue(EnableSnappingProperty, value);
    }

    /// <summary>
    /// If true will preview and connect only to <see cref="Connector"/>s, otherwise will also enable <see cref="ItemContainer"/>s.
    /// </summary>
    public bool AllowOnlyConnectors
    {
        get => GetValue(AllowOnlyConnectorsProperty);
        set => SetValue(AllowOnlyConnectorsProperty, value);
    }

    /// <summary>
    /// Gets or set the connection thickness.
    /// </summary>
    public double StrokeThickness
    {
        get => GetValue(StrokeThicknessProperty);
        set => SetValue(StrokeThicknessProperty, value);
    }

    /// <summary>
    /// Gets or sets the stroke color of the connection.
    /// </summary>
    public IBrush? Stroke
    {
        get => GetValue(StrokeProperty);
        set => SetValue(StrokeProperty, value);
    }

    /// <summary>
    /// Gets or sets the visibility of the connection.
    /// </summary>
    public new bool IsVisible
    {
        get => GetValue(IsVisibleProperty);
        set => SetValue(IsVisibleProperty, value);
    }

    /// <summary>
    /// Gets or sets the direction of this connection.
    /// </summary>
    public ConnectionDirection Direction
    {
        get => GetValue(DirectionProperty);
        set => SetValue(DirectionProperty, value);
    }

    #endregion

    #region Attached Properties

    private static readonly AttachedProperty<bool> AllowOnlyConnectorsAttachedProperty =
        AvaloniaProperty.RegisterAttached<PendingConnection, Control, bool>(
            "AllowOnlyConnectorsAttached",
            defaultValue: true);

    /// <summary>
    /// Will be set for <see cref="Connector"/>s and <see cref="ItemContainer"/>s when the pending connection is over the element if <see cref="EnablePreview"/> or <see cref="EnableSnapping"/> is true.
    /// </summary>
    public static readonly AttachedProperty<bool> IsOverElementProperty =
        AvaloniaProperty.RegisterAttached<PendingConnection, Control, bool>(
            "IsOverElement",
            defaultValue: false);

    internal static bool GetAllowOnlyConnectorsAttached(Control elem)
        => elem.GetValue(AllowOnlyConnectorsAttachedProperty);

    internal static void SetAllowOnlyConnectorsAttached(Control elem, bool value)
        => elem.SetValue(AllowOnlyConnectorsAttachedProperty, value);

    public static bool GetIsOverElement(Control elem)
        => elem.GetValue(IsOverElementProperty);

    public static void SetIsOverElement(Control elem, bool value)
        => elem.SetValue(IsOverElementProperty, value);

    #endregion

    #region Commands

    public static readonly StyledProperty<ICommand?> StartedCommandProperty =
        AvaloniaProperty.Register<PendingConnection, ICommand?>(nameof(StartedCommand));

    public static readonly StyledProperty<ICommand?> CompletedCommandProperty =
        AvaloniaProperty.Register<PendingConnection, ICommand?>(nameof(CompletedCommand));

    /// <summary>
    /// Gets or sets the command to invoke when the pending connection is started.
    /// Will not be invoked if <see cref="NodifyEditor.ConnectionStartedCommand"/> is used.
    /// <see cref="Source"/> will be set to the <see cref="Connector"/>'s <see cref="StyledElement.DataContext"/> that started this connection and will also be the command's parameter.
    /// </summary>
    public ICommand? StartedCommand
    {
        get => GetValue(StartedCommandProperty);
        set => SetValue(StartedCommandProperty, value);
    }

    /// <summary>
    /// Gets or sets the command to invoke when the pending connection is completed.
    /// Will not be invoked if <see cref="NodifyEditor.ConnectionCompletedCommand"/> is used.
    /// <see cref="Target"/> will be set to the desired <see cref="Connector"/>'s <see cref="StyledElement.DataContext"/> and will also be the command's parameter.
    /// </summary>
    public ICommand? CompletedCommand
    {
        get => GetValue(CompletedCommandProperty);
        set => SetValue(CompletedCommandProperty, value);
    }

    #endregion

    #region Fields

    /// <summary>
    /// Gets or sets whether hit testing is enabled for pending connections.
    /// </summary>
    /// <remarks>
    /// - When enabled, the <see cref="IsOverElementProperty"/> is updated on connectors during the drag operation. <br />
    /// - When disabled, the <see cref="EnablePreview"/> and <see cref="EnableSnapping"/> properties will have no effect. <br />
    /// - Disable hit testing to improve performance.
    /// </remarks>
    public static bool EnableHitTesting { get; set; } = true;

    /// <summary>
    /// Gets or sets the maximum number of hotkeys that can be displayed for a pending connection.
    /// </summary>
    /// <remarks>The maximum value can be 9.</remarks>
    public static uint MaxHotKeys { get; set; } = 9;

    /// <summary>
    /// Gets or sets whether hotkeys are enabled for pending connections.
    /// </summary>
    public static HotKeysDisplayMode HotKeysDisplayMode { get; set; } = HotKeysDisplayMode.Keyboard;

    /// <summary>
    /// Gets the <see cref="NodifyEditor"/> that owns this <see cref="PendingConnection"/>.
    /// </summary>
    protected NodifyEditor? Editor { get; private set; }

    private Control? _connectionTarget;

    #endregion

    static PendingConnection()
    {
        IsHitTestVisibleProperty.OverrideDefaultValue<PendingConnection>(false);
        IsEnabledProperty.OverrideDefaultValue<PendingConnection>(false);

        IsVisibleProperty.Changed.AddClassHandler<PendingConnection>((pc, e) => pc.OnIsVisibleChanged(e));
        AllowOnlyConnectorsProperty.Changed.AddClassHandler<PendingConnection>((pc, e) => pc.OnAllowOnlyConnectorsChanged(e));
    }

    private void OnIsVisibleChanged(AvaloniaPropertyChangedEventArgs e)
    {
        var isVisible = (bool)e.NewValue!;
        this.IsVisible = isVisible;
    }

    private void OnAllowOnlyConnectorsChanged(AvaloniaPropertyChangedEventArgs e)
    {
        if (Editor != null)
        {
            SetAllowOnlyConnectorsAttached(Editor, (bool)e.NewValue!);
        }
    }

    /// <inheritdoc />
    protected override void OnApplyTemplate(TemplateAppliedEventArgs e)
    {
        base.OnApplyTemplate(e);

        if (Editor != null)
        {
            Editor.RemoveHandler(Connector.PendingConnectionStartedEvent, OnPendingConnectionStarted);
            Editor.RemoveHandler(Connector.PendingConnectionDragEvent, OnPendingConnectionDrag);
            Editor.RemoveHandler(Connector.PendingConnectionCompletedEvent, OnPendingConnectionCompleted);
        }

        Editor = this.FindAncestorOfType<NodifyEditor>();

        if (Editor != null)
        {
            Editor.AddHandler(Connector.PendingConnectionStartedEvent, OnPendingConnectionStarted);
            Editor.AddHandler(Connector.PendingConnectionDragEvent, OnPendingConnectionDrag);
            Editor.AddHandler(Connector.PendingConnectionCompletedEvent, OnPendingConnectionCompleted);

            SetAllowOnlyConnectorsAttached(Editor, AllowOnlyConnectors);
        }
    }

    #region Event Handlers

    protected virtual void OnPendingConnectionStarted(object? sender, PendingConnectionEventArgs e)
    {
        if (!e.Handled && !e.Canceled)
        {
            e.Handled = true;
            e.Canceled = !StartedCommand?.CanExecute(e.SourceConnector) ?? false;

            Target = null;
            IsVisible = !e.Canceled;
            SourceAnchor = e.Anchor;
            TargetAnchor = new Point(e.Anchor.X + e.OffsetX, e.Anchor.Y + e.OffsetY);
            Source = e.SourceConnector;

            if (!e.Canceled)
            {
                StartedCommand?.Execute(Source);
            }

            if (EnablePreview)
            {
                PreviewTarget = e.SourceConnector;
            }
        }
    }

    protected virtual void OnPendingConnectionDrag(object? sender, PendingConnectionEventArgs e)
    {
        if (!e.Handled && IsVisible)
        {
            e.Handled = true;
            TargetAnchor = new Point(e.Anchor.X + e.OffsetX, e.Anchor.Y + e.OffsetY);

            if (!EnableHitTesting)
            {
                return;
            }

            // Look for a potential connector
            Control? target = FindConnectionTarget(TargetAnchor);

            // Update the connector's anchor and snap to it, if snapping is enabled
            if (EnableSnapping && target is Connector connector)
            {
                connector.UpdateAnchor();
                TargetAnchor = connector.Anchor;
            }

            SetConnectionTarget(target);

            // Update the preview target if enabled
            if (EnablePreview)
            {
                PreviewTarget = target?.DataContext;
            }
        }
    }

    protected virtual void OnPendingConnectionCompleted(object? sender, PendingConnectionEventArgs e)
    {
        if (!e.Handled && IsVisible)
        {
            e.Handled = true;
            IsVisible = false;

            SetConnectionTarget(null);

            if (!e.Canceled)
            {
                Target = e.TargetConnector;

                // Invoke the CompletedCommand if event is not handled
                if (CompletedCommand?.CanExecute(Target) ?? false)
                {
                    CompletedCommand?.Execute(Target);
                }
            }

            if (EnablePreview)
            {
                PreviewTarget = null;
            }
        }
    }

    /// <summary>
    /// Sets the connection target and updates the visual state of the target element.
    /// </summary>
    private void SetConnectionTarget(Control? target)
    {
        if (target == _connectionTarget)
        {
            return;
        }

        if (_connectionTarget != null)
        {
            SetIsOverElement(_connectionTarget, false);
        }

        if (target != null)
        {
            SetIsOverElement(target, true);
        }

        _connectionTarget = target;
    }

    /// <summary>
    /// Searches for a potential <see cref="Connector"/> or <see cref="ItemContainer"/> at the specified position within the editor.
    /// </summary>
    public Control? FindConnectionTarget(Point position)
    {
        if (Editor != null)
        {
            return GetPotentialConnector(Editor, position, AllowOnlyConnectors);
        }

        return null;
    }

    #endregion

    #region Helpers

    /// <summary>
    /// Searches for a potential <see cref="Connector"/> or <see cref="ItemContainer"/> at the specified position within the editor.
    /// </summary>
    /// <param name="editor">The <see cref="NodifyEditor"/> to scan for connectors or item containers.</param>
    /// <param name="position">The position in the editor to check for intersections.</param>
    /// <param name="allowOnlyConnectors">
    /// If true, only <see cref="Connector"/>s are considered; otherwise, the method will also check for <see cref="ItemContainer"/>s.
    /// </param>
    /// <returns>
    /// Returns one of the following, depending on what is found at the specified position:
    /// <br /> - A <see cref="Connector"/> if one is present.
    /// <br /> - An <see cref="ItemContainer"/> if <paramref name="allowOnlyConnectors"/> is false and a <see cref="Connector"/> is not found.
    /// <br /> - The provided <see cref="NodifyEditor"/> itself if neither a <see cref="Connector"/> nor an <see cref="ItemContainer" /> is found, and <paramref name="allowOnlyConnectors"/> is true.
    /// <br /> - Null if no valid element is identified at the specified position.
    /// </returns>
    internal static Control? GetPotentialConnector(NodifyEditor editor, Point position, bool allowOnlyConnectors)
    {
        // TODO: Implement full hit testing when NodifyEditor.ItemsHost is exposed as a property
        // For now, return null as a placeholder
        return null;

        // Original implementation (commented out until ItemsHost is available):
        // Connector? connector = editor.ItemsHost?.GetElementAtPosition<Connector>(position);
        // if (connector != null && connector.Editor == editor)
        //     return connector;
        //
        // if (allowOnlyConnectors)
        //     return null;
        //
        // var itemContainer = editor.ItemsHost?.GetElementAtPosition<ItemContainer>(position);
        // if (itemContainer != null && itemContainer.Editor == editor)
        //     return itemContainer;
        //
        // return editor;
    }

    /// <summary>
    /// Searches for a potential <see cref="Connector"/> or <see cref="ItemContainer"/> at the specified position,
    /// automatically determining whether to prioritize connectors based on editor settings.
    /// </summary>
    /// <param name="editor">The <see cref="NodifyEditor"/> to scan.</param>
    /// <param name="position">The position in the editor to check for intersections.</param>
    /// <returns>
    /// Returns a <see cref="Connector"/>, an <see cref="ItemContainer"/>, the <see cref="NodifyEditor"/>, or null.
    /// </returns>
    internal static Control? GetPotentialConnector(NodifyEditor editor, Point position)
        => GetPotentialConnector(editor, position, GetAllowOnlyConnectorsAttached(editor));

    #endregion
}
