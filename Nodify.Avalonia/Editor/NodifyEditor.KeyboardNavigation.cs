using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using Nodify.Interactivity;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace Nodify;

public partial class NodifyEditor : IKeyboardNavigationLayer, IKeyboardNavigationLayerGroup
{
    /// <summary>
    /// Gets or sets the default viewport edge offset applied when bringing an item into view as a result of keyboard focus.
    /// </summary>
    public static double BringIntoViewEdgeOffset { get; set; } = 32d;

    /// <summary>
    /// Automatically focus the first container when the navigation layer changes or the editor gets focused.
    /// </summary>
    public static bool AutoFocusFirstElement { get; set; } = true;

    /// <summary>
    /// Automatically pan the viewport when a node is focused via keyboard navigation.
    /// </summary>
    public static bool AutoPanOnNodeFocus { get; set; } = true;

    /// <summary>
    /// Automatically registers the decorators layer for keyboard navigation.
    /// </summary>
    public static bool AutoRegisterDecoratorsLayer { get; set; }

    /// <summary>
    /// Automatically registers the connectors layer for keyboard navigation.
    /// </summary>
    public static bool AutoRegisterConnectionsLayer { get; set; } = true;

    /// <summary>
    /// Indicates whether the viewport should automatically pan to follow elements moved via keyboard dragging.
    /// </summary>
    public static bool PanViewportOnKeyboardDrag { get; set; } = true;

    /// <summary>
    /// Defines the minimum distance to move or navigate when using directional input (such as arrow keys), scaled by the <see cref="ViewportZoom"/>.
    /// If the <see cref="GridCellSize"/> is smaller than this value, the movement step is increased to the nearest greater multiple of the <see cref="GridCellSize"/>.
    /// </summary>
    public static double MinimumNavigationStepSize { get; set; } = 10d;

    public IKeyboardNavigationLayer? ActiveNavigationLayer => _activeKeyboardNavigationLayer;
    public IKeyboardNavigationLayer KeyboardNavigationLayer => this;

    KeyboardNavigationLayerId IKeyboardNavigationLayer.Id => KeyboardNavigationLayerId.Nodes;
    IKeyboardFocusTarget<Control>? IKeyboardNavigationLayer.LastFocusedElement => _focusNavigator.LastFocusedElement;

    int IReadOnlyCollection<IKeyboardNavigationLayer>.Count => _navigationLayers.Count;

    private readonly List<IKeyboardNavigationLayer> _navigationLayers = new List<IKeyboardNavigationLayer>();
    private IKeyboardNavigationLayer? _activeKeyboardNavigationLayer;

    #region Focus Handling

    private readonly StatefulFocusNavigator<ItemContainer> _focusNavigator = new();

    public event Action<KeyboardNavigationLayerId>? ActiveNavigationLayerChanged;

    bool IKeyboardNavigationLayer.TryMoveFocus(TraversalRequest request)
    {
        return _focusNavigator.TryMoveFocus(request, TryFindContainerToFocus);
    }

    bool IKeyboardNavigationLayer.TryRestoreFocus()
    {
        return _focusNavigator.TryRestoreFocus();
    }

    private bool TryFindContainerToFocus(ItemContainer? currentElement, TraversalRequest request, out ItemContainer? containerToFocus)
    {
        containerToFocus = null;

        if (currentElement is ItemContainer focusedContainer)
        {
            containerToFocus = FindNextFocusTarget(focusedContainer, request);
        }
        // The current element is not a nested editor, but a focusable element inside an ItemContainer
        else if (currentElement is Control elem && elem != this && elem.FindAncestorOfType<ItemContainer>() is ItemContainer parentContainer)
        {
            containerToFocus = parentContainer;
        }
        else if (Items.Count > 0)
        {
            var viewport = new Rect(ViewportLocation, ViewportSize);
            var containers = ItemContainers;
            containerToFocus = containers.FirstOrDefault(container => container?.IsSelectableInArea(viewport, isContained: false) is true)
                ?? containers.First();
        }

        return containerToFocus != null;
    }

    protected virtual ItemContainer? FindNextFocusTarget(ItemContainer currentContainer, TraversalRequest request)
    {
        var focusNavigator = new DirectionalFocusNavigator<ItemContainer>(ItemContainers);
        var result = focusNavigator.FindNextFocusTarget(currentContainer, request);

        return result?.Element;
    }

    protected virtual void OnElementFocused(IKeyboardFocusTarget<ItemContainer> target)
    {
        if (AutoPanOnNodeFocus)
        {
            BringIntoView(target.Bounds, BringIntoViewEdgeOffset);
        }
    }

    public bool MoveFocus(FocusNavigationDirection direction)
        => MoveFocus(new TraversalRequest(direction));

    public bool MoveFocus(TraversalRequest request)
        => ActiveNavigationLayer?.TryMoveFocus(request) ?? false;

    void IKeyboardNavigationLayer.OnActivated()
    {
        KeyboardNavigationLayer.TryRestoreFocus();
    }

    void IKeyboardNavigationLayer.OnDeactivated()
    {
    }

