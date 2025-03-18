using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WCI_SUV.Core.Interface;
using WCI_SUV.Core.Services;
using WCI_SUV.DB.Data;
using WCI_SUV.DB.Services;
using WCI_SUV.IO.Models.OPC;
using WCI_SUV.IO.Services.OPC;
using WCI_SUV.Core.Entities;

namespace WCI_SUV.Tests
{
    [TestClass]
    public class TicketTests
    {
        #region Private Fields
        private OpcService _opcService;
        private PlcOpcClient _opcClient;
        private ILogger<OpcService> _opcLogger;
        private NodeEntityService _nodeService;
        private NodeManager _nodeManager;
        private ApplicationDbContext _context;

        private TicketEntityService _ticketEntityService;

        private TicketCache _ticketCache;
        private TicketProcessor _ticketProcessor;
        private ConveyorCache _conveyorCache;

        #endregion


        #region Test Initialization
        [TestInitialize]
        public async Task Setup()
        {
            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseSqlServer("Data Source=MICHAEL-XPS-13\\SQLEXPRESS;Initial Catalog=compusort_suv;Integrated Security=True;Connect Timeout=30;Encrypt=True;Trust Server Certificate=True;Application Intent=ReadWrite;Multi Subnet Failover=False")
            .Options;

            _opcClient = new PlcOpcClient();
            var loggerFactory = LoggerFactory.Create(builder =>
            {
                builder.AddConsole();
            });

            _context = new ApplicationDbContext(options, skipAudit: true);  // Using the simpler constructor

            _nodeService = new NodeEntityService(_context);

            _ticketEntityService = new TicketEntityService(_context);

            _nodeManager = new NodeManager(_nodeService);

            await _nodeManager.InitializeAsync();

            _opcLogger = loggerFactory.CreateLogger<OpcService>();

            _opcService = new OpcService(_opcClient, _nodeManager);

            bool isConnected = await _opcService.ConnectToServer("opc.tcp://192.168.22.248");

            if (isConnected == false)
            {
                throw new Exception("Failed to connect to OPC server");
            }

            _ticketCache = new TicketCache(_ticketEntityService);

            await _ticketCache.LoadTicketsAsync();

            _conveyorCache = new ConveyorCache();

            _ticketProcessor = new TicketProcessor(_ticketCache, _conveyorCache, _opcService);

            await _ticketProcessor.InitializeAsync();
        }

        #endregion


        #region Test Methods


        [TestMethod]
        public void TestTicketCache()
        {
            var ticketList = _ticketCache.GetAllTickets();

            foreach (var ticket in ticketList)
            {
                Console.WriteLine($"Ticket {ticket.TicketNumber} - Slot: {ticket.SlotNumber}");
            }

            Assert.IsTrue(ticketList.Count() > 0);
        }

        [TestMethod]
        public async Task TestTicketProcessor()
        {
            Ticket newTicket = new Ticket
            {
                TicketNumber = 1234,
                SlotNumber = 2,
            };

            await _ticketProcessor.ProcessTicket(newTicket.TicketNumber,(Int16) newTicket.SlotNumber);

            _ticketCache.GetAllTickets();

            Assert.IsTrue(_ticketCache.GetTicket(newTicket.TicketNumber) != null);
        }

        [TestMethod]
        public async Task TestTicketSlot()
        {
            Int32 ticketNumber = 1989;

            var currentSlotRes = await _opcService.GetCurrentSlotNumber();

            var currentSlot = currentSlotRes.Value; 

            Console.WriteLine($"Current Slot: {currentSlot}");

            var nextAvailableSlot = _conveyorCache.GetClosestAvailableSlot(currentSlot);

            Console.WriteLine($"Next Available Slot: {nextAvailableSlot}");

            await _opcService.SetTargetSlot((short)80);

            await _opcService.GoToTargetSlot();

            _opcService.Dispose();

        }

        #endregion





    }
}
