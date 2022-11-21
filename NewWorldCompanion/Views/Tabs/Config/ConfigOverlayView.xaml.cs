using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace NewWorldCompanion.Views.Tabs.Config
{
    /// <summary>
    /// Interaction logic for ConfigOverlayView.xaml
    /// </summary>
    public partial class ConfigOverlayView : UserControl
    {
        public ConfigOverlayView()
        {
            InitializeComponent();
        }

        private void TextBoxFilter_GotFocus(object sender, RoutedEventArgs e)
        {
            TextBoxFilterWatermark.Visibility = Visibility.Collapsed;
        }

        private void TextBoxFilter_LostFocus(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(TextBoxFilter.Text))
            {
                TextBoxFilterWatermark.Visibility = Visibility.Visible;
            }
        }
    }
}
