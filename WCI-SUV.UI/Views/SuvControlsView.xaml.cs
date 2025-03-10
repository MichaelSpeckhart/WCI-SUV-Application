using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using WCI_SUV.UI.ViewModels;

namespace WCI_SUV.UI.Views
{
    public partial class SuvControlsView : Page
    {
        private readonly SuvControlsViewModel _viewModel;

        public SuvControlsView(SuvControlsViewModel viewModel)
        {
            InitializeComponent();
            _viewModel = viewModel;
            DataContext = _viewModel;
            Unloaded += async (s, e) =>
            {
                if (DataContext is SuvControlsViewModel vm)
                {
                    await vm.CleanupAsync();
                }
            };
        }

        private void LeftButton_Pressed(object sender, MouseButtonEventArgs e)
        {
            _viewModel.StartConveyorReverse();
        }

        private void RightButton_Pressed(object sender, MouseButtonEventArgs e)
        {
            _viewModel.StartConveyorForward();
        }

        private void Button_Released(object sender, MouseButtonEventArgs e)
        {
            _viewModel.StopConveyor();
        }
    }
}
