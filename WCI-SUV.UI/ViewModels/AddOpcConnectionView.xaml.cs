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
using System.Windows.Shapes;

namespace WCI_SUV.UI.ViewModels
{
    /// <summary>
    /// Interaction logic for AddOpcConnectionView.xaml
    /// </summary>
    public partial class AddOpcConnectionView : Window
    {
        public AddOpcConnectionView()
        {
            InitializeComponent();
        }

        private void UpdateWatermarkVisibility()
        {
            ServerNameWatermark.Visibility = string.IsNullOrWhiteSpace(ServerNameTextBox.Text) ? Visibility.Visible : Visibility.Collapsed;
            EndpointWatermark.Visibility = string.IsNullOrWhiteSpace(EndpointTextBox.Text) ? Visibility.Visible : Visibility.Collapsed;
            UsernameWatermark.Visibility = string.IsNullOrWhiteSpace(UsernameTextBox.Text) ? Visibility.Visible : Visibility.Collapsed;
        }
    }
}
