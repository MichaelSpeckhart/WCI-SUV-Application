using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WCI_SUV.Core.Interface.Scanner
{
    public interface IScanner
    {
        // Reads the output of a scanner in text file delimited by a \n

        public Task<string> ReadAsync(string filePath);




    }
}
