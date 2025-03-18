using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WCI_SUV.Core.Interface
{
    public interface IConveyorCache
    {

        Int16? GetClosestAvailableSlot(Int16 currentSlot);
        void ReserveSlot(Int32 slotNumber);
        void ReleaseSlot(Int32 slotNumber);
        public void PrintConveyorCache();
    }
}
