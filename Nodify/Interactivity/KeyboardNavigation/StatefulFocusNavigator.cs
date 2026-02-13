using System;
using Avalonia.Controls;
using Avalonia.Input;

namespace Nodify.Interactivity
{
    internal class StatefulFocusNavigator<TElement>
        where TElement : Control, IKeyboardFocusTarget<TElement>
    {
        public delegate bool FindNextFocusTargetDelegate(TElement? currentElement, TraversalRequest request, out TElement? elementToFocus);

        private readonly WeakReference<TElement?> _previousFocusedElement = new WeakReference<TElement?>(null);
        private readonly WeakReference<TElement?> _lastFocusedElement = new WeakReference<TElement?>(null);
        private NavigationDirection? _previousFocusNavigationDirection;

        private readonly Action<IKeyboardFocusTarget<TElement>> _onFocus;

        public TElement? LastFocusedElement => _lastFocusedElement.TryGetTarget(out var target) ? target : null;

        public StatefulFocusNavigator(Action<IKeyboardFocusTarget<TElement>> onFocus)
        {
            _onFocus = onFocus;
        }

        public bool TryMoveFocus(TraversalRequest request, FindNextFocusTargetDelegate findNext)
        {
            // In Avalonia, we don't have a global Keyboard.FocusedElement.
            // The currently focused element within the navigation context is tracked by the caller.
            TElement? currentTarget = null;

            // If the request is in the opposite direction of the last focus navigation, try to restore the previous focused container
            if (_previousFocusedElement.TryGetTarget(out var prevTarget)
                && _previousFocusNavigationDirection.HasValue
                && request.FocusNavigationDirection.IsOppositeOf(_previousFocusNavigationDirection.Value)
                && prevTarget!.Focus())
            {
                _previousFocusNavigationDirection = request.FocusNavigationDirection;
                _previousFocusedElement.SetTarget(currentTarget);
                _lastFocusedElement.SetTarget(prevTarget);

                _onFocus(prevTarget);
                return true;
            }
            else if (findNext(currentTarget, request, out var nextTarget) && nextTarget!.Element.Focus())
            {
                _previousFocusNavigationDirection = request.FocusNavigationDirection;
                _previousFocusedElement.SetTarget(currentTarget);
                _lastFocusedElement.SetTarget(nextTarget);

                _onFocus(nextTarget);
                return true;
            }

            return false;
        }

        public bool TryRestoreFocus()
        {
            if (_lastFocusedElement.TryGetTarget(out var lastTarget))
            {
                if (lastTarget!.IsFocused)
                {
                    return true;
                }

                if (lastTarget.Focus())
                {
                    _onFocus.Invoke(lastTarget);
                    return true;
                }
            }

            return false;
        }
    }
}
