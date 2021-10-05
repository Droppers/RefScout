using System;
using System.Globalization;
using System.Windows.Data;
using RefScout.Analyzer;

namespace RefScout.Wpf.Converters;

internal class SourceLanguageToTextConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        return value switch
        {
            null => string.Empty,
            AssemblySourceLanguage language => language switch
            {
                AssemblySourceLanguage.VbNet => "VB.NET",
                AssemblySourceLanguage.CSharp => "C Sharp",
                AssemblySourceLanguage.FSharp => "F Sharp",
                AssemblySourceLanguage.CppCli => "C++/CLI",
                _ => language.ToString()
            },
            _ => string.Empty
        };
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) =>
        throw new NotSupportedException();
}