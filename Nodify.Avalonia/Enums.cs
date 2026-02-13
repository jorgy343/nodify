namespace Nodify;

/// <summary>
/// Specifies the possible alignment values used by the NodifyEditor.
/// </summary>
public enum Alignment
{
    Top,
    Left,
    Bottom,
    Right,
    Middle,
    Center
}

/// <summary>
/// The type of selection operation being performed.
/// </summary>
public enum SelectionType
{
    Replace,
    Remove,
    Append,
    Invert
}

/// <summary>
/// Specifies the offset type that can be applied to a connection.
/// </summary>
public enum ConnectionOffsetMode
{
    /// <summary>
    /// No offset applied.
    /// </summary>
    None,

    /// <summary>
    /// The offset is applied in a circle around the point.
    /// </summary>
    Circle,

    /// <summary>
    /// The offset is applied in a rectangle shape around the point.
    /// </summary>
    Rectangle,

    /// <summary>
    /// The offset is applied in a rectangle shape around the point, perpendicular to the edges.
    /// </summary>
    Edge,

    /// <summary>
    /// The offset is applied as a fixed margin.
    /// </summary>
    Static
}

/// <summary>
/// The direction in which a connection is oriented.
/// </summary>
public enum ConnectionDirection
{
    /// <summary>
    /// From Source to Target.
    /// </summary>
    Forward,

    /// <summary>
    /// From Target to Source.
    /// </summary>
    Backward
}

/// <summary>
/// The end at which the arrow head is drawn.
/// </summary>
public enum ArrowHeadEnds
{
    /// <summary>
    /// Arrow head at start.
    /// </summary>
    Start,

    /// <summary>
    /// Arrow head at end.
    /// </summary>
    End,

    /// <summary>
    /// Arrow heads at both ends.
    /// </summary>
    Both,

    /// <summary>
    /// No arrow head.
    /// </summary>
    None
}

/// <summary>
/// The shape of the arrowhead.
/// </summary>
public enum ArrowHeadShape
{
    /// <summary>
    /// The default arrowhead.
    /// </summary>
    Arrowhead,

    /// <summary>
    /// An ellipse.
    /// </summary>
    Ellipse,

    /// <summary>
    /// A rectangle.
    /// </summary>
    Rectangle
}
