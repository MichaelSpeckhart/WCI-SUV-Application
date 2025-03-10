using Microsoft.Extensions.Logging;
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
using WCI_SUV.DB.Data;

using WCI_SUV.DB.Services;


// Internal Project imports
using WCI_SUV.UI.ViewModels;
using WCI_SUV.UI;

namespace WCI_SUV.UI.Views
{
    /// <summary>
    /// Interaction logic for ConveyorControlsView.xaml
    /// </summary>
    public partial class ConveyorControlsView : Page
    {
        public ConveyorControlsView(ConveyorControlsViewModel viewModel)
        {
            DataContext = viewModel;
            InitializeComponent();
            Unloaded += async (s, e) =>
            {
                if (DataContext is ConveyorControlsViewModel vm)
                {
                    await vm.CleanupAsync();
                }
            };
        }

        private async void OnLoaded(object sender, RoutedEventArgs e)
        {
            //// Assuming you're using dependency injection to get services
            //var logger = AppHost.Services.GetRequiredService<ILogger<ConveyorControlsViewModel>>();
            //var context = AppHost.Services.GetRequiredService<ApplicationDbContext>();
            //var nodeEntityLogger = AppHost.Services.GetRequiredService<ILogger<NodeEntityService>>();

            //// Create the ViewModel and set the DataContext
            //DataContext = await ConveyorControlsViewModel.CreateAsync(logger, context, nodeEntityLogger);
        }


    }
}
