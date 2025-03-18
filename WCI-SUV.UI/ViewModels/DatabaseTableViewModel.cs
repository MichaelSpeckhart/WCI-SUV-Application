using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using WCI_SUV.DB.Data;
using WCI_SUV.UI.Commands;

namespace WCI_SUV.UI.ViewModels
{
    public class DatabaseTableViewModel : INotifyPropertyChanged
    {
        public enum SortCommands
        {
            Ascending,
            Descending,
            Recent,
            Oldest
        }

        #region Private Fields
        private string _tableName               { get; set; }
        private string _columnName              { get; set; }
        private List<string> _columns           { get; set; }
        private ApplicationDbContext _dbContext { get; set; }

        private readonly ILogger<DatabaseTableViewModel> _logger;
        private string _statusMessage;
        private string _selectedTableName = string.Empty;
        private DataTable _dataTable;
        private string _searchText;
        private int _rowCount;
        private int _filteredRowCount;
        private DateTime _lastRefreshTime;
        private string _databaseConnInfo;
        private object _selectedEntry;
        #endregion

        #region Table Name Collection
        public ObservableCollection<string> TableNames { get; set; } = new ObservableCollection<string>
        {
            "Conveyor",
            "Node",
            "Ticket"
        };
        #endregion

        #region Public Fields
        public string SelectedTable
        {
            get => _selectedTableName;
            set
            {
                if (_selectedTableName != value)
                {
                    _selectedTableName = value;
                    OnPropertyChanged(nameof(SelectedTable));

                    if (SelectTableCommand.CanExecute(null))
                    {
                        SelectTableCommand.Execute(null); // Executes the command when selection changes
                    }
                }
            }
        }

        public DataTable TableData
        {
            get => _dataTable;
            set
            {
                if (_dataTable != value)
                {
                    _dataTable = value;
                    OnPropertyChanged(nameof(TableData));

                    // Update filtered data and counts
                    ApplyFilter();

                    // Update row counts
                    RowCount = _dataTable?.Rows.Count ?? 0;
                    LastRefreshTime = DateTime.Now;
                }
            }
        }

        public string SearchText
        {
            get => _searchText;
            set
            {
                if (_searchText != value)
                {
                    _searchText = value;
                    OnPropertyChanged(nameof(SearchText));
                    ApplyFilter();
                }
            }
        }

        public int RowCount
        {
            get => _rowCount;
            set
            {
                if (_rowCount != value)
                {
                    _rowCount = value;
                    OnPropertyChanged(nameof(RowCount));
                }
            }
        }

        public int FilteredRowCount
        {
            get => _filteredRowCount;
            set
            {
                if (_filteredRowCount != value)
                {
                    _filteredRowCount = value;
                    OnPropertyChanged(nameof(FilteredRowCount));
                }
            }
        }

        public DateTime LastRefreshTime
        {
            get => _lastRefreshTime;
            set
            {
                if (_lastRefreshTime != value)
                {
                    _lastRefreshTime = value;
                    OnPropertyChanged(nameof(LastRefreshTime));
                }
            }
        }

        public string DatabaseConnInfo
        {
            get => _databaseConnInfo;
            set
            {
                if (_databaseConnInfo != value)
                {
                    _databaseConnInfo = value;
                    OnPropertyChanged(nameof(DatabaseConnInfo));
                }
            }
        }

        public DataTable FilteredTableData
        {
            get => _dataTable; // For now, just return the same table until we implement filtering
        }

        public object SelectedEntry
        {
            get => _selectedEntry;

            set
            {
                _selectedEntry = value;
                OnPropertyChanged(nameof(SelectedEntry));


            }
        }

        #endregion

        // Handle choosing which table
        public DatabaseTableViewModel(
            ApplicationDbContext dbContext,
            ILogger<DatabaseTableViewModel> logger)
        {
            _dbContext = dbContext;
            _columns = new List<string>();

            #region Initialize strings
            _tableName = string.Empty;
            _columnName = string.Empty;
            _lastRefreshTime = DateTime.Now;
            #endregion

            _logger = logger;

            InitializeCommands();
        }

