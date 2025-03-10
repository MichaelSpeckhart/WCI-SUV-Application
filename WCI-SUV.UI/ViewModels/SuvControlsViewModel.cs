using Microsoft.Extensions.Logging;
using System;
using System.ComponentModel;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Input;
using WCI_SUV.Core.Common;
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
        private bool _isRunning;
        private string _currentSlot;
        private Int16 _targetSlot;

        private CancellationTokenSource _conveyorTokenSource;
        private CancellationTokenSource _slotUpdateTokenSource;

        public SuvControlsViewModel(
            ILogger<SuvControlsViewModel> logger,
            IOpcService opcService,
            string opcServerAddress)
        {
            _logger = logger;
            _opcService = opcService;
            _opcServerAddress = opcServerAddress;

            _statusMessage = string.Empty;
            _targetSlot = 0;

            InitializeCommands();
        }

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
                    var val = await _opcService.GetCurrentSlotNumber();

                    string currSlot = "";
                    
                    if (val == null)
                    {
                        currSlot = string.Empty;
                    } else
                    {
                        CurrentSlot = val.Value.ToString();
                    } 
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error initializing OPC-UA connection.");
                StatusMessage = $"Error: {ex.Message}";
            }
        }

        #region Public Properties

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

        #endregion

        #region Commands

        public ICommand ConveyorForwardPressCommand { get; private set; }
        public ICommand ConveyorForwardReleaseCommand { get; private set; }
        public ICommand ConveyorReversePressCommand { get; private set; }
        public ICommand ConveyorReverseReleaseCommand { get; private set; }
        public ICommand SetTargetSlotCommand { get; private set; }

        private void InitializeCommands()
        {
            ConveyorForwardPressCommand = new RelayCommand(_ => StartConveyorForward(), _ => true);
            ConveyorForwardReleaseCommand = new RelayCommand(_ => StopConveyor(), _ => true);
            ConveyorReversePressCommand = new RelayCommand(_ => StartConveyorReverse(), _ => true);
            ConveyorReverseReleaseCommand = new RelayCommand(_ => StopConveyor(), _ => true);
            SetTargetSlotCommand = new RelayCommand(async _ => await SetTargetSlot(), _ => true);
        }

        #endregion

        #region OPC Control Methods

        public void StartConveyorForward()
        {
            StopConveyor(); // Ensure no previous task is running

            _conveyorTokenSource = new CancellationTokenSource();
            _slotUpdateTokenSource = new CancellationTokenSource();
            CancellationToken token = _conveyorTokenSource.Token;

            Task.Run(async () =>
            {
                while (!token.IsCancellationRequested)
                {
                    try
                    {
                        var result = await _opcService.ConveyorJogFoward();
                        if (!result.isSuccess)
                        {
                            StatusMessage = "Failed to move forward: " + result.ErrorMessage;
                            return;
                        }
                        StatusMessage = "Moving Forward...";
                        await Task.Delay(200, token); // Adjust delay for frequency of OPC calls
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "Error sending forward command.");
                        StatusMessage = "Error moving forward.";
                    }
                }
            }, token);
            StartSlotUpdateLoop();
        }

        public void StartConveyorReverse()
        {
            StopConveyor(); // Ensure no previous task is running

            _conveyorTokenSource = new CancellationTokenSource();
            _slotUpdateTokenSource = new CancellationTokenSource();
            CancellationToken token = _conveyorTokenSource.Token;

            Task.Run(async () =>
            {
                while (!token.IsCancellationRequested)
                {
                    try
                    {
                        var result = await _opcService.ConveyorJogReverse();
                        
                        if (!result.isSuccess)
                        {
                            StatusMessage = "Failed to move reverse: " + result.ErrorMessage;
                            return;
                        }
                        StatusMessage = "Moving Reverse...";
                        await Task.Delay(200, token); // Adjust delay for frequency of OPC calls
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "Error sending reverse command.");
                        StatusMessage = "Error moving reverse.";
                    }
                }
            }, token);
            StartSlotUpdateLoop();
        }

        private void StartSlotUpdateLoop()
        {
            if (_slotUpdateTokenSource == null || _slotUpdateTokenSource.IsCancellationRequested)
            {
                _slotUpdateTokenSource = new CancellationTokenSource();
            }

            CancellationToken token = _slotUpdateTokenSource.Token;

            Task.Run(async () =>
            {
                while (!token.IsCancellationRequested)
                {
                    try
                    {
                        var slotTask = await _opcService.GetCurrentSlotNumber();

                        var slotResult = slotTask;
                        
                        if (slotResult.isSuccess)
                        {
                            CurrentSlot = slotResult.Value.ToString();
                        }
                    }
                    catch (Exception ex)
                    {
                        StatusMessage = "Error fetching slot: " + ex.Message;
                    }

                    await Task.Delay(10, token); // Adjust polling rate as needed
                }
            }, token);
        }


        public void StopConveyor()
        {
            if (_conveyorTokenSource != null)
            {
                _conveyorTokenSource.Cancel();
                _conveyorTokenSource.Dispose();
                _conveyorTokenSource = null;
            }

            if (_slotUpdateTokenSource != null)
            {
                _slotUpdateTokenSource.Cancel();
                _slotUpdateTokenSource.Dispose();
                _slotUpdateTokenSource = null;
            }

            // 🚨 Explicitly send STOP signal to OPC
            _opcService.WriteNodeValue<bool>(83, false);
            _opcService.WriteNodeValue<bool>(84, false);

            StatusMessage = "Stopped.";
        }




        private async Task SetTargetSlot()
        {
            if (!int.TryParse(CurrentSlot, out int currentSlot))
            {
                StatusMessage = "Error: Invalid current slot value.";
                return;
            }

            if (currentSlot == TargetSlot)
            {
                StatusMessage = "Already at Target Slot.";
                return;
            }



            await _opcService.WriteNodeValue<Int16>(264, TargetSlot);

            bool moveForward = Math.Abs(TargetSlot - currentSlot) > 100
                ? TargetSlot < currentSlot
                : TargetSlot > currentSlot;

            _conveyorTokenSource = new CancellationTokenSource();
            _slotUpdateTokenSource = new CancellationTokenSource();
            CancellationToken token = _conveyorTokenSource.Token;

            StatusMessage = $"Moving to Slot {TargetSlot}... ({(moveForward ? "Forward" : "Reverse")})";

            await Task.Run(async () =>
            {
                while (!token.IsCancellationRequested)
                {
                    // Fetch latest slot position
                    var slotResult = await _opcService.GetCurrentSlotNumber();
                    if (slotResult.isSuccess)
                    {
                        CurrentSlot = slotResult.Value.ToString();
                    }

                    if (!int.TryParse(CurrentSlot, out currentSlot))
                    {
                        StatusMessage = "Error: Unable to read Current Slot.";
                        StopConveyor();
                        return;
                    }

                    // Adjust speed if close to target slot
                    if (Math.Abs(currentSlot - TargetSlot) <= 15)
                    {
                        await _opcService.SlowConveyorSpeed();
                    }

                    // 🚨 STOP MOVEMENT IF TARGET IS REACHED
                    if (currentSlot == TargetSlot)
                    {
                        StatusMessage = $"Arrived at Slot {TargetSlot}. Stopping...";

                        // 🚨 Ensure Conveyor Stops
                        StopConveyor();
                        return;
                    }

                    

                    // Move in the correct direction
                    var result = moveForward
                        ? await _opcService.ConveyorJogFoward()
                        : await _opcService.ConveyorJogReverse();

                    if (!result.isSuccess)
                    {
                        StatusMessage = $"Error moving {(moveForward ? "Forward" : "Reverse")}. Stopping...";
                        StopConveyor();
                        return;
                    }

                    await Task.Delay(200, token);
                }
            }, token);
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
