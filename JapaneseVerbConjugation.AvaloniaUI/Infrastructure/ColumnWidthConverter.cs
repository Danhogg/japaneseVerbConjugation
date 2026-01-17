using System;
using System.Globalization;
using Avalonia.Data.Converters;

namespace JapaneseVerbConjugation.AvaloniaUI.Infrastructure
{
    public sealed class ColumnWidthConverter : IValueConverter
    {
        public double MinItemWidth { get; set; } = 420;

        public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            if (value is not double totalWidth || totalWidth <= 0)
                return MinItemWidth;

            var columns = Math.Max(1, (int)Math.Floor(totalWidth / MinItemWidth));
            return Math.Floor(totalWidth / columns);
        }

        public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
            => throw new NotSupportedException();
    }
}
