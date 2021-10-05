using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Windows;
using System.Windows.Data;

namespace RefScout.Wpf.Converters;

internal class InverseNullOrEmptyCollectionVisibilityConverter : IValueConverter
{
    public object Convert(object? value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is not IEnumerable<object> enumerable)
        {
            return Visibility.Collapsed;
        }

        return !enumerable.Any() ? Visibility.Collapsed : Visibility.Visible;
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) =>
        throw new NotSupportedException();
}