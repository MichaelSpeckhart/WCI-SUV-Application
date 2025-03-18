using Microsoft.Extensions.Logging;
using System;
using System.ComponentModel;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Input;
using WCI_SUV.Core.Interface;
using WCI_SUV.Core.Interface.OPC;
using WCI_SUV.UI.Commands;

namespace WCI_SUV.UI.ViewModels
{
    public class SuvControlsViewModel : INotifyPropertyChanged
    {
        private readonly ILogger<SuvControlsViewModel> _logger;
        private readonly IOpcService _opcService;
        private readonly string _opcServerAddress;

        private string _statusMessage;
        private string _currentSlot;
        private Int16 _targetSlot;
        private bool _isConnected;
        private CancellationTokenSource _cancellationTokenSource;

        public SuvControlsViewModel(ILogger<SuvControlsViewModel> logger, IOpcService opcService, string opcServerAddress)
        {
            _logger = logger;
            _opcService = opcService;
            _opcServerAddress = opcServerAddress;

            InitializeCommands();
        }

        public async Task InitializeAsync()
        {
            try
            {
                _isConnected = await _opcService.ConnectToServer(_opcServerAddress);
                StatusMessage = _isConnected ? "Connected to OPC-UA" : "Failed to connect.";
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "OPC-UA Connection Error");
                StatusMessage = $"Error: {ex.Message}";
            }
        }

        public string StatusMessage
        {
            get => _statusMessage;
            set { _statusMessage = value; OnPropertyChanged(nameof(StatusMessage)); }
        }

        public string CurrentSlot
        {
            get => _currentSlot;
            set { _currentSlot = value; OnPropertyChanged(nameof(CurrentSlot)); }
        }

        public Int16 TargetSlot
        {
            get => _targetSlot;
            set { _targetSlot = value; OnPropertyChanged(nameof(TargetSlot)); }
        }

        public ICommand SetTargetSlotCommand { get; private set; }

        private void InitializeCommands()
        {
            SetTargetSlotCommand = new RelayCommand(async _ => await MoveToTargetSlot(), _ => _isConnected);
        }

        private async Task MoveToTargetSlot()
        {
            if (!Int16.TryParse(CurrentSlot, out Int16 currentSlot))
            {
                StatusMessage = "Invalid Current Slot.";
                return;
            }

            if (currentSlot == TargetSlot)
            {
                StatusMessage = "Already at Target Slot.";
                return;
            }

            _cancellationTokenSource?.Cancel();
            _cancellationTokenSource = new CancellationTokenSource();
            CancellationToken token = _cancellationTokenSource.Token;

            StatusMessage = $"Moving to Slot {TargetSlot}...";
            var result = await _opcService.GoToTargetSlot();

            if (result.isSuccess)
            {
                StatusMessage = $"Arrived at Slot {TargetSlot}.";
            }
            else
            {
                StatusMessage = $"Error: {result.ErrorMessage}";
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;
        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
