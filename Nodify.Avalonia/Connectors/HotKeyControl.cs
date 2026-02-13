using Avalonia;
using Avalonia.Controls;

namespace Nodify;

public class HotKeyControl : Control
{
    public static readonly StyledProperty<int> NumberProperty =
        AvaloniaProperty.Register<HotKeyControl, int>(nameof(Number), defaultValue: 0);

    public int Number
    {
        get => GetValue(NumberProperty);
        set => SetValue(NumberProperty, value);
    }

    static HotKeyControl()
    {
    }
}
