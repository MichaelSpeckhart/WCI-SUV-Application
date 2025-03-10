using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System.Windows;
using WCI_SUV.Core.Interface.Database;
using WCI_SUV.Core.Interface.OPC;
using WCI_SUV.Core.Services;
using WCI_SUV.DB.Data;
using WCI_SUV.DB.Services;
using WCI_SUV.IO.Models.OPC;
using WCI_SUV.IO.Services.OPC;
using WCI_SUV.UI.ViewModels;

using System.IO;
using System.Diagnostics;

namespace WCI_SUV.UI
{
    public partial class App : Application
    {
        private readonly ServiceProvider _serviceProvider;

        public App()
        {
            Debug.WriteLine("App is running...");
            Console.WriteLine("App is running....");
            var services = new ServiceCollection();

            // Add logging
            services.AddLogging(configure =>
            {
                configure.AddConsole();
                configure.SetMinimumLevel(LogLevel.Debug); // Ensure we see all logs
                configure.AddDebug(); // Add debug output window logging
            });

            // Register DbContext (ApplicationDbContext)
            services.AddDbContext<ApplicationDbContext>(options =>
                options.UseSqlServer("Data Source=MICHAEL-XPS-13\\SQLEXPRESS;Initial Catalog=compusort_suv;Integrated Security=True;Connect Timeout=30;Encrypt=True;Trust Server Certificate=True;Application Intent=ReadWrite;Multi Subnet Failover=False"));

            // Register services and view models
            services.AddSingleton<MainWindowViewModel>();  // Register MainWindowViewModel

            // Register the services required by MainWindowViewModel
            services.AddSingleton<ConveyorEntityService>();  // ConveyorEntityService
            services.AddSingleton<IOpcService, OpcService>();  // IOpcService (implementation)
            services.AddSingleton<PlcOpcClient>();  // PlcOpcClient

            // Register other services
            services.AddSingleton<NodeManager>();
            services.AddSingleton<INodeEntityService, NodeEntityService>();
            services.AddSingleton<ILoggerFactory, LoggerFactory>();  // Register ILoggerFactory

            services.AddSingleton<ILogger<MainWindowViewModel>>(provider =>
                provider.GetRequiredService<ILoggerFactory>().CreateLogger<MainWindowViewModel>());

            services.AddSingleton<MainWindowViewModel>(provider => new MainWindowViewModel(
                provider.GetRequiredService<ILogger<MainWindowViewModel>>(),
                provider.GetRequiredService<ILoggerFactory>(),
                provider.GetRequiredService<ConveyorEntityService>(),
                provider.GetRequiredService<IOpcService>(),
                provider
            ));

            services.AddSingleton<ILogger<ConveyorControlsViewModel>>(provider =>
                provider.GetRequiredService<ILoggerFactory>().CreateLogger<ConveyorControlsViewModel>());

            services.AddSingleton<ConveyorControlsViewModel>(provider => new ConveyorControlsViewModel(
                provider.GetRequiredService<ILogger<ConveyorControlsViewModel>>(),
                provider.GetRequiredService<ConveyorEntityService>(),
                provider.GetRequiredService<IOpcService>(),
                "opc.tcp://192.168.22.248" // Your OPC server address
            ));

            services.AddSingleton<SuvControlsViewModel>(provider => new SuvControlsViewModel(
                provider.GetRequiredService<ILogger<SuvControlsViewModel>>(),
                provider.GetRequiredService<IOpcService>(),
                "opc.tcp://192.168.22.248"
            ));

            services.AddSingleton<DatabaseTableViewModel>(provider => new DatabaseTableViewModel(
                provider.GetRequiredService<ApplicationDbContext>(),
                provider.GetRequiredService<ILogger<DatabaseTableViewModel>>()
            ));

            // Build the DI container
            _serviceProvider = services.BuildServiceProvider();
        }

        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);

            // Log the container services to ensure everything is registered
            Console.WriteLine("Services Registered:");
            foreach (var service in _serviceProvider.GetServices<object>())
            {
                Console.WriteLine(service.GetType().Name);
            }

            // Try to resolve MainWindowViewModel from DI container
            var mainWindowViewModel = _serviceProvider.GetService<MainWindowViewModel>();

            if (mainWindowViewModel == null)
            {
                throw new Exception("Failed to resolve MainWindowViewModel.");
            }

            // Instantiate MainWindow and pass MainWindowViewModel to its constructor
            var mainWindow = new MainWindow(mainWindowViewModel);
            mainWindow.Show();
        }

        protected override void OnExit(ExitEventArgs e)
        {
            // Get the OPC service and disconnect
            var opcService = _serviceProvider.GetService<IOpcService>();
            opcService?.Dispose();

            base.OnExit(e);
        }
    }


}
