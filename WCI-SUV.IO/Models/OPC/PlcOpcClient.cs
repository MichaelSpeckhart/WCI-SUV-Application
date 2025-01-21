

using Opc.Ua;
using Opc.UaFx;
using Opc.UaFx.Client;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;



namespace WCI_SUV.IO.Models.OPC
{

    public class PlcOpcClient
    {
        private string _serverUrl;
        private OpcClient _client;

        public PlcOpcClient(string serverUrl)
        {
            _serverUrl = serverUrl;
            _client = new OpcClient(serverUrl);
        }

        public void Connect()
        {
            try
            {
                _client.Connect();
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
            }
        }

        public void Disconnect()
        {
            try
            {
                _client.Disconnect();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Failed to disconnect {ex.ToString()}");
            }
        }

        public void Write<T>(string node, T value)
        {
            try
            {
                var statusCode = _client.WriteNode(node, value);

                if (statusCode.IsBad)
                {
                    Console.WriteLine($"Error: Status code = {statusCode.ToString()}");
                    return;
                }
                

            }
            catch (OpcException opcException)
            {
                Console.WriteLine($"opcException: {opcException.Message}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Exception: {ex.Message}");  
            }
        }

        public T? Read<T>(string node)
        {
            try
            {
                var value = _client.ReadNode(node);

                if (value.Status.IsBad)
                {
                    Console.WriteLine($"Error: Status Code = {value.Status.Code}");
                    return default;
                }

                return value.As<T>();

            } catch (OpcException opcException)
            {
                Console.WriteLine($"opcException: {opcException.Message}");
                return default;
            } catch (Exception ex)
            {
                Console.WriteLine($"Exception: {ex.Message}");
                return default;
            }
        }


        public Dictionary<string, T?>? BatchRead<T>(IEnumerable<string> nodes)
        {
            var results = new Dictionary<string, T?>();

            try
            {
                for (int i = 0; i < nodes.Count(); ++i)
                {
                    var res = _client.ReadNode(nodes.ElementAt(i));

                    if (res.Status.IsBad)
                    {
                        Console.WriteLine($"Read Error: {res.Status.Code.ToString()}");

                        results.Add(nodes.ElementAt(i), default);
                    }
                    else
                    {
                        results.Add(nodes.ElementAt(i), res.As<T>());
                    }
                }

                return results;
            }
            catch (OpcException opcException)
            {
                Console.WriteLine($"OpcException: {opcException.Message}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Exception: {ex.Message} {ex.StackTrace}");
            }

            return results;
        }

        public Dictionary<string, OpcStatus>? BatchWrite<T>(Dictionary<string, T> values)
        {
            var results = new Dictionary<string, OpcStatus>();

            try
            {
                
                var keyList = values.Keys.ToList();
                var valList = values.Values.ToList();


                for (int i = 0; i < values.Count(); ++i)
                {
                    var result = _client.WriteNode(keyList[i], valList[i]);

                    results.Add(keyList[i], result);
                }

                return results;
            }
            catch (OpcException opcException)
            {
                Console.WriteLine($"OpcException: {opcException.Message}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Exception: {ex.Message} {ex.StackTrace}");
            }

            return results;
        }
        


    }


}
