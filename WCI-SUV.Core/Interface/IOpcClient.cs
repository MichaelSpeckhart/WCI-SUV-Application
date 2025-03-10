using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.NetworkInformation;
using System.Text;
using System.Threading.Tasks;

using WCI_SUV.Core.Common;

namespace WCI_SUV.Core.Interface
{
    public class WriteResult 
    {
        public string Key { get; set; } = string.Empty;
        public bool isSuccessful { get; set; }
        public string? ErrorMessage { get; set; }
    }

    public interface IOpcClient
    {
        public void Dispose();

        public Task<Result<bool>> ConnectAsync(string endpointUrl);

        public Task<T> ReadNodeAsync<T>(uint registerId, ushort namespacex);

        public Task WriteNodeAsync<T>(uint registerId, ushort namespacex, T value);

        public Task GetNamespaceUri();


    }
}
