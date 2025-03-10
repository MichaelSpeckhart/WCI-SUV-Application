using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WCI_SUV.Core.Events
{
    public class NodeChangedEventArgs : EventArgs
    {
        public string NodeName { get; }
        public object OldValue { get;  }
        public object NewValue { get; }
        public DateTime Timestamp { get; }

        public NodeChangedEventArgs(string nodeName, object oldValue, object newValue)
        {
            NodeName = nodeName;
            OldValue = oldValue;
            NewValue = newValue;
            Timestamp = DateTime.Now;
        }


    }
}
