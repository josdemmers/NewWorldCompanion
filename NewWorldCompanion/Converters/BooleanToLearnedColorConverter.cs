using System;
using System.Globalization;
using System.Windows.Data;
using System.Windows.Media;

namespace NewWorldCompanion.Converters
{
    class BooleanToLearnedColorConverter : IValueConverter
    {
        public Object Convert(Object value, Type targetType, Object parameter, CultureInfo culture)
        {
            if (targetType == typeof(Brush))
            {
                var learned = System.Convert.ToBoolean(value, culture);

                if (learned) return Brushes.Green;
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
