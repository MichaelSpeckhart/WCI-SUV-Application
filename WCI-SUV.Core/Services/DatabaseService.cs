using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.Text.Json;

using WCI_SUV.Core.Models;
using WCI_SUV.Core.Interface.Database;
using WCI_SUV.Core.Entities;
using System.Text.Json.Nodes;
using System.Text.Json.Serialization;
using WCI_SUV.Core.Common;
using System.Data.Common;
using System.IO;

namespace WCI_SUV.Core.Services
{
    public class DatabaseService
    {

        private readonly IConveyorEntityService _conveyorService;
        private readonly INodeEntityService _nodeService;
        private readonly EntityMapper _entityMapper;

        private string _tableName { get; set; }
        private string _appSettingsFile;
        private string _connectionString;


        public enum TableNames
        {
            Conveyor,
            Node
        }

        // Enum to map register name to register number



        public DatabaseService(IConveyorEntityService conveyorService, string tableName)
        {
            _conveyorService = conveyorService;
            _tableName = tableName;
        }

        public DatabaseService(INodeEntityService nodeService, string tableName)
        {
            _nodeService = nodeService;
            _tableName = tableName;
        }

        public async Task<IEnumerable<BaseEntity>> GetTableDataAsync(string tableName)
        {
            if (_entityMapper.GetEntityType(tableName) == null)
            {
                Console.WriteLine($"Table name {tableName} does not exist in the database.");
                return Enumerable.Empty<BaseEntity>();
            }

            if (tableName == "Conveyor")
            {
                return await _conveyorService.GetAllConveyorAsync();
            }
            else
            {
                Console.WriteLine($"Table name {tableName} does not exist in the database.");
                return Enumerable.Empty<BaseEntity>();
            }
        }


        #region App Settings
        
        public string GetAppSettingsFile()
        {
            return _appSettingsFile;
        }

        public void SetAppSettingsFile(string appSettingsFile)
        {
            _appSettingsFile = appSettingsFile;
        }

        public async Task WriteConnectionStringToFile(string connectionString)
        {
            if (string.IsNullOrEmpty(connectionString))
            {
                Console.WriteLine("Connection string is empty.");
                return;
            }

            if (string.IsNullOrEmpty(_appSettingsFile))
            {
                Console.WriteLine("App settings file is empty.");
                return;
            }

            using (Stream writer = File.OpenWrite(_appSettingsFile))
            {

                JsonDatabaseConnections conn = new JsonDatabaseConnections(string.Empty, connectionString);

                await JsonSerializer.SerializeAsync(writer, conn);

            }

        }
        #endregion

        public async Task<Result<bool>> WriteNodesToDatabase(string path, string tableName)
        {
            List<Node> nodes = new List<Node>();
            try
            {


                string[] lines = File.ReadAllLines(path);

                foreach (string line in lines)
                {
                    if (string.IsNullOrWhiteSpace(line))
                        continue;

                    string[] parts = line.Split(',').Select(p => p.Trim()).ToArray();



                    if (parts.Length >= 3)
                    {
                        string name = parts[0].Split('=')[1].Trim();

                        // Extract register numbers
                        string registerInfo = parts[1].Trim();
                        var registerMatch = System.Text.RegularExpressions.Regex.Match(registerInfo, @"ns=(\d+);i=(\d+)");
                        Int16 namespaceInfo = Int16.Parse(registerMatch.Groups[1].Value);
                        uint identifier = uint.Parse(registerMatch.Groups[2].Value);

                        string type = parts[2].Split('=')[1].Trim();

                        Node node = new Node
                        {
                            Name = name,
                            Namespace = namespaceInfo,
                            Register = identifier,
                            Type = type
                        };

                        await _nodeService.AddNodeAsync(node);

                    }
                }

                return Result<bool>.Success(true);
            }
            catch (Exception ex)
            {
                return Result<bool>.Failure(ex.Message
                    + " " + ex.InnerException?.Message);
            }

        }

        /// <summary>
        /// Create in memory table of nodes for fast access
        /// </summary>
        /// <param name="tableName"></param>
        /// <returns></returns>
        public async Task<Result<Dictionary<string, Node>>> ReadNodesFromDatabase(string tableName)
        {
            Dictionary<string, Node> nodes = new Dictionary<string, Node>();
            try
            {
                List<Node> nodeData = (List<Node>)await _nodeService.GetAllNodesAsync();

                foreach(var node in nodeData)
                {
                    nodes.Add(node.Name, node);
                }

                return Result<Dictionary<string,Node>>.Success(nodes);
            }
            catch (DbException dbException)
            {
                return Result<Dictionary<string, Node>>.Failure(dbException.Message
                    + " " + dbException.InnerException?.Message);
            }
            catch (Exception ex)
            {
                return Result<Dictionary<string, Node>>.Failure(ex.Message
                    + " " + ex.InnerException?.Message);
            }
        }

    }
}
