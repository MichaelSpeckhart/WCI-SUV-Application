using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WCI_SUV.Core.Entities;
using WCI_SUV.Core.Interface.Database;
using WCI_SUV.DB.Data;

namespace WCI_SUV.DB.Services
{
    public class TicketEntityService : ITicketEntityService
    {

        #region Private Members

        private readonly ApplicationDbContext _context;
        private readonly ILogger<TicketEntityService> _logger;

        #endregion

        #region Constructors

        public TicketEntityService(ApplicationDbContext context)
        {
            _context = context;
            var loggerFactory = LoggerFactory.Create(builder =>
            {
                builder.AddConsole();
            });

            _logger = loggerFactory.CreateLogger<TicketEntityService>();
        }

        public TicketEntityService()
        {
            _context = null;
            _logger = null;
        }

        #endregion

        #region Public Methods

        public async Task<Ticket> GetTicketByIdAsync(Int32 ticketNumber)
        {
            var defaultTicket = new Ticket();

            try
            {
                var ticket = await _context.Tickets.FindAsync(ticketNumber);

                if (ticket == null)
                {
                    _logger.LogWarning("In function 'GetTicketByIdAsync', returns null");
                    return defaultTicket;
                }

                return ticket;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                _logger.LogError($"In function 'GetTicketByIdAsync', exception occured: {ex.Message}");
            }
            return defaultTicket;
        }


        public async Task<IEnumerable<Ticket>> GetAllTicketsAsync()
        {
            try
            {
                var results = await _context.Tickets
                    .AsNoTracking()
                    .ToListAsync()
                    .ConfigureAwait(false);

                if (results == null || results.Count() == 0)
                {
                    _logger.LogWarning("In function 'GetAllTicketsAsync', returns null");
                    return new List<Ticket>();
                }

                return results;
            }
            catch (SqlException dbException)
            {
                Console.WriteLine(dbException.Message);
                _logger.LogError($"In function 'GetAllTicketsAsync', exception occured: {dbException.Message}");
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                _logger.LogError($"In function 'GetAllTicketsAsync', exception occured: {ex.Message}");
            }
            return new List<Ticket>();
        }


        public async Task<Ticket> AddTicketAsync(Ticket ticket)
        {
            if (ticket == null)
            {
                throw new ArgumentNullException(nameof(ticket));
            }

            try
            {
                var result = await _context.Tickets.AddAsync(ticket)
                    .ConfigureAwait(false);

                var added = await _context.SaveChangesAsync()
                    .ConfigureAwait(false);

                return result.Entity;
            }
            catch (DbUpdateException updateException)
            {
                _logger.LogError($"In function 'AddTicketAsync', exception occured: {updateException.Message}");
                throw;
            }
            catch (SqlException sqlException)
            {
                _logger.LogError($"In function 'AddTicketAsync', exception occured: {sqlException.Message}");
                throw;
            }
            catch (Exception ex)
            {
                _logger.LogError($"In function 'AddTicketAsync', exception occured: {ex.Message}");
                throw;
            }
        }


        public async Task UpdateTicketAsync(Ticket ticket)
        {
            if (ticket == null)
            {
                throw new ArgumentNullException(nameof(ticket));
            }

            try
            {
                _context.Tickets.Update(ticket);
                await _context.SaveChangesAsync().ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error occurred while updating ticket: {Message}", ex.Message);
                throw;
            }
        }


        public async Task DeleteTicketAsync(Int32 ticketNumber)
        {
            try
            {
                var ticket = await _context.Tickets.FindAsync(ticketNumber);

                if (ticket == null)
                {
                    throw new ArgumentNullException(nameof(ticket));
                }

                var result = _context.Tickets.Remove(ticket);

                await _context.SaveChangesAsync().ConfigureAwait(false);    
            }
            catch (DbUpdateException updateException)
            {
                _logger.LogError(updateException, "Failed to save changes to database: {Message}", updateException.Message);
                throw;
            }
            catch (SqlException sqlException)
            {
                _logger.LogError(sqlException, "SQL error occurred while removing a conveyor: {Message}", sqlException.Message);
                throw;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error occurred while removing conveyor: {Message}", ex.Message);
                throw;
            }
        }


        public Task<bool> TicketNumberExists(Int32 ticketNumber)
        {
            try
            {
                var result = _context.Tickets.AnyAsync(t => t.TicketNumber == ticketNumber);
                return result;
            }
            catch (SqlException sqlException)
            {
                _logger.LogError(sqlException, "SQL error occurred while checking if account number exists: {Message}", sqlException.Message);
                throw;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error occurred while checking if account number exists: {Message}", ex.Message);
                throw;
            }
        }


        public List<Int32>? GetSlotColumn()
        {
            try
            {
                var results = _context.Tickets
                    .Select(c => c.SlotNumber)
                    .ToList();

                return results;


            }
            catch (SqlException sqlException)
            {
                _logger.LogError(sqlException, "SQL error occurred while getting slot column: {Message}", sqlException.Message);
                throw;
            }
            catch (DbUpdateConcurrencyException dbUpdateException)
            {
                _logger.LogError(dbUpdateException, "Concurrency error occurred while getting slot column: {Message}", dbUpdateException.Message);
                throw;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error occurred while getting slot column: {Message}", ex.Message);
                throw;
            }
        }

        



        #endregion


    }
}
