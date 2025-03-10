using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WCI_SUV.Core.Interface;

using Microsoft.Extensions.Logging;
using System.Text.Json;
using System.Text.Json.Nodes;

namespace WCI_SUV.IO.Services
{
    public class JsonService 
    {
        private readonly ILogger<JsonService> _logger;
        private readonly JsonSerializerOptions _serializer;

        public JsonService(ILogger<JsonService> logger)
        {
            _logger = logger;
            _serializer = new JsonSerializerOptions
            {
                WriteIndented = true
            };
        }

        /// <summary>
        /// Write generic data to JSON File asynchronously 
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="filePath"></param>
        /// <param name="data"></param>
        /// <returns></returns>
        /// <exception cref="ArgumentNullException"></exception>
        public async Task SaveToJsonAsync<T>(string filePath, T data)
        {
            if (string.IsNullOrEmpty(filePath))
            {
                throw new ArgumentNullException(nameof(filePath));
            }

            if (data == null)
            {
                throw new ArgumentNullException(nameof(data));
            }

            if (FileExists(filePath).Result == false)
            {
                await CreateDirectoryIfNotExist(Path.GetDirectoryName(filePath));
            }

            try
            {
                var jsonData = JsonSerializer.Serialize(data, _serializer);
                await File.WriteAllTextAsync(filePath, jsonData);

            }
            catch (JsonException jsonException)
            {
                _logger.LogError(jsonException, "Error while saving data to json file");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error while saving data to json file");
            }
        }

        public async Task<T> LoadFromJsonAsync<T>(string filePath)
        {
            if (string.IsNullOrEmpty(filePath) == true)
            {
                throw new ArgumentNullException(nameof(filePath));
            }

            try
            {
                var data = await File.ReadAllTextAsync(filePath);
                var jsonData = JsonSerializer.Deserialize<T>(data, _serializer);

                if (jsonData == null)
                {
                    throw new JsonException($"Error while deserializing data from file {filePath}");
                }

                return jsonData;
            }
            catch (JsonException jsonException)
            {
                _logger.LogError(jsonException, "Error while saving data to json file");
                return default(T);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error while saving data to json file");
                return default(T);
            }
        }

        public async Task<bool> FileExists(string filePath)
        {
            return await Task.Run(() => File.Exists(filePath));
        }

        public async Task CreateDirectoryIfNotExist(string dirPath)
        {
            if (string.IsNullOrEmpty(dirPath))
            {
                throw new ArgumentNullException(nameof(dirPath));
            }

            await Task.Run(() => Directory.CreateDirectory(dirPath));
        }


    }
}
