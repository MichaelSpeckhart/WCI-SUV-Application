using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WCI_SUV.Core.Interface
{
    public interface ITicketProcessor
    {

        Task<bool> ProcessTicket(Int32 ticketNumber, Int16 currentSlot);
        Task<bool> InitializeAsync();


    }
}
