using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using WCI_SUV.Core.Entities;
using WCI_SUV.Core.Interface.Database;


namespace WCI_SUV.Core.Services
{
    public class NodeManager
    {

        private readonly ConcurrentDictionary<string, Node> _nodes;
        private readonly INodeEntityService _nodeEntityService;

        public NodeManager(INodeEntityService service)
        {
            _nodes = new ConcurrentDictionary<string, Node>();
            _nodeEntityService = service;
        }

        public async Task InitializeAsync()
        {
            var nodes = (List<Node>)await _nodeEntityService.GetAllNodesAsync();
            foreach (var node in nodes)
            {
                _nodes.TryAdd(node.Name, node);
            }
        }

        public async Task<Node>? GetNode(string name)
        {
            var res = await _nodeEntityService.GetNodeByNameAsync(name);
            
            return res;
        }

        public async Task AddNodeAsync(Node node)
        {
            try
            {
                await _nodeEntityService.AddNodeAsync(node);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Exception in adding node: {ex.Message}");
            }
            
            _nodes.TryAdd(node.Name, node);
        }

        public int GetNodeCount()
        {
            return _nodes.Count;
        }
    }
}
