using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WCI_SUV.Core.Entities;
using WCI_SUV.Core.Interface.Database;

namespace WCI_SUV.Core.Services
{
    public class ConveyorService : IConveyorEntityService
    {
        private readonly ApplicationDbContext _context;
        
        public Task<Conveyor> GetConveyorByIdAsync(long accountNumber)
        {
            try
            {
                
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
        }


        public Task<IEnumerable<Conveyor>> GetAllConveyorAsync()
        {

        }


        public Task<Conveyor> AddConveyorAsync(Conveyor coneyor)
        {

        }


        public Task UpdateConveyorAsync(Conveyor conveyor)
        {

        }


        public Task DeleteConveyorAsync(long accountNumber)
        {

        }


        public Task<bool> AccountNumberExists(long accountNumber)
        {

        }


        public List<long>? GetSlotColumn()
        {

        }





    }
}
