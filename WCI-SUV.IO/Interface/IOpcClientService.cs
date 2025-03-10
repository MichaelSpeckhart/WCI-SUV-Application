using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WCI_SUV.Core.Interface
{
    public interface IOpcClientService
    {
        Task ConnectAsync();
        Task DisconnectAsync();
        Task<T> ReadTagAsync<T>(string tagName);
        Task WriteTagAsync<T>(string tagName, T value);
        Task SubscribeToTagAsync(string tagName, Action<object> callback);

    }
}
