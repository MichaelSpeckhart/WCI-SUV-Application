using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WCI_SUV.Core.Common;
using WCI_SUV.Core.Entities;
using WCI_SUV.Core.Interface.Database;
using WCI_SUV.DB.Data;

namespace WCI_SUV.DB.Services
{
    public class NodeEntityService : INodeEntityService
    {
        private readonly ApplicationDbContext _dbContext;
        private readonly ILogger<NodeEntityService> _logger;

        public NodeEntityService(ApplicationDbContext dbContext)
        {
            _dbContext = dbContext ?? throw new ArgumentNullException(nameof(dbContext));
            var loggerFactory = LoggerFactory.Create(builder =>
            {
                builder.AddConsole();
            });

            _logger = loggerFactory.CreateLogger<NodeEntityService>();
        }

        public async Task<Node> GetNodeByNameAsync(string name)
        {
            var defaultNode = new Node();

            try
            {
                var node = await _dbContext.Nodes.FindAsync(name).ConfigureAwait(false);

                if (node == null)
                {
                    _logger.LogWarning("In function 'GetNodeByNameAsync', returns null");
                    return defaultNode;
                }

                return node;
            }
            catch (SqlException sqlException)
            {
                _logger.LogError($"In function 'GetNodeByNameAsync', SQL exception occured: {sqlException.Message}");
            }
            catch (DbException dbException)
            {
                _logger.LogError($"In function 'GetNodeByNameAsync', DB exception occured: {dbException.Message}");
            }
            catch (Exception exception)
            {
                _logger.LogError($"In function 'GetNodeByNameAsync', exception occured: {exception.Message}");
            }
            return defaultNode;
        }

        public async Task<IEnumerable<Node>> GetAllNodesAsync()
        {
            try
            {
                var results = await _dbContext.Nodes.
                    AsNoTracking().
                    ToListAsync().
                    ConfigureAwait(false);

                return results;
            }
            catch (SqlException sqlException)
            {
                _logger.LogError($"In function 'GetAllNodesAsync', SQL exception occured: {sqlException.Message}");
            }
            catch (DbException dbException)
            {
                _logger.LogError($"In function 'GetAllNodesAsync', DB exception occured: {dbException.Message}");
            }
            catch (Exception exception)
            {
                _logger.LogError($"In function 'GetAllNodesAsync', exception occured: {exception.Message}");
            }
            return default(IEnumerable<Node>);
        }

        public async Task<Result<bool>> AddNodeAsync(Node node)
        {
            if (node == null)
            {
                throw new ArgumentNullException("Node is null");
            }

            if (_dbContext.Nodes == null)
            {
                throw new ArgumentNullException("Node table is null");
            }

            try
            {
                await _dbContext.Nodes.AddAsync(node);
                var res = await _dbContext.SaveChangesAsync();

                return res > 0
                    ? Result<bool>.Success(true)
                    : Result<bool>.Failure("No changes were saved to the database");
            }
            catch (DbUpdateException dbUpdateException)
            {
                _logger.LogError($"In function 'AddNodeAsync', DB update exception occured: {dbUpdateException.Message}"
                    + $" Inner exception: {dbUpdateException.InnerException?.Message}");
                return Result<bool>.Failure($"In function 'AddNodeAsync', DB update exception occured: {dbUpdateException.Message}"
                    + $" Inner exception: {dbUpdateException.InnerException?.Message}");
            }
            catch (SqlException sqlException)
            {
                _logger.LogError($"In function 'AddNodeAsync', SQL exception occured: {sqlException.Message}");
                return Result<bool>.Failure($"In function 'AddNodeAsync', SQL exception occured: {sqlException.Message}");
            }
            catch (DbException dbException)
            {
                _logger.LogError($"In function 'AddNodeAsync', DB exception occured: {dbException.Message}");
                return Result<bool>.Failure($"In function 'AddNodeAsync', DB exception occured: {dbException.Message}");
            }
            catch (Exception exception)
            {
                _logger.LogError($"In function 'AddNodeAsync', exception occured: {exception.Message}");
                return Result<bool>.Failure($"In function 'AddNodeAsync', exception occured: {exception.Message}");
            }
        }

        public async Task UpdateNodeAsync(Node node)
        {
            try
            {
                _dbContext.Nodes.Update(node);
                await _dbContext.SaveChangesAsync();
            }
            catch (DbUpdateException dbUpdateException)
            {
                _logger.LogError($"In function 'UpdateNodeAsync', DB update exception occured: {dbUpdateException.Message}"
                    + $" Inner exception: {dbUpdateException.InnerException?.Message}");
            }
            catch (SqlException sqlException)
            {
                _logger.LogError($"In function 'UpdateNodeAsync', SQL exception occured: {sqlException.Message}");
            }
            catch (DbException dbException)
            {
                _logger.LogError($"In function 'UpdateNodeAsync', DB exception occured: {dbException.Message}");
            }
            catch (Exception exception)
            {
                _logger.LogError($"In function 'UpdateNodeAsync', exception occured: {exception.Message}");
            }
        }

        public async Task DeleteNodeAsync(string name)
        {
            try
            {
                await _dbContext.Nodes.Where(x => x.Name == name)
                    .ExecuteDeleteAsync();

                await _dbContext.SaveChangesAsync();
            }
            catch (DbUpdateException dbUpdateException)
            {
                _logger.LogError($"In function 'DeleteNodeAsync', DB update exception occured: {dbUpdateException.Message}"
                    + $" Inner exception: {dbUpdateException.InnerException?.Message}");
            }
            catch (SqlException sqlException)
            {
                _logger.LogError($"In function 'DeleteNodeAsync', SQL exception occured: {sqlException.Message}");
            }
            catch (DbException dbException)
            {
                _logger.LogError($"In function 'DeleteNodeAsync', DB exception occured: {dbException.Message}");
            }
            catch (Exception exception)
            {
                _logger.LogError($"In function 'DeleteNodeAsync', exception occured: {exception.Message}");
            }
        }

        public async Task<bool> NodeNameExistsAsync(string name)
        {
            return await _dbContext.Nodes.AnyAsync(x => x.Name == name);
        }

        //public async Task<List<object>> GetColumnByName(string columnName)
        //{

        //}

    }

}
