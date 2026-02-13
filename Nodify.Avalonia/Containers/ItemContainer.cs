using Avalonia;
using Avalonia.Controls;

namespace Nodify;

/// <summary>
/// Represents a container for an item in the NodifyEditor.
/// This is a placeholder for the full ItemContainer implementation.
/// </summary>
public partial class ItemContainer : ContentControl
{
    /// <summary>
    /// Gets or sets whether this container is selectable.
    /// </summary>
    public bool IsSelectable { get; set; } = true;

    /// <summary>
    /// Gets or sets whether this container is currently selected.
    /// </summary>
    public bool IsSelected { get; set; }

    /// <summary>
    /// Gets or sets whether this container is previewing selection.
    /// </summary>
    public bool? IsPreviewingSelection { get; set; }

    /// <summary>
    /// Determines if this container is selectable within the specified area.
    /// </summary>
    public bool IsSelectableInArea(Rect area, bool fit)
    {
        // Placeholder implementation
        var bounds = Bounds;
        return fit ? area.Contains(bounds) : area.Intersects(bounds);
    }
}
