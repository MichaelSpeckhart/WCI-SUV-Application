using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using WCI_SUV.UI.ViewModels;
using WCI_SUV.UI.Views;

namespace WCI_SUV.UI
{

    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        #region Private View Model Fields
        private readonly ConveyorControlsViewModel  _conveyorControlsViewModel;
        private readonly DatabaseTableViewModel     _databaseTableViewModel;
        private readonly SuvControlsViewModel       _suvControlsViewModel;
        #endregion

        public MainWindow(MainWindowViewModel viewModel)
        {
            InitializeComponent();
            DataContext = viewModel;
        }
    }
}
