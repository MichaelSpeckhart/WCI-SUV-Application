using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WCI_SUV.Core.Entities;

namespace WCI_SUV.Core.Interface
{
    public interface ITicketCache
    {

        Task LoadTicketsAsync();
        Ticket GetTicket(Int32 ticketNumber);
        IEnumerable<Ticket> GetAllTickets();
        Task<bool> FlushTable();
        Task<bool> AddTicketToCacheAsync(Ticket newTicket);


    }
}
