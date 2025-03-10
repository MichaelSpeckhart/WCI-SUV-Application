using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

using WCI_SUV.UI.ViewModels;

namespace WCI_SUV.UI.Views
{
    /// <summary>
    /// Interaction logic for DatabaseTableView.xaml
    /// </summary>
    public partial class DatabaseTableView : Page
    {
        public DatabaseTableView(DatabaseTableViewModel dbViewModel)
        {
            
            InitializeComponent();
            DataContext = dbViewModel;
            Unloaded += async (s, e) =>
            {
                if (DataContext is DatabaseTableViewModel vm)
                {
                    await vm.CleanupAsync();
                }
            };
        }

        


    }

    
}
