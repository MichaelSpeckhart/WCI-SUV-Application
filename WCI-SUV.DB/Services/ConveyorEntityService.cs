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
    public class ConveyorEntityService : IConveyorEntityService
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<ConveyorEntityService> _logger;

        public ConveyorEntityService(ApplicationDbContext context)
        {
            _context = context;
            var loggerFactory = LoggerFactory.Create(builder =>
            {
                builder.AddConsole();
            });

            _logger = loggerFactory.CreateLogger<ConveyorEntityService>();
        }

        public ConveyorEntityService()
        {
            _context = null;
            _logger = null;
        }

        public async Task<Conveyor> GetConveyorByIdAsync(Int32 accountNumber)
        {
            var defaultConveyor = new Conveyor();

            try
            {
                var conveyor = await _context.Conveyors.FindAsync(accountNumber);

                if (conveyor == null)
                {
                    _logger.LogWarning("In function 'GetConveyorByIdAsync', returns null");
                    return defaultConveyor;
                }


                return conveyor;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                _logger.LogError($"In function 'GetConveyorByIdAsync', exception occured: {ex.Message}");
            }

            return defaultConveyor;
        }

        /// <summary>
        /// Get all the conveyors in a list
        /// </summary>
        /// <returns></returns>
        public async Task<IEnumerable<Conveyor>> GetAllConveyorAsync()
        {
            try
            {
                var results = await _context.Conveyors
                    .AsNoTracking()
                    .ToListAsync()
                    .ConfigureAwait(false);

                if (results == null || results.Count() == 0)
                {
                    _logger.LogWarning("No conveyors found in database");
                    return Enumerable.Empty<Conveyor>();
                }

                return results;

            }
            catch (SqlException dbException)
            {
                _logger.LogError(dbException, "SQL error occurred while retrieving conveyors: {Message}", dbException.Message);
                throw;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error occurred while retrieving conveyors: {Message}", ex.Message);
                throw;
            }
        }


        public async Task<Conveyor> AddConveyorAsync(Conveyor conveyor)
        {
            if (conveyor == null)
            {
                throw new ArgumentNullException(nameof(conveyor));
            }

            try
            {
                var result = await _context.Conveyors.AddAsync(conveyor)
                    .ConfigureAwait(false);

                await _context.SaveChangesAsync()
                    .ConfigureAwait(false);

                return result.Entity;

            }
            catch (DbUpdateException updateException)
            {
                _logger.LogError(updateException, "Failed to save conveyor to database: {Message}", updateException.Message);
                throw;
            }
            catch (SqlException dbException)
            {
                _logger.LogError(dbException, "SQL error occurred while adding a conveyor: {Message}", dbException.Message);
                throw;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error occurred while adding conveyor: {Message}", ex.Message);
                throw;
            }
        }


        public async Task UpdateConveyorAsync(Conveyor conveyor)
        {
            if (conveyor == null)
            {
                throw new ArgumentNullException(nameof(conveyor));
            }

            try
            {
                _context.Conveyors.Update(conveyor);
                await _context.SaveChangesAsync().ConfigureAwait(false);

            } catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error occurred while updating conveyor: {Message}", ex.Message);
                throw;
            }
        }


        public async Task DeleteConveyorAsync(Int32 accountNumber)
        {
            try
            {
                var conveyor = await _context.Conveyors.FindAsync(accountNumber);

                if (conveyor == null)
                {
                    throw new ArgumentNullException(nameof(conveyor));
                }

                var result = _context.Conveyors.Remove(conveyor);

                await _context.SaveChangesAsync();

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


        public Task<bool> AccountNumberExists(Int32 accountNumber)
        {
            try
            {
                var result = _context.Conveyors.AnyAsync(c => c.AccountNumber == accountNumber);

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
            // Returns a list from the "Slot" column in the database table
            try
            {
                var results = _context.Conveyors
                    .Select(c => c.SlotNumber)
                    .ToList();

                return results;


            }
            catch (SqlException sqlException)
            {
                throw;
            }
            catch (DbUpdateConcurrencyException dbUpdateException)
            {
                throw;
            }
            catch (Exception ex)
            {
                throw;
            }
        }

        // Function that returns the values of a column in a List given the name of the column
        // from the Conveyor Entity class
        public List<object>? GetColumn(string columnName)
        {
            try
            {
                var results = _context.Conveyors
                    .Select(c => c.GetType().GetProperty(columnName).GetValue(c, null))
                    .ToList();

                return results;
            }
            catch (SqlException sqlException)
            {
                throw;
            }
            catch (DbUpdateConcurrencyException dbUpdateException)
            {
                throw;
            }
            catch (Exception ex)
            {
                throw;
            }
        }

    }
}
