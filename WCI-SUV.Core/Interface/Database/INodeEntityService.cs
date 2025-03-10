using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WCI_SUV.Core.Common;
using WCI_SUV.Core.Entities;

namespace WCI_SUV.Core.Interface.Database
{
    public interface INodeEntityService
    {
        Task<Node> GetNodeByNameAsync(string name);
        Task<IEnumerable<Node>> GetAllNodesAsync();
        Task<Result<bool>> AddNodeAsync(Node node);
        Task UpdateNodeAsync(Node node);
        Task DeleteNodeAsync(string name);
        Task<bool> NodeNameExistsAsync(string name);
        //Task<List<object>> GetColumnByName(string columnName);
    }
}
