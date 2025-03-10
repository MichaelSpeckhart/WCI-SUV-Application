using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using WCI_SUV.Core.Interface.Scanner;

namespace WCI_SUV.IO.Services.Scanner
{
    public class ScannerService : IScanner
    {

        public async Task<string> ReadAsync(string filePath)
        {
            var text =  File.ReadLinesAsync(filePath);

            return text.ToString();
        }

        public void CreateScannerFile()
        {
            // Create a file with a UUID that will be read from 

        }

    }
} 

