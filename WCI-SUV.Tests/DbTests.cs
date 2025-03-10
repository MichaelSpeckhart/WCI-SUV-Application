using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WCI_SUV.Core.Interface.Database;
using WCI_SUV.DB.Data;
using WCI_SUV.DB.Services;

using WCI_SUV.Core.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.SqlServer;
using WCI_SUV.Core.Entities;
using Microsoft.VisualStudio.TestTools.UnitTesting.Logging;

namespace WCI_SUV.Tests
{

    using PlcNode = WCI_SUV.Core.Entities.Node;

    [TestClass]
    public class DbTests
    {

        private ApplicationDbContext _context;
        private NodeEntityService _nodeEntityService;
        private TicketEntityService _ticketEntityService;
        private ILogger<NodeEntityService> _logger;
        private NodeManager _nodeManager;
        private DatabaseService _databaseService;

        private ConveyorEntityService _conveyorEntityService;

        [TestInitialize]
        public async Task Setup()
        {
            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseSqlServer("Data Source=MICHAEL-XPS-13\\SQLEXPRESS;Initial Catalog=compusort_suv;Integrated Security=True;Connect Timeout=30;Encrypt=True;Trust Server Certificate=True;Application Intent=ReadWrite;Multi Subnet Failover=False")
            .Options;

            // Create logger factory with debug output
            var loggerFactory = LoggerFactory.Create(builder =>
            {
                builder.AddConsole();
            });

            _logger = loggerFactory.CreateLogger<NodeEntityService>();
            _context = new ApplicationDbContext(options, skipAudit: true);  // Using the simpler constructor
            _nodeEntityService = new NodeEntityService(_context);
            _ticketEntityService = new TicketEntityService(_context);
            _nodeManager = new NodeManager(_nodeEntityService);
            _databaseService = new DatabaseService(_nodeEntityService, "Nodes");
            _conveyorEntityService = new ConveyorEntityService(_context);
            await _nodeManager.InitializeAsync();

            // Ensure database exists and is up to date
            _context.Database.EnsureCreated();
        }

        [TestMethod]
        public async Task WriteNodesToDatabase()
        {
            var res = await _databaseService.WriteNodesToDatabase("C:\\Users\\msspe\\source\\repos\\WCI-SUV-Application\\WCI-SUV.Tests\\public\\Nodes.txt", "Nodes");
            Assert.IsTrue(res.isSuccess);
        }


        [TestMethod]
        public async Task GetInMemoryNodeTable()
        {
            await _nodeManager.InitializeAsync();

            Assert.IsTrue(_nodeManager.GetNodeCount() > 0);
        }

        [TestMethod]
        public async Task GetNodeByName()
        {
            NodeManager nodeManager = new NodeManager(_nodeEntityService);
            

            var node = await _nodeEntityService.GetNodeByNameAsync("SlotRunning");

            Assert.IsTrue(node.Register == 79);

            Assert.IsNotNull(node);
        }

        [TestMethod]
        public async Task VerifyNodeNamespace()
        {
            var node = await _nodeEntityService.GetNodeByNameAsync("FramePosition");

            Assert.AreEqual(node.Namespace, 1);
        }

        [TestMethod]
        public async Task GetConveyorTable()
        {
            var conveyorTable = await _context.Conveyors.ToListAsync();

            foreach (var conveyor in conveyorTable)
            {
                Console.WriteLine(conveyor.GarmentNumber);
            }

            Assert.IsTrue(conveyorTable.Count > 0);
        }

        [TestMethod]
        public async Task AddTicket()
        {
            Ticket ticket = new Ticket
            {
                TicketNumber = 1234,
                TicketSize = 1,
                SlotNumber = 2,
            };
            await _ticketEntityService.AddTicketAsync(ticket);

        }

        [TestMethod]
        public async Task AddConveyor()
        {
            Conveyor conveyor = new Conveyor
            {
                AccountNumber = 768,
                TicketNumber = 39,
                SlotNumber = 3,
                GarmentNumber = 4,
                EmployeeNumber = 5,
                TicketSize = 1099
            };

            await _conveyorEntityService.AddConveyorAsync(conveyor);

        }


        [TestCleanup]
        public void Cleanup()
        {
            _context.Dispose();
        }

    }
}
