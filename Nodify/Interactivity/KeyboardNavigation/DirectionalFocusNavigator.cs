using System;
using System.Collections.Generic;
using System.Linq;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;

namespace Nodify.Interactivity
{
    internal readonly struct DirectionalFocusNavigator<TElement>
        where TElement : Control, IKeyboardFocusTarget<TElement>
    {
        private readonly IEnumerable<IKeyboardFocusTarget<TElement>> _availableTargets;

        public DirectionalFocusNavigator(IEnumerable<IKeyboardFocusTarget<TElement>> availableTargets)
        {
            _availableTargets = availableTargets;
        }

        public readonly IKeyboardFocusTarget<TElement>? FindNextFocusTarget(IKeyboardFocusTarget<TElement> currentContainer, TraversalRequest request)
        {
            var currentContainerBounds = currentContainer.Bounds;

            IEnumerable<IKeyboardFocusTarget<TElement>> candidates = request.FocusNavigationDirection switch
            {
                NavigationDirection.Left => _availableTargets.Where(c => c.Bounds.Left < currentContainerBounds.Left),
                NavigationDirection.Right => _availableTargets.Where(c => c.Bounds.Left > currentContainerBounds.Left),
                NavigationDirection.Up => _availableTargets.Where(c => c.Bounds.Top < currentContainerBounds.Top),
                NavigationDirection.Down => _availableTargets.Where(c => c.Bounds.Top > currentContainerBounds.Top),
                NavigationDirection.Previous => FindCandidatesLinearly(currentContainer, request),
                NavigationDirection.Next => FindCandidatesLinearly(currentContainer, request),
                NavigationDirection.First => FindCandidatesLinearly(currentContainer, request),
                NavigationDirection.Last => FindCandidatesLinearly(currentContainer, request),
                _ => Array.Empty<IKeyboardFocusTarget<TElement>>()
            };

            // Wrap focus if no candidates found in the current direction
            if (!candidates.Any())
            {
                candidates = request.FocusNavigationDirection switch
                {
                    NavigationDirection.Left => _availableTargets.OrderByDescending(c => c.Bounds.Left).Take(1),
                    NavigationDirection.Right => _availableTargets.OrderBy(c => c.Bounds.Left).Take(1),
                    NavigationDirection.Up => _availableTargets.OrderByDescending(c => c.Bounds.Top).Take(1),
                    NavigationDirection.Down => _availableTargets.OrderBy(c => c.Bounds.Top).Take(1),
                    _ => Array.Empty<IKeyboardFocusTarget<TElement>>()
                };

                request.Wrapped = true;
            }

            IKeyboardFocusTarget<TElement>? best = null;
            double minDistanceSquared = double.MaxValue;

            foreach (var candidate in candidates)
            {
                var v = candidate.Bounds.TopLeft - currentContainerBounds.TopLeft;
                double distanceSquared = v.X * v.X + v.Y * v.Y;
                if (distanceSquared < minDistanceSquared)
                {
                    minDistanceSquared = distanceSquared;
                    best = candidate;
                }
            }

            return best;
        }

        private IKeyboardFocusTarget<TElement>[] FindCandidatesLinearly(IKeyboardFocusTarget<TElement> currentContainer, TraversalRequest request)
        {
            var nextTarget = new LinearFocusNavigator<TElement>(_availableTargets).FindNextFocusTarget(currentContainer, request);
            return nextTarget is null ? Array.Empty<IKeyboardFocusTarget<TElement>>() : new[] { nextTarget };
        }
    }
}
