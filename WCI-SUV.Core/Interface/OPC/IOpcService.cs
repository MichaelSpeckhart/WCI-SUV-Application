using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WCI_SUV.Core.Common;
using WCI_SUV.Core.Entities;

namespace WCI_SUV.Core.Interface.OPC
{

    public enum NodeIds
    {
        Running = 79,
        TargetSlot = 264,
        CurrentSlot = 163,
        SlowSpeed = 249,
        Reverse = 179
    }

    public interface IOpcService
    {
        // Note: these should have corresponding buttons on the UI or at least directly interact with UI

        public Task<bool> ConnectToServer(string endpoint);


        //public Task<Result<Dictionary<string, string>>> GetNodesValues();
        //public Task<Result<object>> GetNodeValue(Node nodeId);
        //public Task<Result<bool>> SetNodeValue<T>(Node nodeID, T value);
        
        public Task<Result<bool>> SetTargetSlot(Int16 targetSlot); 
        public Task<Result<Int16>> GetTargetSlot();
        public Task<Result<bool>> GoToTargetSlot();
        //public Task<Result<bool>> ReverseConveyorDirection();

        // Conveyor Control Methods
        #region Conveyor Controls

        public Task<Result<bool>> StopConveyor(); // Stops Conveyor
        public Task<Result<bool>> RunConveyor(); // Starts Conveyor
        public Task<Result<bool>> SlowConveyorSpeed(); // Halves the Speed
        //public Task<Result<bool>> AccelerateConveyorSpeed(); // Doubles the speed
       // public Task<Result<bool>> SetConveyorSpeed(Int16 newSpeed); // Sets the specific speed

       // public Task<Result<bool>> ConveyorJogFoward();
        //public Task<Result<bool>> ConveyorJogReverse();

        //public Task<Result<bool>> WriteNodeValue<T>(uint node, T value);

        public Task<Result<Int16>> GetCurrentSlotNumber();



        #endregion

        public void Dispose();
    }
}