    protected override void OnLostFocus(RoutedEventArgs? e)
    {
        base.OnLostFocus(e);

        // In Avalonia, we check if focus is moving outside the editor
        if (e is { } args)
        {
            var oldFocus = FocusManager.Instance?.GetFocusedElement();
            if (oldFocus is Control oldControl && !IsNavigationTrigger(oldControl) && this.IsVisualAncestorOf(oldControl))
            {
                // Check if new focus is outside this editor
                var topLevel = TopLevel.GetTopLevel(this);
                var newFocus = FocusManager.Instance?.GetFocusedElement();

                if (newFocus is Control newControl && !this.IsVisualAncestorOf(newControl))
                {
                    var container = oldControl.FindAncestorOfType<Control>(IsNavigationTrigger);
                    if (container is { } elem && elem.Focus())
                    {
                        e.Handled = true;
                    }
                    else
                    {
                        e.Handled = this.Focus();
                    }
                }
            }
        }
    }

    protected override void OnGotFocus(GotFocusEventArgs e)
    {
        base.OnGotFocus(e);

        if (ActiveNavigationLayer != null)
        {
            // Check if focus is coming from outside the editor
            var keyboardDevice = e.KeyboardDevice;
            bool isFocusComingFromOutside = e.Source is Control source && !this.IsVisualAncestorOf(source);

            if (isFocusComingFromOutside && ActiveNavigationLayer.TryRestoreFocus())
            {
                e.Handled = true;
            }
            else if (ActiveNavigationLayer.LastFocusedElement is null && e.Source == this && AutoFocusFirstElement)
            {
                e.Handled = ActiveNavigationLayer.TryMoveFocus(new TraversalRequest(FocusNavigationDirection.Next));
            }
        }
    }

    protected internal virtual bool IsNavigationTrigger(Control? control)
    {
        return control is NodifyEditor || control is ItemContainer || control is ConnectionContainer || control is DecoratorContainer;
    }

    #endregion

    #region Layer Management

    protected virtual void OnKeyboardNavigationLayerActivated(IKeyboardNavigationLayer activeLayer)
    {
        if (AutoFocusFirstElement && !activeLayer!.TryRestoreFocus() && HandleNestedEditor())
        {
            activeLayer.TryMoveFocus(new TraversalRequest(FocusNavigationDirection.Next));
        }

        bool HandleNestedEditor()
        {
            var parentEditor = this.FindAncestorOfType<NodifyEditor>();
            return parentEditor is null || parentEditor.IsKeyboardFocusWithin;
        }
    }

    public bool RegisterNavigationLayer(IKeyboardNavigationLayer layer)
    {
        if (_navigationLayers.Any(l => l.Id == layer.Id))
        {
            return false;
        }

        _navigationLayers.Add(layer);

        Debug.WriteLine($"Registered {layer} as a keyboard navigation layer in {this}");

        return true;
    }

    public bool RemoveNavigationLayer(KeyboardNavigationLayerId layerId)
    {
        var layerToRemove = _navigationLayers.FirstOrDefault(layer => layer.Id == layerId);
        if (layerToRemove != null && _navigationLayers.Remove(layerToRemove))
        {
            ActivatePreviousNavigationLayer();

            return true;
        }

        return false;
    }

    public bool ActivateNavigationLayer(KeyboardNavigationLayerId layerId)
    {
        var newLayer = _navigationLayers.FirstOrDefault(x => x.Id == layerId);
        if (newLayer != null)
        {
            var prevLayer = _activeKeyboardNavigationLayer;
            _activeKeyboardNavigationLayer = newLayer;
            prevLayer?.OnDeactivated();
            newLayer.OnActivated();
            OnKeyboardNavigationLayerActivated(newLayer);
            Debug.WriteLine($"Activated {_activeKeyboardNavigationLayer} as a keyboard navigation layer in {this}");
            ActiveNavigationLayerChanged?.Invoke(layerId);
            return true;
        }

        return false;
    }

    public bool ActivateNextNavigationLayer()
    {
        if (_navigationLayers.Count > 0)
        {
            Debug.Assert(ActiveNavigationLayer != null);

            int currentIndex = _navigationLayers.IndexOf(ActiveNavigationLayer!);
            int nextIndex = (currentIndex + 1) % _navigationLayers.Count;

            var layer = _navigationLayers[nextIndex];
            return ActivateNavigationLayer(layer.Id);
        }

        return false;
    }

    public bool ActivatePreviousNavigationLayer()
    {
        if (_navigationLayers.Count > 0)
        {
            Debug.Assert(ActiveNavigationLayer != null);

            int currentIndex = _navigationLayers.IndexOf(ActiveNavigationLayer!);
            int prevIndex = (currentIndex - 1 + _navigationLayers.Count) % _navigationLayers.Count;
            var layer = _navigationLayers[prevIndex];
            return ActivateNavigationLayer(layer.Id);
        }

        return false;
    }

    public IEnumerator<IKeyboardNavigationLayer> GetEnumerator()
        => _navigationLayers.GetEnumerator();

    IEnumerator IEnumerable.GetEnumerator()
        => GetEnumerator();

    #endregion
}
