using Avalonia.Input;
using Avalonia.Interactivity;

namespace Nodify.Interactivity
{
    /// <summary>
    /// Represents a keyboard gesture that requires a trigger key to be held down
    /// before pressing a combo key. For example, press and hold Space, then press Left arrow.
    /// </summary>
    public class KeyComboGesture : KeyGesture
    {
        private static readonly WeakReferenceCollection<KeyComboGesture> _allCombos = new WeakReferenceCollection<KeyComboGesture>(16);

        private bool _isTriggerDown;
        private int _comboCounter;

        /// <summary>
        /// Gets a value indicating whether the combo gesture has been performed at least once.
        /// </summary>
        private bool HasBeenPerformedAtLeastOnce => _comboCounter > 0;

        /// <summary>
        /// Gets or sets the key that must be pressed first to activate this combo gesture.
        /// </summary>
        public Key TriggerKey { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the combo key can be repeatedly triggered
        /// without releasing the trigger key.
        /// </summary>
        public bool AllowRepeatingComboKey { get; set; }

        static KeyComboGesture()
        {
            InputElement.KeyUpEvent.AddClassHandler<InputElement>(
                (_, e) => HandleKeyUp(e),
                RoutingStrategies.Tunnel,
                handledEventsToo: true);

            InputElement.LostFocusEvent.AddClassHandler<InputElement>(
                (_, e) => HandleFocusLost(e),
                handledEventsToo: true);
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="KeyComboGesture"/> class.
        /// </summary>
        public KeyComboGesture(Key triggerKey, Key comboKey) : this(triggerKey, comboKey, KeyModifiers.None)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="KeyComboGesture"/> class with modifiers.
        /// </summary>
        public KeyComboGesture(Key triggerKey, Key comboKey, KeyModifiers modifiers) : base(comboKey, modifiers)
        {
            TriggerKey = triggerKey;
            _allCombos.Add(this);
        }

        private static void HandleFocusLost(RoutedEventArgs e)
        {
            foreach (var combo in _allCombos)
            {
                combo.Reset();
            }
        }

        private static void HandleKeyUp(KeyEventArgs e)
        {
            foreach (var combo in _allCombos)
            {
                if (e.Key == combo.TriggerKey)
                {
                    if (combo.HasBeenPerformedAtLeastOnce)
                    {
                        e.Handled = true;
                    }
                    combo.Reset();
                }
            }
        }

        private void Reset()
        {
            _isTriggerDown = false;
            _comboCounter = 0;
        }

        /// <inheritdoc />
        public override bool Matches(object? targetElement, RoutedEventArgs inputEventArgs)
        {
            // Only trigger on key down events
            if (inputEventArgs.RoutedEvent != InputElement.KeyDownEvent)
                return false;

            if (inputEventArgs is KeyEventArgs keyArgs)
            {
                if (keyArgs.Key == TriggerKey)
                {
                    _isTriggerDown = true;
                }

                bool matches = _isTriggerDown && base.Matches(targetElement, inputEventArgs);
                if (!matches)
                {
                    return false;
                }

                _comboCounter++;

                if (!AllowRepeatingComboKey)
                {
                    _isTriggerDown = false;
                }

                return matches;
            }

            return false;
        }
    }
}
