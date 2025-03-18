using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WCI_SUV.Core.Entities;

using WCI_SUV.Core.Interface.Database;

namespace WCI_SUV.Core.Services
{
    public class TicketManager
    {
        #region Private Members

        private List<Ticket> _ticketEntries;
        private readonly ITicketEntityService _ticketEntityService;

        #endregion


        #region Constructors and Initializers

        public TicketManager()
        {
            _ticketEntries = new List<Ticket>();
        }


        public async Task InitializeAsync()
        {
            
            _ticketEntries = _ticketEntityService.GetAllTicketsAsync().Result.ToList();



        }


        #endregion





    }
}