        #region Async Initialization
        public async Task InitializeAsync()
        {
            await Task.Yield(); // Allows UI to refresh before execution starts

            try
            {
                var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                            .UseSqlServer("Data Source=MICHAEL-XPS-13\\SQLEXPRESS;Initial Catalog=compusort_suv;Integrated Security=True;Connect Timeout=30;Encrypt=True;Trust Server Certificate=True;Application Intent=ReadWrite;Multi Subnet Failover=False")
                            .Options;

                _dbContext = new ApplicationDbContext(options, skipAudit: true);  // Using the simpler constructor
                // Ensure database exists and is up to date
                _dbContext.Database.EnsureCreated();
                // Initialize the database context
                await _dbContext.Database.MigrateAsync();
                // Check if connected
                var isConnected = await _dbContext.Database.CanConnectAsync();
                if (!isConnected)
                {
                    _logger.LogError("Failed to connect to database");
                    StatusMessage = "Failed to connect to database";
                }
                else
                {
                    _logger.LogInformation("Connected to Microsoft SQL Server.");
                    StatusMessage = "Connected to Microsoft SQL Server.";
                    DatabaseConnInfo = "Connected to: Microsoft SQL Server";

                    // Automatically load first table if none selected
                    //if (string.IsNullOrEmpty(SelectedTable) && TableNames.Count > 0)
                    //{
                    //    SelectedTable = TableNames[0];
                    //}
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error initializing Microsoft SQL Server connection.");
                StatusMessage = $"Error: {ex.Message}";
            }
        }
        #endregion

        #region Properties
        public string StatusMessage
        {
            get => _statusMessage;
            set { _statusMessage = value; OnPropertyChanged(nameof(StatusMessage)); }
        }
        #endregion

        public async Task CleanupAsync()
        {
            try
            {
                await _dbContext.DisposeAsync();
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                // Log the exception
                // _logger.LogError($"In function 'CleanupAsync', exception occured: {ex.Message}");
            }
        }

        #region Commands
        public ICommand SelectTableCommand  { get; private set; }
        public ICommand SelectSortCommand   { get; private set; }
        public ICommand RefreshCommand      { get; private set; }
        public ICommand ExportCommand       { get; private set; }
        public ICommand AddEntryCommand     { get; private set; }
        public ICommand EditEntryCommand    { get; private set; }
        public ICommand DeleteEntryCommand  { get; private set; }
        public ICommand SearchTextCommand   { get; private set; }

        private void InitializeCommands()
        {
            SelectTableCommand  =    new RelayCommand(async _ => await SelectTable(), _ => true);
            RefreshCommand      =    new RelayCommand(async _ => await LoadTableData(), _ => !string.IsNullOrEmpty(SelectedTable));
            ExportCommand       =    new RelayCommand(_ => ExportTable(), _ => TableData != null && TableData.Rows.Count > 0);
            AddEntryCommand     =    new RelayCommand(async _ => await AddRecord(), _ => !string.IsNullOrEmpty(SelectedTable));
        }
        #endregion

        #region INotifyPropertyChanged
        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
        #endregion

        #region Command Methods 

        public async Task AddRecord()
        {
            if (string.IsNullOrEmpty(SelectedTable)) return;

            try
            {
                _logger.LogInformation("Adding new entry to table.");
                StatusMessage = "Adding new entry to table.";

                // Implement logic to add new entry to table
                // For now, just refresh the table
            }
            catch (Exception ex)
            {
                StatusMessage = $"Error adding entry: {ex.Message}";
                _logger.LogError(ex, "Error adding entry.");
            }
        }


        public async Task SelectTable()
        {
            if (string.IsNullOrEmpty(SelectedTable))
                return;
            try
            {
                await LoadTableData();
            }
            catch (Exception ex)
            {
                StatusMessage = $"Error selecting table: {ex.Message}";
                _logger.LogError(ex, "Error selecting table.");
            }
        }

        private void ExportTable()
        {
            // Implement CSV export logic here
            StatusMessage = "Export functionality not yet implemented";
        }

        private void ApplyFilter()
        {
            if (TableData == null || string.IsNullOrWhiteSpace(SearchText))
            {
                // If no filter or no data, just show everything
                FilteredRowCount = RowCount;
                OnPropertyChanged(nameof(FilteredTableData));
                return;
            }

            try
            {
                // Simple filtering can be implemented using DefaultView.RowFilter
                // For now, we'll just update the FilteredRowCount
                FilteredRowCount = RowCount;
                OnPropertyChanged(nameof(FilteredTableData));

                // Future implementation could use:
                // TableData.DefaultView.RowFilter = "[ColumnName] LIKE '%" + SearchText + "%'";
            }
            catch (Exception ex)
            {
                StatusMessage = $"Error applying filter: {ex.Message}";
                _logger.LogError(ex, "Error applying filter.");
            }
        }

      



        #endregion

        #region Load Table Data
        public async Task LoadTableData()
        {
            if (string.IsNullOrEmpty(SelectedTable))
                return;

            try
            {
                DataTable dataTable = new DataTable();

                if (SelectedTable == "Conveyor")
                {
                    var items = await _dbContext.Conveyors.ToListAsync();
                    dataTable = ConvertToDataTable(items);
                }
                else if (SelectedTable == "Node")
                {
                    var items = await _dbContext.Nodes.ToListAsync();
                    dataTable = ConvertToDataTable(items);
                }
                else if (SelectedTable == "Ticket")
                {
                    var items = await _dbContext.Tickets.ToListAsync();
                    dataTable = ConvertToDataTable(items);
                }

                TableData = dataTable;
                StatusMessage = $"Loaded {SelectedTable} successfully with {dataTable.Rows.Count} rows.";
                _logger.LogInformation($"Loaded {SelectedTable} into DataTable.");

                // Make sure to trigger property change for related properties
                OnPropertyChanged(nameof(FilteredTableData));
                OnPropertyChanged(nameof(TableData.DefaultView));
            }
            catch (Exception ex)
            {
                StatusMessage = $"Error loading table data: {ex.Message}";
                _logger.LogError(ex, "Error loading table data.");
            }
        }

        private DataTable ConvertToDataTable<T>(List<T> items)
        {
            DataTable dataTable = new DataTable(typeof(T).Name);

            if (items == null || !items.Any())
                return dataTable;

            // Get properties of T
            var properties = typeof(T).GetProperties();

            // Create columns in DataTable
            foreach (var prop in properties)
            {
                dataTable.Columns.Add(prop.Name, Nullable.GetUnderlyingType(prop.PropertyType) ?? prop.PropertyType);
            }

            // Populate DataTable rows
            foreach (var item in items)
            {
                var values = properties.Select(prop => prop.GetValue(item, null)).ToArray();
                dataTable.Rows.Add(values);
            }

            return dataTable;
        }
        #endregion
    }
}