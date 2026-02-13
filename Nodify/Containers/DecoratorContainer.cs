using Nodify.Interactivity;
using System;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Data;
using Avalonia.Interactivity;
using Avalonia.VisualTree;

namespace Nodify
{
    /// <summary>
    /// The container for all the items generated from the <see cref="NodifyEditor.Decorators"/> collection.
    /// </summary>
    public class DecoratorContainer : ContentControl, INodifyCanvasItem, IKeyboardFocusTarget<DecoratorContainer>
    {
        #region Styled Properties

        public static readonly StyledProperty<Point> LocationProperty =
            ItemContainer.LocationProperty.AddOwner<DecoratorContainer>(
                new StyledPropertyMetadata<Point>(defaultBindingMode: BindingMode.TwoWay));
        public static readonly StyledProperty<Size> ActualSizeProperty =
            ItemContainer.ActualSizeProperty.AddOwner<DecoratorContainer>();

        /// <summary>
        /// Gets or sets the location of this <see cref="DecoratorContainer"/> inside the <see cref="NodifyEditor.DecoratorsHost"/>.
        /// </summary>
        public Point Location
        {
            get => GetValue(LocationProperty);
            set => SetValue(LocationProperty, value);
        }

        /// <summary>
        /// Gets the actual size of this <see cref="DecoratorContainer"/>.
        /// </summary>
        public Size ActualSize
        {
            get => GetValue(ActualSizeProperty);
            set => SetValue(ActualSizeProperty, value);
        }

        private static void OnLocationChanged(DecoratorContainer item, AvaloniaPropertyChangedEventArgs e)
        {
            item.OnLocationChanged();
        }

        #endregion

        #region Routed Events

        public static readonly RoutedEvent<RoutedEventArgs> LocationChangedEvent =
            RoutedEvent.Register<DecoratorContainer, RoutedEventArgs>(nameof(LocationChanged), RoutingStrategies.Bubble);

        /// <summary>
        /// Occurs when the <see cref="Location"/> of this <see cref="DecoratorContainer"/> is changed.
        /// </summary>
        public event EventHandler<RoutedEventArgs> LocationChanged
        {
            add => AddHandler(LocationChangedEvent, value);
            remove => RemoveHandler(LocationChangedEvent, value);
        }

        /// <summary>
        /// Raises the <see cref="LocationChangedEvent"/>.
        /// </summary>
        protected void OnLocationChanged()
        {
            RaiseEvent(new RoutedEventArgs(LocationChangedEvent, this));
        }

        #endregion

        Rect IKeyboardFocusTarget<DecoratorContainer>.Bounds => new Rect(Location, ActualSize);
        DecoratorContainer IKeyboardFocusTarget<DecoratorContainer>.Element => this;

        private DecoratorsControl? _owner;
        public DecoratorsControl? Owner => _owner ??= this.GetParentOfType<DecoratorsControl>();

        static DecoratorContainer()
        {
            FocusableProperty.OverrideDefaultValue<DecoratorContainer>(true);
            AffectsParentArrange<DecoratorContainer>(LocationProperty);
            LocationProperty.Changed.AddClassHandler<DecoratorContainer>(OnLocationChanged);
        }

        public DecoratorContainer(DecoratorsControl parent)
        {
            _owner = parent;
            SizeChanged += OnSizeChanged;
            DetachedFromVisualTree += OnDetachedFromVisualTree;
        }

        public DecoratorContainer()
        {
            SizeChanged += OnSizeChanged;
            DetachedFromVisualTree += OnDetachedFromVisualTree;
        }

        private void OnSizeChanged(object? sender, SizeChangedEventArgs e)
        {
            ActualSize = e.NewSize;
        }

        private void OnDetachedFromVisualTree(object? sender, VisualTreeAttachmentEventArgs e)
        {
            if (IsKeyboardFocusWithin)
            {
                Owner?.Editor?.Focus();
            }
        }
    }
}
