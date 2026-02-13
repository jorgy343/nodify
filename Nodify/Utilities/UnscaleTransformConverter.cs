using System;
using System.Collections.Generic;
using System.Globalization;
using Avalonia.Data.Converters;
using Avalonia.Media;

namespace Nodify
{
    internal sealed class UnscaleTransformConverter : IValueConverter
    {
        public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            Transform result = (Transform)((TransformGroup)value!).Children[0].Inverse!;
            return result;
        }

        public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            return value;
        }
    }

    internal sealed class ScaleDoubleConverter : IMultiValueConverter
    {
        public object? Convert(IList<object?> values, Type targetType, object? parameter, CultureInfo culture)
        {
            double result = (double)values[0]! * (double)values[1]!;
            return result;
        }
    }

    internal sealed class ScalePointConverter : IMultiValueConverter
    {
        public object? Convert(IList<object?> values, Type targetType, object? parameter, CultureInfo culture)
        {
            var point = (Avalonia.Point)values[0]!;
            var scale = (double)values[1]!;
            var result = new Avalonia.Point(point.X * scale, point.Y * scale);
            return result;
        }
    }
}
