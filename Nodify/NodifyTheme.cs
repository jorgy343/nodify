using Avalonia.Markup.Xaml;
using Avalonia.Styling;

namespace Nodify
{
    /// <summary>
    /// Provides the default Nodify theme for Avalonia applications.
    /// Include this in your App.axaml Styles collection to use Nodify controls.
    /// </summary>
    public class NodifyTheme : Styles
    {
        public NodifyTheme()
        {
            AvaloniaXamlLoader.Load(this);
        }
    }
}
