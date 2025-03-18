using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WCI_SUV.Core.Interface;
using WCI_SUV.Core.Interface.Database;
using WCI_SUV.Core.Interface.OPC;
using WCI_SUV.Core.Entities;

namespace WCI_SUV.Core.Services
{
    public class TicketProcessor : ITicketProcessor
    {

        #region Private Fields

        private readonly ITicketCache               _ticketCache;
        private readonly IConveyorCache             _conveyorCache;
        private readonly IOpcService                _opcService;

        private readonly ILogger<TicketProcessor>   _logger;  

        #endregion

        #region Constructors and Initializers

        public TicketProcessor(
            ITicketCache ticketCache,
            IConveyorCache conveyorCache,
            IOpcService opcService
        )
        {
            _ticketCache = ticketCache;
            _conveyorCache = conveyorCache;
            _opcService = opcService;

            var loggerFactory = LoggerFactory.Create(builder =>
            {
                builder.AddConsole();
            });

            _logger = loggerFactory.CreateLogger<TicketProcessor>();

        }

        public async Task<bool> InitializeAsync()
        {
            try
            {

                await _ticketCache.LoadTicketsAsync();
                var tickets = _ticketCache.GetAllTickets();

                foreach (var ticket in tickets)
                {
                    //Console.WriteLine($"Ticket {ticket.TicketNumber} - Slot: {ticket.SlotNumber}");
                    if (ticket.SlotNumber != null)
                    {
                        _conveyorCache.ReserveSlot(ticket.SlotNumber - 1);
                    }
                }

                _logger.LogInformation($"Loaded slots slots into the cache");

                //_conveyorCache.PrintConveyorCache();

                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error initializing TicketProcessor: {ex.Message}");
                return false;
            }
        }

        #endregion

        public async Task LoadTicketsAsync()
        {
            try
            {
                await _ticketCache.LoadTicketsAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error loading tickets into cache: {ex.Message}");
            }
        }

        public async Task<bool> ProcessTicket(Int32 ticketNumber, Int16 currentSlot)
        {
            try
            {
                Int32? closestSlot = _conveyorCache.GetClosestAvailableSlot(currentSlot);

                if (closestSlot == null)
                {
                    return false;
                }

                _conveyorCache.ReserveSlot(closestSlot.Value);

                _conveyorCache.PrintConveyorCache();

                Ticket newTicket = new Ticket
               {
                   TicketNumber = ticketNumber,
                   SlotNumber = closestSlot.Value
               };

                await _ticketCache.AddTicketToCacheAsync(newTicket);

                await _ticketCache.FlushTable();



                return true ;
            }
            catch (Exception ex)
            {
                return false;
            }
        }



    }
}
