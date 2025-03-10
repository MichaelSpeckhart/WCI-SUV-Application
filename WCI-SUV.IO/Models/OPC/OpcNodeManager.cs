using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WCI_SUV.IO.Models.OPC
{


    public class OpcNodeManager
    {

        // Enumerate Node IDs
        public enum NodeIds
        {
            Running = 79,
            TargetSlot = 264,
            CurrentSlot = 163,
            SlowSpeed = 249
        }

        public string ConstructNodeId(int namespacex, NodeIds nodeId)
        {
            return string.Format("ns={0};i={1}", namespacex, nodeId);
        }

        public OpcNodeManager()
        {

        }

        // Each Node should have a type associated with it. Running is a bool, target slot is an INT16, and so on
        public Type? GetNodeType(NodeIds nodeId)
        {
            switch (nodeId)
            {
                case NodeIds.Running:
                    return typeof(bool);
                case NodeIds.TargetSlot:
                    return typeof(Int16);
                case NodeIds.CurrentSlot:
                    return typeof(Int16);
                case NodeIds.SlowSpeed:
                    return typeof(Int16);
                default:
                    return null;
            }
        }




    }
}
