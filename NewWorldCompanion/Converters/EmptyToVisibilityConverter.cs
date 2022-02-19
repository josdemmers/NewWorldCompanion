using NewWorldCompanion.Entities;
using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace NewWorldCompanion.Converters
{
    public class EmptyToVisibilityConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            CraftingRecipe craftingRecipe = (CraftingRecipe)value;
            return string.IsNullOrWhiteSpace(craftingRecipe?.Id) ? Visibility.Hidden : Visibility.Visible;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
