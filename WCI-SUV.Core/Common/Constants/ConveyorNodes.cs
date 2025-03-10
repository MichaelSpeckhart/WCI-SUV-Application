using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WCI_SUV.Core.Common
{
    public static class ConveyorNodes
    {

        public const string CONVEYOR_RUN        =   "SlotRunning";
        public const string CONVEYOR_STOP       =   "SlotRunning"; // Same as Conveyor Run just need to assign false val
        public const string CONVEYOR_SPEED      =   "RunSpeed";
        public const string CONVEYOR_REVERSE    =   "RunRev";
        public const string CURRENT_SLOT        =   "ActualSlotID1";
        public const string TARGET_SLOT         =   "SlotID1";
        public const string CONVEYOR_FWRD       =   "Station1JogFwd";
        public const string CONVEYOR_REV        =   "Station1JogRev";
        public const string SLOT_RUN_REQ        =   "SlotRunRequest";
        public const string CREEP_SPEED         =   "CreepSpeed";
        public const string CURR_SPEED          =   "CurrentSpeed";

   
    }
}
