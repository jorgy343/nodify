namespace Nodify.Interactivity;

/// <inheritdoc cref="MultiGesture.Match.Any" />
public sealed class AnyGesture : MultiGesture
{
    public AnyGesture(params Gesture[] gestures) : base(Match.Any, gestures)
    {
    }
}
