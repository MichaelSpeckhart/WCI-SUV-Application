using Microsoft.Extensions.Logging;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WCI_SUV.Core.Entities;
using WCI_SUV.Core.Interface;

namespace WCI_SUV.DB.Services
{
    public class TicketCache : ITicketCache
    {

        // Need private members: TicketEntityService, List<Ticket>

        #region Private Members

        private readonly TicketEntityService        _ticketEntityService;
        private readonly Dictionary<Int32, Ticket>  _ticketEntries = new();
        private readonly ILogger<TicketCache>       _logger;

        #endregion

        #region Constructors and Initializers

        public TicketCache(TicketEntityService ticketEntityService)
        {
            _ticketEntityService = ticketEntityService;
            var loggerFactory = LoggerFactory.Create(builder =>
            {
                builder.AddConsole();
            });
            _logger = loggerFactory.CreateLogger<TicketCache>();
        }

        #endregion

        #region Public Methods
        public async Task LoadTicketsAsync()
        {
            try
            {
                var tickets = await _ticketEntityService.GetAllTicketsAsync();

                _ticketEntries.Clear();

                foreach (var ticket in tickets)
                {
                    _ticketEntries.Add(ticket.TicketNumber, ticket);
                }
                _logger.LogInformation($"Loaded {_ticketEntries.Count} tickets into the cache");
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error loading tickets into cache: {ex.Message}");
            }
           
        }

        public Ticket GetTicket(Int32 ticketNumber)
        {
            if (_ticketEntries.ContainsKey(ticketNumber))
            {
                return _ticketEntries[ticketNumber];
            }
            return null;
        }

        public IEnumerable<Ticket> GetAllTickets()
        {
            return _ticketEntries.Values;
        }

        public async Task<bool> AddTicketToCacheAsync(Ticket newTicket)
        {
            try
            {
                if (_ticketEntries.ContainsKey(newTicket.TicketNumber))
                {
                    _logger.LogError($"Ticket {newTicket.TicketNumber} already exists in the cache.");
                    return false;
                }
                _ticketEntries.Add(newTicket.TicketNumber, newTicket);
                await _ticketEntityService.AddTicketAsync(newTicket);
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error adding ticket to cache: {ex.Message}");
                return false;
            }

        }


        public async Task<bool> FlushTable()
        {
            try
            {
                foreach (var ticket in _ticketEntries.Values)
                {
                    if (ticket != null)
                    {
                        await _ticketEntityService.UpdateTicketAsync(ticket);
                    }
                    
                }

                //_ticketEntries.Clear();

                return true;

            }
            catch (Exception ex)
            {
                _logger.LogError($"Error flushing table: {ex.Message}");
                return false;
            }
        }

        #endregion 


    }
}
