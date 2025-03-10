using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WCI_SUV.Core.Interface
{
    public interface IJsonService
    {
        Task SaveToJsonAsync<T>(string filePath, T data);
        Task<T> LoadFromJsonAsync<T>(string filePath);
        Task<bool> FileExists(string filePath);
        Task CreateDirectoryIfNotExist(string dirPath);

    }
}
