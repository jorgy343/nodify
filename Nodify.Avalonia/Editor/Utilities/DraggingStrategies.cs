using Avalonia;
using Avalonia.Media;
using System.Collections.Generic;
using System.Linq;

namespace Nodify;

internal interface IDraggingStrategy
{
    IReadOnlyCollection<ItemContainer> Containers { get; }
    Vector Offset { get; }
    void Update(Vector change);
    void End();
    void Abort();
}

/// <summary>
/// Updates the RenderTransform to preview the container position and commits the position changes at the end of the operation.
/// </summary>
internal sealed class DraggingOptimized : IDraggingStrategy
{
    private readonly uint _gridCellSize;
    private readonly List<ItemContainer> _selectedContainers;
    private Vector _dragAccumulator = new Vector(0, 0);

    public Vector Offset { get; private set; }
    public IReadOnlyCollection<ItemContainer> Containers => _selectedContainers;

    public DraggingOptimized(IEnumerable<ItemContainer> containers, uint gridCellSize)
    {
        _gridCellSize = gridCellSize;
        _selectedContainers = containers.Where(c => c.IsDraggable).ToList();
    }

    public void Abort()
    {
        for (var i = 0; i < _selectedContainers.Count; i++)
        {
            ItemContainer container = _selectedContainers[i];
            var r = (TranslateTransform)container.RenderTransform!;

            r.X = 0;
            r.Y = 0;

            container.OnPreviewLocationChanged(container.Location);
        }

        _selectedContainers.Clear();
    }

    public void End()
    {
        for (var i = 0; i < _selectedContainers.Count; i++)
        {
            ItemContainer container = _selectedContainers[i];
            var r = (TranslateTransform)container.RenderTransform!;

            Point result = container.Location + new Vector(r.X, r.Y);

            // Correct the final position
            if (NodifyEditor.EnableSnappingCorrection && (r.X != 0 || r.Y != 0))
            {
                result = new Point(
                    (int)result.X / _gridCellSize * _gridCellSize,
                    (int)result.Y / _gridCellSize * _gridCellSize);
            }

            container.Location = result;

            r.X = 0;
            r.Y = 0;
        }

        _selectedContainers.Clear();
    }

    public void Update(Vector change)
    {
        _dragAccumulator += change;
        var delta = new Vector(
            (int)_dragAccumulator.X / _gridCellSize * _gridCellSize,
            (int)_dragAccumulator.Y / _gridCellSize * _gridCellSize);
        _dragAccumulator -= delta;

        if (delta.X != 0 || delta.Y != 0)
        {
            Offset += delta;

            for (var i = 0; i < _selectedContainers.Count; i++)
            {
                ItemContainer container = _selectedContainers[i];
                var r = (TranslateTransform)container.RenderTransform!;

                r.X += delta.X; // Snapping without correction
                r.Y += delta.Y; // Snapping without correction

                container.OnPreviewLocationChanged(container.Location + new Vector(r.X, r.Y));
            }
        }
    }
}

internal sealed class DraggingSimple : IDraggingStrategy
{
    private readonly uint _gridCellSize;
    private readonly List<ItemContainer> _selectedContainers;
    private Vector _dragAccumulator = new Vector(0, 0);

    public Vector Offset { get; private set; }
    public IReadOnlyCollection<ItemContainer> Containers => _selectedContainers;

    public DraggingSimple(IEnumerable<ItemContainer> containers, uint gridCellSize)
    {
        _gridCellSize = gridCellSize;
        _selectedContainers = containers.Where(c => c.IsDraggable).ToList();
    }

    public void Abort()
    {
        for (var i = 0; i < _selectedContainers.Count; i++)
        {
            ItemContainer container = _selectedContainers[i];
            container.Location -= Offset;
        }

        _selectedContainers.Clear();
    }

    public void End()
    {
        for (var i = 0; i < _selectedContainers.Count; i++)
        {
            ItemContainer container = _selectedContainers[i];
            Point result = container.Location;

            // Correct the final position
            if (NodifyEditor.EnableSnappingCorrection)
            {
                result = new Point(
                    (int)result.X / _gridCellSize * _gridCellSize,
                    (int)result.Y / _gridCellSize * _gridCellSize);
            }

            container.Location = result;
        }

        _selectedContainers.Clear();
    }

    public void Update(Vector change)
    {
        _dragAccumulator += change;
        var delta = new Vector(
            (int)_dragAccumulator.X / _gridCellSize * _gridCellSize,
            (int)_dragAccumulator.Y / _gridCellSize * _gridCellSize);
        _dragAccumulator -= delta;

        if (delta.X != 0 || delta.Y != 0)
        {
            Offset += delta;

            for (var i = 0; i < _selectedContainers.Count; i++)
            {
                ItemContainer container = _selectedContainers[i];
                container.Location = new Point(container.Location.X + delta.X, container.Location.Y + delta.Y);
            }
        }
    }
}
