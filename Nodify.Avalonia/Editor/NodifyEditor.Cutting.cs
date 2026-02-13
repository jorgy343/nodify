using Avalonia;
using Avalonia.Media;
using Avalonia.Styling;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Windows.Input;

namespace Nodify;

public partial class NodifyEditor
{
    #region Cutting Properties

    public static readonly StyledProperty<IStyle?> CuttingLineStyleProperty =
        AvaloniaProperty.Register<NodifyEditor, IStyle?>(nameof(CuttingLineStyle));

    private static readonly DirectProperty<NodifyEditor, Point> CuttingLineStartPropertyKey =
        AvaloniaProperty.RegisterDirect<NodifyEditor, Point>(
            nameof(CuttingLineStart),
            o => o._cuttingLineStart,
            (o, v) => o._cuttingLineStart = v);

    private Point _cuttingLineStart;

    private static readonly DirectProperty<NodifyEditor, Point> CuttingLineEndPropertyKey =
        AvaloniaProperty.RegisterDirect<NodifyEditor, Point>(
            nameof(CuttingLineEnd),
            o => o._cuttingLineEnd,
            (o, v) => o._cuttingLineEnd = v);

    private Point _cuttingLineEnd;

    private static readonly DirectProperty<NodifyEditor, bool> IsCuttingPropertyKey =
        AvaloniaProperty.RegisterDirect<NodifyEditor, bool>(
            nameof(IsCutting),
            o => o._isCutting,
            (o, v) => o._isCutting = v);

    private bool _isCutting;

    public static readonly StyledProperty<ICommand?> CuttingStartedCommandProperty =
        AvaloniaProperty.Register<NodifyEditor, ICommand?>(nameof(CuttingStartedCommand));

    public static readonly StyledProperty<ICommand?> CuttingCompletedCommandProperty =
        AvaloniaProperty.Register<NodifyEditor, ICommand?>(nameof(CuttingCompletedCommand));

    /// <summary>
    /// Gets or sets the style to use for the cutting line.
    /// </summary>
    public IStyle? CuttingLineStyle
    {
        get => GetValue(CuttingLineStyleProperty);
        set => SetValue(CuttingLineStyleProperty, value);
    }

    /// <summary>
    /// Gets the start point of the <see cref="CuttingLine"/> while <see cref="IsCutting"/> is true.
    /// </summary>
    public Point CuttingLineStart
    {
        get => _cuttingLineStart;
        private set => SetAndRaise(CuttingLineStartPropertyKey, ref _cuttingLineStart, value);
    }

    /// <summary>
    /// Gets the end point of the <see cref="CuttingLine"/> while <see cref="IsCutting"/> is true.
    /// </summary>
    public Point CuttingLineEnd
    {
        get => _cuttingLineEnd;
        private set => SetAndRaise(CuttingLineEndPropertyKey, ref _cuttingLineEnd, value);
    }

    /// <summary>
    /// Gets a value that indicates whether a cutting operation is in progress.
    /// </summary>
    public bool IsCutting
    {
        get => _isCutting;
        private set => SetAndRaise(IsCuttingPropertyKey, ref _isCutting, value);
    }

    /// <summary>Invoked when a cutting operation is started.</summary>
    public ICommand? CuttingStartedCommand
    {
        get => GetValue(CuttingStartedCommandProperty);
        set => SetValue(CuttingStartedCommandProperty, value);
    }

    /// <summary>Invoked when a cutting operation is completed.</summary>
    public ICommand? CuttingCompletedCommand
    {
        get => GetValue(CuttingCompletedCommandProperty);
        set => SetValue(CuttingCompletedCommandProperty, value);
    }

    #endregion

    /// <summary>
    /// Gets or sets whether cancelling a cutting operation is allowed (see <see cref="EditorGestures.NodifyEditorGestures.CancelAction"/>).
    /// </summary>
    public static bool AllowCuttingCancellation { get; set; } = true;

    /// <summary>
    /// Gets or sets whether the cutting line should apply the preview style to the intersected elements.
    /// </summary>
    /// <remarks>
    /// This may hurt performance because intersection must be calculated on mouse move.
    /// </remarks>
    public static bool EnableCuttingLinePreview { get; set; } = false;

    /// <summary>
    /// The list of supported connection types for cutting. Type must be derived from Control.
    /// </summary>
    public static readonly HashSet<Type> CuttingConnectionTypes = new HashSet<Type>();

    private List<Avalonia.Controls.Control>? _cuttingLinePreviousConnections;
    private readonly LineGeometry _cuttingLineGeometry = new LineGeometry();

    private void OnCuttingCompleted()
    {
        if (CuttingCompletedCommand?.CanExecute(DataContext) ?? false)
            CuttingCompletedCommand.Execute(DataContext);
    }

