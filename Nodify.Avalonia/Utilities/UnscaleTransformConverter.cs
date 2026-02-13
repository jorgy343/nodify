using Avalonia;
using Avalonia.Data.Converters;
using Avalonia.Media;
using System;
using System.Collections.Generic;
using System.Globalization;

namespace Nodify;

internal sealed class UnscaleTransformConverter : IValueConverter
{
    private static readonly MatrixTransform _identityTransform = new(Matrix.Identity);

    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is TransformGroup group && group.Children.Count > 0)
        {
            // Get the inverse of the first transform (scale transform)
            var transform = group.Children[0];
            if (transform is ScaleTransform scale)
            {
                return new ScaleTransform(1.0 / scale.ScaleX, 1.0 / scale.ScaleY);
            }
        }
        return _identityTransform;
    }

    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        return value ?? _identityTransform;
    }
}

internal sealed class ScaleDoubleConverter : IMultiValueConverter
{
    public object? Convert(IList<object?> values, Type targetType, object? parameter, CultureInfo culture)
    {
        if (values.Count >= 2 && values[0] is double d1 && values[1] is double d2)
        {
            return d1 * d2;
        }
        return 0.0;
    }

    public object[] ConvertBack(object? value, Type[] targetTypes, object? parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}

internal sealed class ScalePointConverter : IMultiValueConverter
{
    public object? Convert(IList<object?> values, Type targetType, object? parameter, CultureInfo culture)
    {
        if (values.Count >= 2 && values[0] is Point point && values[1] is double scale)
        {
            return new Point(point.X * scale, point.Y * scale);
        }
        return new Point();
    }

    public object[] ConvertBack(object? value, Type[] targetTypes, object? parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}
