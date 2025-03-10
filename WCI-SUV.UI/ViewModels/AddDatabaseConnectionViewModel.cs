using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using WCI_SUV.Core;
using WCI_SUV;

namespace WCI_SUV.UI.ViewModels
{
    public class AddDatabaseConnectionViewModel
    {
        // View Model implementation for "Add Database Connection" dialog

        public string DatabaseName { get; set; }
        public string ServerName { get; set; }
        public string ConnectionString { get; set; }

        public AddDatabaseConnectionViewModel()
        {
            DatabaseName = string.Empty;
            ServerName = string.Empty;
            ConnectionString = string.Empty;
        }

        public void SaveConnection()
        {
            // Write code to interact with WCI-SUV.IO to write database connection to json file

        }





    }
}
