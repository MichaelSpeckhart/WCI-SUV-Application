using Microsoft.Extensions.Logging;
using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using WCI_SUV.Core.Interface.OPC;
using WCI_SUV.DB.Services;
using WCI_SUV.IO.Services.OPC;
using WCI_SUV.UI.Commands;

namespace WCI_SUV.UI.ViewModels
{
    public class ConveyorControlsViewModel : INotifyPropertyChanged
    {
        #region Private Fields
        private readonly ILogger<ConveyorControlsViewModel> _logger;
        private readonly ConveyorEntityService _conveyorService;
        private readonly IOpcService _opcService;

        private string _opcServerAddress;

        private bool _isRunning;
        private bool _isReversed;
        private short _currSpeed;
        private short _targetSlot;
        private string _speedInput;
        private string _slotInput;
        private string _statusMessage;
        #endregion

        #region Constructor
        public ConveyorControlsViewModel(
            ILogger<ConveyorControlsViewModel> logger,
            ConveyorEntityService conveyorService,
            IOpcService opcService,
            string opcServerAddress)
        {
            _logger = logger;
            _conveyorService = conveyorService;
            _opcService = opcService;
            _opcServerAddress = opcServerAddress;

            _isRunning = false;
            _isReversed = false;
            _currSpeed = 0;
            _targetSlot = 0;
            _statusMessage = "";

            InitializeCommands();
        }
        #endregion

        #region Async Initialization
        public async Task InitializeAsync()
        {
            await Task.Yield(); // Allows UI to refresh before execution starts

            try
            {
                bool isConnected = await _opcService.ConnectToServer(_opcServerAddress);
                if (!isConnected)
                {
                    _logger.LogError("Failed to connect to OPC-UA server.");
                    StatusMessage = "Failed to connect to OPC-UA.";
                }
                else
                {
                    _logger.LogInformation("Connected to OPC-UA server.");
                    StatusMessage = "Connected to OPC-UA.";
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error initializing OPC-UA connection.");
                StatusMessage = $"Error: {ex.Message}";
            }
        }


        #endregion

        #region Properties

        public bool IsRunning
        {
            get => _isRunning;
            set
            {
                if (_isRunning != value)
                {
                    _isRunning = value;
                    OnPropertyChanged(nameof(IsRunning));

                    // Refresh button state
                    ((RelayCommand)RunConveyorCommand).RaiseCanExecuteChanged();
                    ((RelayCommand)StopConveyorCommand).RaiseCanExecuteChanged();
                }
            }
        }


        public bool IsReversed
        {
            get => _isReversed;
            set { _isReversed = value; OnPropertyChanged(nameof(IsReversed)); }
        }

        public short CurrSpeed
        {
            get => _currSpeed;
            set { _currSpeed = value; OnPropertyChanged(nameof(CurrSpeed)); }
        }

        public short TargetSlot
        {
            get => _targetSlot;
            set { _targetSlot = value; OnPropertyChanged(nameof(TargetSlot)); }
        }

        public string StatusMessage
        {
            get => _statusMessage;
            set { _statusMessage = value; OnPropertyChanged(nameof(StatusMessage)); }
        }
        #endregion

        #region Commands
        public ICommand RunConveyorCommand      { get; private set; }
        public ICommand StopConveyorCommand     { get; private set; }
        public ICommand SetTargetSlotCommand    { get; private set; }
        public ICommand ConveyorRightCommand    { get; private set; }
        public ICommand ConveyorLeftCommand     { get; private set; }
        public ICommand ReverseConveyorCommand  { get; private set; }

        private void InitializeCommands()
        {
            RunConveyorCommand   = new RelayCommand(async _ => await RunConveyor(), _ => !IsRunning);
            StopConveyorCommand  = new RelayCommand(async _ => await StopConveyor(), _ => IsRunning);
            SetTargetSlotCommand = new RelayCommand(async _ => await SetTargetSlot(), _ => IsRunning);
            ConveyorRightCommand = new RelayCommand(async _ => await SetConveyorDirection(), _ => IsRunning);
        }
        #endregion

        #region Command Execution Methods
        private async Task RunConveyor()
        {
            try
            {
                IsRunning = true;
                await _opcService.RunConveyor();
                StatusMessage = "Conveyor running.";
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error starting conveyor.");
                StatusMessage = $"Error: {ex.Message}";
            }
        }

        private async Task StopConveyor()
        {
            try
            {
                IsRunning = false;
                await _opcService.StopConveyor();
                StatusMessage = "Conveyor stopped.";
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error stopping conveyor.");
                StatusMessage = $"Error: {ex.Message}";
            }
        }

        private async Task SetTargetSlot()
        {
            try
            {
                await _opcService.SetTargetSlot(_targetSlot);
                StatusMessage = $"{_targetSlot}";
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error setting target slot");
                StatusMessage = $"Error: {ex.Message}";
            }
        }

        private async Task SetConveyorDirection()
        {
            try
            {
                if (IsReversed)
                {
                    await _opcService.ReverseConveyorDirection();
                }
            } catch (Exception ex)
            {
                _logger.LogError(ex, "Error setting conveyor direction");
                StatusMessage = $"Error: {ex.Message}";
            }
        }

        private async Task ReverseConveyor()
        {
            try
            {
                IsReversed = !IsReversed;
                await _opcService.ReverseConveyorDirection();
                StatusMessage = "Conveyor direction reversed.";
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error reversing conveyor direction.");
                StatusMessage = $"Error: {ex.Message}";
            }
        }
        #endregion

        #region INotifyPropertyChanged
        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
        #endregion

        public async Task CleanupAsync()
        {
            try
            {
                _opcService.Dispose();
                _logger.LogInformation("Disconnected from OPC-UA server.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error disconnecting from OPC-UA server.");
            }
        }
    }
}
