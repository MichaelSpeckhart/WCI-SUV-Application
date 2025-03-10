using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WCI_SUV.Core.Entities;

namespace WCI_SUV.Core.Interface.Database
{
    public interface IConveyorEntityService
    {
        Task<Conveyor> GetConveyorByIdAsync(Int32 accountNumber);
        Task<IEnumerable<Conveyor>> GetAllConveyorAsync();
        Task<Conveyor> AddConveyorAsync(Conveyor coneyor);
        Task UpdateConveyorAsync(Conveyor conveyor);
        Task DeleteConveyorAsync(Int32 accountNumber);
        Task<bool> AccountNumberExists(Int32 accountNumber);
        public List<Int32>? GetSlotColumn();
    }
}
