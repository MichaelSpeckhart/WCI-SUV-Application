using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WCI_SUV.Core.Models
{
    public class JsonDatabaseConnections
    {
        public string Name { get; set; }
        public string ConnectionString { get; set; }

        public JsonDatabaseConnections(string name, string connectionString)
        {
            Name = name;
            ConnectionString = connectionString;
        }




    }
}
