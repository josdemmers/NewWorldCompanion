using System;
using System.Globalization;
using System.Windows.Data;
using System.Windows.Media;

namespace NewWorldCompanion.Converters
{
    class DateTimeToHealthConverter : IValueConverter
    {
        public Object Convert(Object value, Type targetType, Object parameter, CultureInfo culture)
        {
            if (targetType == typeof(Brush))
            {
                var updated = System.Convert.ToDateTime(value, culture);
                var timeSpan = DateTime.Now - updated;

                if (timeSpan < TimeSpan.FromHours(24))
                    return Brushes.Green;
                else if (timeSpan < TimeSpan.FromHours(36))
                    return Brushes.Orange;
                else
                    return Brushes.Red;
            }

            throw new InvalidOperationException("Converter can only convert to value of type Brush.");
        }

        public Object ConvertBack(Object aValue, Type aTargetType, Object aParameter, CultureInfo aCulture)
        {
            throw new InvalidOperationException("Converter cannot convert back.");
        }
    }
}
