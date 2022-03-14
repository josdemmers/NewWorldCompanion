using MahApps.Metro.Controls;
using MahApps.Metro.Controls.Dialogs;
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

namespace NewWorldCompanion.Views.Dialogs
{
    /// <summary>
    /// Interaction logic for CooldownConfigView.xaml
    /// </summary>
    public partial class CooldownConfigView : UserControl
    {
        public CooldownConfigView()
        {
            InitializeComponent();
        }

        private async void ButtonDone_Click(object sender, RoutedEventArgs e)
        {
            var dialog = (sender as DependencyObject).TryFindParent<BaseMetroDialog>();
            await(Application.Current.MainWindow as MetroWindow).HideMetroDialogAsync(dialog);
        }
    }
}
