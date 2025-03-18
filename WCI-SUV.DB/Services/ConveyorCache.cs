using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using WCI_SUV.Core.Interface;

namespace WCI_SUV.Core.Services
{
    public class ConveyorCache : IConveyorCache
    {
        // Need a list of numbers 1 -200
        private List<Int32> _conveyorCache = new();


        public ConveyorCache()
        {
            for (int i = 1; i <= 200; i++)
            {
                _conveyorCache.Add(0);
            }
        }

        public void PrintConveyorCache()
        {
            Console.WriteLine("Conveyor Cache:");
            for (int i = 0; i < _conveyorCache.Count; i++)
            {
                Console.WriteLine($"Slot {i + 1}: {_conveyorCache[i]}");
            }
        }

        public Int16? GetClosestAvailableSlot(Int16 currentSlot)
        {
            Int16? rightClosestSlot = null;
            Int16? leftClosestSlot = null;

            // Search Right
            for (Int16 i = (Int16)(currentSlot + 1); i < _conveyorCache.Count; i++)
            {
                if (_conveyorCache[i] == 0)
                {
                    rightClosestSlot = i;
                    break;
                }
            }

            // Search Left
            for (Int16 i = (Int16)(currentSlot - 1); i >= 0; i--) // Ensure we check slot 0
            {
                if (_conveyorCache[i] == 0)
                {
                    leftClosestSlot = i;
                    break;
                }
            }

            // No slots available
            if (rightClosestSlot == null && leftClosestSlot == null)
            {
                return null;
            }

            // If only one side has an available slot
            if (rightClosestSlot == null)
            {
                return leftClosestSlot;
            }
            if (leftClosestSlot == null)
            {
                return rightClosestSlot;
            }

            // Return the closest slot
            return (currentSlot - leftClosestSlot <= rightClosestSlot - currentSlot)
                ? leftClosestSlot
                : rightClosestSlot;
        }

        public void ReserveSlot(Int32 slotNumber)
        {
            _conveyorCache[slotNumber] = 1;
        }

        public void ReleaseSlot(Int32 slotNumber)
        {
            _conveyorCache[slotNumber] = 0;
        }
    }
}