    private void OnCuttingStarted()
    {
        if (CuttingStartedCommand?.CanExecute(DataContext) ?? false)
            CuttingStartedCommand.Execute(DataContext);
    }

    /// <summary>
    /// Starts the cutting operation at the current mouse/pointer location. Call <see cref="EndCutting"/> to complete the operation or <see cref="CancelCutting"/> to abort it.
    /// </summary>
    /// <remarks>This method has no effect if a cutting operation is already in progress.</remarks>
    public void BeginCutting()
    {
        // TODO: Get actual pointer location when pointer event handling is added
        BeginCutting(new Point(0, 0));
    }

    /// <summary>
    /// Starts the cutting operation at the specified location. Call <see cref="EndCutting"/> to complete the operation or <see cref="CancelCutting"/> to abort it.
    /// </summary>
    /// <remarks>This method has no effect if a cutting operation is already in progress.</remarks>
    /// <param name="location">The starting location for cutting items, in graph space coordinates.</param>
    public void BeginCutting(Point location)
    {
        if (IsCutting)
        {
            return;
        }

        CuttingLineStart = location;
        CuttingLineEnd = location;
        IsCutting = true;

        _cuttingLineGeometry.StartPoint = location;
        _cuttingLineGeometry.EndPoint = location;
    }

    /// <summary>
    /// Updates the current cutting line position and the style for the intersecting elements if <see cref="EnableCuttingLinePreview"/> is true.
    /// </summary>
    /// <param name="amount">The amount to adjust the cutting line's endpoint.</param>
    public void UpdateCuttingLine(Vector amount)
    {
        CuttingLineEnd += amount;

        UpdateCuttingLine(CuttingLineEnd);
    }

    /// <summary>
    /// Updates the current cutting line position and the style for the intersecting elements if <see cref="EnableCuttingLinePreview"/> is true.
    /// </summary>
    /// <param name="location">The location of the cutting line's endpoint.</param>
    public void UpdateCuttingLine(Point location)
    {
        Debug.Assert(IsCutting);
        CuttingLineEnd = location;

        if (EnableCuttingLinePreview)
        {
            _cuttingLineGeometry.EndPoint = CuttingLineEnd;

            ResetConnectionStyle();
            ApplyConnectionStyle();
        }
    }

    /// <summary>
    /// Cancels the current cutting operation without applying any changes if <see cref="AllowCuttingCancellation"/> is true.
    /// Otherwise, it ends the cutting operation by calling <see cref="EndCutting"/>.
    /// </summary>
    /// <remarks>This method has no effect if there's no cutting operation in progress.</remarks>
    public void CancelCutting()
    {
        if (!AllowCuttingCancellation)
        {
            EndCutting();
            return;
        }

        if (IsCutting)
        {
            ResetConnectionStyle();
            IsCutting = false;
        }
    }

    /// <summary>
    /// Completes the cutting operation and applies the changes.
    /// </summary>
    /// <remarks>This method has no effect if there's no cutting operation in progress.</remarks>
    public void EndCutting()
    {
        if (!IsCutting)
        {
            return;
        }

        ResetConnectionStyle();

        if (ConnectionsHost != null)
        {
            var lineGeometry = new LineGeometry(CuttingLineStart, CuttingLineEnd);
            var connections = ConnectionsHost.GetIntersectingElements(lineGeometry, CuttingConnectionTypes);

            if (RemoveConnectionCommand != null)
            {
                foreach (var connection in connections)
                {
                    OnRemoveConnection(connection.DataContext);
                }
            }
            else
            {
                RemoveSupportedConnections(connections);
            }
        }

        IsCutting = false;
    }

    private static void RemoveSupportedConnections(List<Avalonia.Controls.Control> connections)
    {
        foreach (var connection in connections)
        {
            if (connection is BaseConnection bc)
            {
                // TODO: Implement BaseConnection.Remove() method
                // bc.Remove();
            }
        }
    }

    private void ApplyConnectionStyle()
    {
        if (ConnectionsHost != null)
        {
            var connections = ConnectionsHost.GetIntersectingElements(_cuttingLineGeometry, CuttingConnectionTypes);
            foreach (var connection in connections)
            {
                CuttingLine.SetIsOverElement(connection, true);
            }

            _cuttingLinePreviousConnections = connections;
        }
    }

    private void ResetConnectionStyle()
    {
        if (_cuttingLinePreviousConnections != null)
        {
            foreach (var connection in _cuttingLinePreviousConnections)
            {
                CuttingLine.SetIsOverElement(connection, false);
            }

            _cuttingLinePreviousConnections = null;
        }
    }
}
