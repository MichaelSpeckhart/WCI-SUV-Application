using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WCI_SUV.Core.Entities;


namespace WCI_SUV.Core.Interface.Database
{
    public interface ITicketEntityService
    {
        Task<Ticket> GetTicketByIdAsync(Int32 ticketNumber);
        Task<IEnumerable<Ticket>> GetAllTicketsAsync();
        Task<Ticket> AddTicketAsync(Ticket ticket);
        Task UpdateTicketAsync(Ticket ticket);
        Task DeleteTicketAsync(Int32 ticketNumber);
        Task<bool> TicketNumberExists(Int32 ticketNumber);
        public List<Int32>? GetSlotColumn();

    }
}
