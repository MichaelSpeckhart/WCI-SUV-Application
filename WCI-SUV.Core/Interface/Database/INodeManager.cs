using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WCI_SUV.Core.Entities;
using WCI_SUV.Core.Events;

namespace WCI_SUV.Core.Interface.Database
{
    public interface INodeManager
    {
        Node GetNode(string name);
        IReadOnlyDictionary<string,Node> GetAllNodes();
        void UpdateNodeValue(string name, object value);
        event EventHandler<NodeChangedEventArgs> NodeValueChanged;


    }
}
