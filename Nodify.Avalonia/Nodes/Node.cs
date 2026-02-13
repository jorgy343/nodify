using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Controls.Templates;
using Avalonia.Media;
using Avalonia.Styling;
using System.Collections;

namespace Nodify;

/// <summary>
/// Represents a control that has a list of <see cref="Input"/> connectors and a list of <see cref="Output"/> connectors.
/// This is the Avalonia port of the WPF Node control with a modernized API.
/// </summary>
public class Node : HeaderedContentControl
{
    protected const string ElementInputItemsControl = "PART_Input";
    protected const string ElementOutputItemsControl = "PART_Output";

    #region Styled Properties

    /// <summary>
    /// Defines the <see cref="ContentBrush"/> property.
    /// </summary>
    public static readonly StyledProperty<IBrush?> ContentBrushProperty =
        AvaloniaProperty.Register<Node, IBrush?>(nameof(ContentBrush));

    /// <summary>
    /// Defines the <see cref="HeaderBrush"/> property.
    /// </summary>
    public static readonly StyledProperty<IBrush?> HeaderBrushProperty =
        AvaloniaProperty.Register<Node, IBrush?>(nameof(HeaderBrush));

    /// <summary>
    /// Defines the <see cref="FooterBrush"/> property.
    /// </summary>
    public static readonly StyledProperty<IBrush?> FooterBrushProperty =
        AvaloniaProperty.Register<Node, IBrush?>(nameof(FooterBrush));

    /// <summary>
    /// Defines the <see cref="Footer"/> property.
    /// </summary>
    public static readonly StyledProperty<object?> FooterProperty =
        AvaloniaProperty.Register<Node, object?>(nameof(Footer));

    /// <summary>
    /// Defines the <see cref="FooterTemplate"/> property.
    /// </summary>
    public static readonly StyledProperty<IDataTemplate?> FooterTemplateProperty =
        AvaloniaProperty.Register<Node, IDataTemplate?>(nameof(FooterTemplate));

    /// <summary>
    /// Defines the <see cref="InputConnectorTemplate"/> property.
    /// </summary>
    public static readonly StyledProperty<IDataTemplate?> InputConnectorTemplateProperty =
        AvaloniaProperty.Register<Node, IDataTemplate?>(nameof(InputConnectorTemplate));

    /// <summary>
    /// Defines the <see cref="OutputConnectorTemplate"/> property.
    /// </summary>
    public static readonly StyledProperty<IDataTemplate?> OutputConnectorTemplateProperty =
        AvaloniaProperty.Register<Node, IDataTemplate?>(nameof(OutputConnectorTemplate));

    /// <summary>
    /// Defines the <see cref="Input"/> property.
    /// </summary>
    public static readonly StyledProperty<IEnumerable?> InputProperty =
        AvaloniaProperty.Register<Node, IEnumerable?>(nameof(Input));

    /// <summary>
    /// Defines the <see cref="Output"/> property.
    /// </summary>
    public static readonly StyledProperty<IEnumerable?> OutputProperty =
        AvaloniaProperty.Register<Node, IEnumerable?>(nameof(Output));

    /// <summary>
    /// Defines the <see cref="ContentContainerStyle"/> property.
    /// </summary>
    public static readonly StyledProperty<Style?> ContentContainerStyleProperty =
        AvaloniaProperty.Register<Node, Style?>(nameof(ContentContainerStyle));

    /// <summary>
    /// Defines the <see cref="HeaderContainerStyle"/> property.
    /// </summary>
    public static readonly StyledProperty<Style?> HeaderContainerStyleProperty =
        AvaloniaProperty.Register<Node, Style?>(nameof(HeaderContainerStyle));

    /// <summary>
    /// Defines the <see cref="FooterContainerStyle"/> property.
    /// </summary>
    public static readonly StyledProperty<Style?> FooterContainerStyleProperty =
        AvaloniaProperty.Register<Node, Style?>(nameof(FooterContainerStyle));

