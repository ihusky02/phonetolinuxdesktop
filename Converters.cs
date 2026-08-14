using System;
using System.Globalization;
using Avalonia.Data.Converters;
using Avalonia.Layout;
using Avalonia.Media;

namespace phonetolinux
{
    // Konwerter tła wiadomości: wychodzące (zielone), przychodzące (szare/ciemne)
    public class MessageBgConverter : IValueConverter
    {
        public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            if (value is bool isOutgoing && isOutgoing)
            {
                // Wiśnięta/Wysłana przez nas (np. zielona)
                return Brush.Parse("#2E7D32");
            }
            // Wiadomość przychodząca (np. ciemnoszara)
            return Brush.Parse("#2C2C2C");
        }

        public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    // Konwerter wyrównania wiadomości: wychodzące do prawej, przychodzące do lewej
    public class MessageAlignConverter : IValueConverter
    {
        public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            if (value is bool isOutgoing && isOutgoing)
            {
                return HorizontalAlignment.Right;
            }
            return HorizontalAlignment.Left;
        }

        public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}