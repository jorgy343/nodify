using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using Avalonia.Styling;

namespace Nodify;

public class NodifyTheme : Styles
{
    public NodifyTheme(IServiceProvider? sp = null)
    {
        AvaloniaXamlLoader.Load(sp, this);
    }
}
