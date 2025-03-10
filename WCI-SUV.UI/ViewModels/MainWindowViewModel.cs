using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using WCI_SUV.Core.Interface.OPC;
using WCI_SUV.DB.Services;
using WCI_SUV.UI.Commands;
using WCI_SUV.IO.Services.OPC;
using WCI_SUV.UI.Views;
using Microsoft.Extensions.DependencyInjection;
using System.Windows;

namespace WCI_SUV.UI.ViewModels
{
    public class MainWindowViewModel : INotifyPropertyChanged
    {

        #region Private Fields
        private readonly ILogger<MainWindowViewModel> _logger;
        private readonly ILoggerFactory _loggerFactory;
        private readonly ConveyorEntityService _conveyorEntityService;
        private readonly IOpcService _opcService;
        private readonly IServiceProvider _serviceProvider;

        private object _currentViewModel;
        #endregion

        public object CurrentViewModel
        {
            get => _currentViewModel;
            set
            {
                _currentViewModel = value;
                OnPropertyChanged(nameof(CurrentViewModel));
            }
        }


        public ConveyorControlsViewModel _conveyorControls { get; }

        #region Constructor
        public MainWindowViewModel(
            ILogger<MainWindowViewModel> logger,
            ILoggerFactory loggerFactory,
            ConveyorEntityService conveyorService,
            IOpcService opcService,
            IServiceProvider serviceProvider)
        {
            _logger = logger;
            _loggerFactory = loggerFactory;
            _conveyorEntityService = conveyorService;
            _opcService = opcService;

            OpenConveyorControlsCommand = new RelayCommand(async _ => await OpenConveyorControls());
            OpenDatabaseViewCommand = new RelayCommand(async _ => await OpenDatabaseView());
            OpenSuvControlsViewCommand = new RelayCommand(async _ => await OpenSuvControlsView());
            _serviceProvider = serviceProvider;
            // CurrentViewModel = new DefaultViewModel();  // Set initial view
        }
        #endregion

        #region Command Execution Methods
        private async Task OpenConveyorControls()
        {
            try
            {
                var conveyorViewModel = _serviceProvider.GetService<ConveyorControlsViewModel>();

                if (conveyorViewModel == null)
                {
                    _logger.LogError("ConveyorViewModel is null");
                    return;
                }

                var mainWindow = App.Current.MainWindow as MainWindow;
                mainWindow?.MainFrame.Navigate(new ConveyorControlsView(conveyorViewModel));

                await Task.Run(async () => {
                    await conveyorViewModel.InitializeAsync();
                    // Dispatch UI updates back to main thread
                    await Application.Current.Dispatcher.InvokeAsync(() => {
                        conveyorViewModel.StatusMessage = conveyorViewModel.StatusMessage;
                    });
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error opening conveyor controls");
            }
        }

        public async Task OpenDatabaseView()
        {
            try
            {
                var databaseViewModel = _serviceProvider.GetService<DatabaseTableViewModel>();

                if (databaseViewModel == null)
                {
                    _logger.LogError("DatabaseViewModel is null");
                    return;
                }

                var mainWindow = App.Current.MainWindow as MainWindow;
                mainWindow?.MainFrame.Navigate(new DatabaseTableView(databaseViewModel));

                await Task.Run(async () =>
                {
                    await databaseViewModel.InitializeAsync();
                    // Dispatch UI updates back to main thread
                    await Application.Current.Dispatcher.InvokeAsync(() => {
                        databaseViewModel.StatusMessage = databaseViewModel.StatusMessage;
                    });
                });

            } catch (Exception ex)
            {
                _logger.LogError(ex, "Error opening database view");
            }
        }

        public async Task OpenSuvControlsView()
        {
            try
            {
                var suvViewModel = _serviceProvider.GetService<SuvControlsViewModel>();

                if (suvViewModel == null)
                {
                    _logger.LogError("SuvViewModel is null");
                    return;
                }

                var mainWindow = App.Current.MainWindow as MainWindow;
                mainWindow?.MainFrame.Navigate(new SuvControlsView(suvViewModel));

                await Task.Run(async () =>
                {
                    await suvViewModel.InitializeAsync();
                    // Dispatch UI updates back to main thread
                    await Application.Current.Dispatcher.InvokeAsync(() => {
                        suvViewModel.StatusMessage = suvViewModel.StatusMessage;
                    });
                });

            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error opening suv controls view");
            }
        }

        #endregion

        #region Commands
        public ICommand OpenConveyorControlsCommand     { get; }
        public ICommand OpenDatabaseViewCommand         { get; }
        public ICommand OpenSuvControlsViewCommand      { get; }
        #endregion

        public event PropertyChangedEventHandler PropertyChanged;
        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }


    }
}