    /// <summary>
    /// Defines the <see cref="HasFooter"/> property.
    /// </summary>
    public static readonly DirectProperty<Node, bool> HasFooterProperty =
        AvaloniaProperty.RegisterDirect<Node, bool>(
            nameof(HasFooter),
            o => o.HasFooter);

    private bool _hasFooter;

    #endregion

    #region Properties

    /// <summary>
    /// Gets or sets the brush used for the background of the content of this <see cref="Node"/>.
    /// </summary>
    public IBrush? ContentBrush
    {
        get => GetValue(ContentBrushProperty);
        set => SetValue(ContentBrushProperty, value);
    }

    /// <summary>
    /// Gets or sets the brush used for the background of the header of this <see cref="Node"/>.
    /// </summary>
    public IBrush? HeaderBrush
    {
        get => GetValue(HeaderBrushProperty);
        set => SetValue(HeaderBrushProperty, value);
    }

    /// <summary>
    /// Gets or sets the brush used for the background of the footer of this <see cref="Node"/>.
    /// </summary>
    public IBrush? FooterBrush
    {
        get => GetValue(FooterBrushProperty);
        set => SetValue(FooterBrushProperty, value);
    }

    /// <summary>
    /// Gets or sets the data for the footer of this control.
    /// </summary>
    public object? Footer
    {
        get => GetValue(FooterProperty);
        set => SetValue(FooterProperty, value);
    }

    /// <summary>
    /// Gets or sets the template used to display the content of the control's footer.
    /// </summary>
    public IDataTemplate? FooterTemplate
    {
        get => GetValue(FooterTemplateProperty);
        set => SetValue(FooterTemplateProperty, value);
    }

    /// <summary>
    /// Gets or sets the template used to display the content of the control's <see cref="Input"/> connectors.
    /// </summary>
    public IDataTemplate? InputConnectorTemplate
    {
        get => GetValue(InputConnectorTemplateProperty);
        set => SetValue(InputConnectorTemplateProperty, value);
    }

    /// <summary>
    /// Gets or sets the template used to display the content of the control's <see cref="Output"/> connectors.
    /// </summary>
    public IDataTemplate? OutputConnectorTemplate
    {
        get => GetValue(OutputConnectorTemplateProperty);
        set => SetValue(OutputConnectorTemplateProperty, value);
    }

    /// <summary>
    /// Gets or sets the data for the input connectors of this control.
    /// </summary>
    public IEnumerable? Input
    {
        get => GetValue(InputProperty);
        set => SetValue(InputProperty, value);
    }

    /// <summary>
    /// Gets or sets the data for the output connectors of this control.
    /// </summary>
    public IEnumerable? Output
    {
        get => GetValue(OutputProperty);
        set => SetValue(OutputProperty, value);
    }

    /// <summary>
    /// Gets or sets the style for the content container.
    /// </summary>
    public Style? ContentContainerStyle
    {
        get => GetValue(ContentContainerStyleProperty);
        set => SetValue(ContentContainerStyleProperty, value);
    }

    /// <summary>
    /// Gets or sets the style for the header container.
    /// </summary>
    public Style? HeaderContainerStyle
    {
        get => GetValue(HeaderContainerStyleProperty);
        set => SetValue(HeaderContainerStyleProperty, value);
    }

    /// <summary>
    /// Gets or sets the style for the footer container.
    /// </summary>
    public Style? FooterContainerStyle
    {
        get => GetValue(FooterContainerStyleProperty);
        set => SetValue(FooterContainerStyleProperty, value);
    }

    /// <summary>
    /// Gets a value that indicates whether the <see cref="Footer"/> is <see langword="null" />.
    /// </summary>
    public bool HasFooter
    {
        get => _hasFooter;
        private set => SetAndRaise(HasFooterProperty, ref _hasFooter, value);
    }

    #endregion

    static Node()
    {
        FooterProperty.Changed.AddClassHandler<Node>((node, e) => node.OnFooterChanged(e));
    }

    private void OnFooterChanged(AvaloniaPropertyChangedEventArgs e)
    {
        HasFooter = e.NewValue != null;
    }
}
