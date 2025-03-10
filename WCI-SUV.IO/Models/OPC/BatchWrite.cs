using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Metadata.Ecma335;
using System.Text;
using System.Threading.Tasks;

namespace WCI_SUV.IO.Models.OPC
{
    public class BatchWrite
    {

        Dictionary<string, object> _nodes;

        public BatchWrite()
        {
            _nodes = new Dictionary<string, object>();
        }

        public void AddNode(string nodeName, object value)
        {
            _nodes.Add(nodeName, value);
        }

        public void RemoveNode(string nodeName)
        {
            _nodes.Remove(nodeName);
        }

        public object? GetNodeValue(string nodeName)
        {
            return _nodes[nodeName];
        }








    }
}
