using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WCI_SUV.Core.Interface.Database;
using WCI_SUV.Core.Services;
using WCI_SUV.DB.Data;
using WCI_SUV.DB.Services;
using WCI_SUV.IO.Models.OPC;
using WCI_SUV.IO.Services.OPC;

namespace WCI_SUV.Tests
{
    [TestClass]
    public class IoTests
    {

        private OpcService _opcService;
        private PlcOpcClient _opcClient;
        private ILogger<OpcService> _opcLogger;
        private NodeEntityService _nodeService;
        private NodeManager _nodeManager;
        private ApplicationDbContext _context;

        private ILogger<PlcOpcClient> GetLogging()
        {
            var serviceCollection = new ServiceCollection();

            
            serviceCollection.AddLogging(config =>
            {
                config.AddConsole(); 
                config.SetMinimumLevel(LogLevel.Debug); 
            });

            
            var serviceProvider = serviceCollection.BuildServiceProvider();

            
            var logger = serviceProvider.GetRequiredService<ILogger<PlcOpcClient>>();

            return logger;
        }

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

            _nodeManager = new NodeManager(_nodeService);

            await _nodeManager.InitializeAsync();

            _opcLogger = loggerFactory.CreateLogger<OpcService>();

            _opcService = new OpcService(_opcClient, _nodeManager);

        }


        [TestMethod]
        public async Task RunConveyor()
        {
           
        }

        [TestMethod]
        public async Task TestSlowConveyorSpeed()
        {
            
            var node = await _nodeService.GetNodeByNameAsync("ConveyorSpeed");

            bool isConnected = await _opcService.ConnectToServer("opc.tcp://192.168.22.248");

            if (isConnected == false)
            {
                throw new Exception("Not Connected");
            }

           
                await _opcService.RunConveyor();


                Task.Delay(100000).Wait();

                await _opcService.SlowConveyorSpeed();

                Task.Delay(100000).Wait();

                await _opcService.StopConveyor();

                _opcService.Dispose();
            






        }


        //[TestMethod]
        //public async Task TestGetNamespaceUri()
        //{


        //    PlcOpcClient client = new PlcOpcClient(GetLogging());

        //    await client.ConnectAsync("opc.tcp://192.168.22.248");

        //    if (client.IsConnected)
        //    {
        //        var res = await client.BrowseGlobalVariables();
                
        //       var nodes =  await client.BrowseRegistersInGlobalVars<string,string,string>(res);

        //        foreach (var node in nodes)
        //        {
                    
        //            Console.WriteLine($"Name = {node.Item1}, Register = {node.Item2}, Type = {node.Item3}");
        //        }
        //    } else
        //    {
        //        throw new Exception("Not Connected");
        //    }

        //    client.Dispose();

        //}

    }
}
