using Avalonia.Controls;
using Avalonia.Input;
using System;

namespace Nodify.Interactivity;

internal class StatefulFocusNavigator<TElement>
    where TElement : Control, IKeyboardFocusTarget<TElement>
{
    public delegate bool FindNextFocusTargetDelegate(TElement? currentElement, TraversalRequest request, out TElement? elementToFocus);

    private readonly WeakReference<TElement?> _previousFocusedElement = new(null);
    private readonly WeakReference<TElement?> _lastFocusedElement = new(null);
    private FocusNavigationDirection? _previousFocusNavigationDirection;

    public TElement? LastFocusedElement
    {
        get
        {
            _lastFocusedElement.TryGetTarget(out TElement? target);
            return target;
        }
    }

    public bool TryMoveFocus(TraversalRequest request, FindNextFocusTargetDelegate findNext)
    {
        var focusedElement = FocusManager.Instance?.GetFocusedElement() as TElement;

        // Check if the focus direction changed to opposite - and restore focus to previous element
        if (focusedElement is not null && _previousFocusNavigationDirection is not null && request.FocusNavigationDirection.IsOppositeOf(_previousFocusNavigationDirection.Value))
        {
            _previousFocusedElement.TryGetTarget(out TElement? previousElement);

            if (previousElement is not null)
            {
                previousElement.Focus();
                _previousFocusNavigationDirection = request.FocusNavigationDirection;
                return true;
            }
        }

        if (findNext(focusedElement, request, out TElement? elementToFocus))
        {
            if (elementToFocus is not null)
            {
                _previousFocusedElement.SetTarget(focusedElement);
                _lastFocusedElement.SetTarget(elementToFocus);
                _previousFocusNavigationDirection = request.FocusNavigationDirection;

                elementToFocus.Focus();
                return true;
            }
        }

        return false;
    }

    public bool TryRestoreFocus()
    {
        _lastFocusedElement.TryGetTarget(out TElement? target);

        if (target is not null)
        {
            target.Focus();
            return true;
        }

        return false;
    }

    public void UpdateLastFocusedElement(TElement element)
    {
        _lastFocusedElement.SetTarget(element);
    }
}
