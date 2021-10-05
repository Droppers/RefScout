using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Windows;
using System.Windows.Data;

namespace RefScout.Wpf.Converters;

internal class NullOrEmptyCollectionVisibilityConverter : IValueConverter
{
    public object Convert(object? value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is not IEnumerable<object> enumerable)
        {
            return Visibility.Visible;
        }

        return !enumerable.Any() ? Visibility.Visible : Visibility.Collapsed;
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) =>
        throw new NotSupportedException();
}