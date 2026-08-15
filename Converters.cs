using System;
using System.Globalization;
using Avalonia.Data.Converters;
using Avalonia.Layout;
using Avalonia.Media;

namespace phonetolinux;

// Konwerter tła wiadomości (wychodzące: zielone, przychodzące: ciemnoszare)
public class MessageBgConverter : IValueConverter
{
    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is bool isOutgoing)
        {
            return isOutgoing ? Brush.Parse("#2E7D32") : Brush.Parse("#2C2C2C");
        }
        return Brush.Parse("#2C2C2C");
    }

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}

// Konwerter wyrównania wiadomości (wychodzące: prawa strona, przychodzące: lewa strona)
public class MessageAlignConverter : IValueConverter
{
    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is bool isOutgoing)
        {
            return isOutgoing ? HorizontalAlignment.Right : HorizontalAlignment.Left;
        }
        return HorizontalAlignment.Right;
    }

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}