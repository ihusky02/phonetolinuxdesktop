using System;
using System.Globalization;
using Avalonia.Data.Converters;
using Avalonia.Media;

namespace phonetolinux
{
    /// <summary>
    /// Converts current selected tab index to active background highlight brush.
    /// </summary>
    public class TabHighlightConverter : IValueConverter
    {
        public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            if (value is int selectedIndex && parameter != null && int.TryParse(parameter.ToString(), out int targetIndex))
            {
                if (selectedIndex == targetIndex)
                {
                    return Brush.Parse("#2C2C2C"); // Active tab background
                }
            }
            return Brush.Parse("Transparent"); // Inactive background
        }

        public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}