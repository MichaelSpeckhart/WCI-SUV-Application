using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;



using Opc.Ua;
using System.Collections;
using PlcNode = WCI_SUV.Core.Entities.Node;
using OpcNode = Opc.Ua.Node;
using Newtonsoft.Json.Linq;

#region Internal Packages
using WCI_SUV.Core.Services;
using WCI_SUV.Core.Entities;
using WCI_SUV.Core.Common;

using WCI_SUV.Core.Interface;
using WCI_SUV.Core.Interface.OPC;
using WCI_SUV.IO.Models.OPC;
#endregion 

namespace WCI_SUV.IO.Services.OPC
{
    public class OpcService : IOpcService
    {
        #region Private Fields
        private Dictionary<string, Type> _nodeTypes = new Dictionary<string, Type>();
        private readonly PlcOpcClient _clientService;
        private readonly NodeManager _nodeManager;
        #endregion

        #region OpcService Constructor
        public OpcService(PlcOpcClient client, NodeManager nodeManager)
        {
            _clientService = client;
            _nodeManager = nodeManager;
            InitializeNodeTypes();
        }
        #endregion

        #region Node Types Initialization
        private void InitializeNodeTypes()
        {
            _nodeTypes.Add("Boolean", typeof(bool));
            _nodeTypes.Add("Byte", typeof(byte));
            _nodeTypes.Add("SByte", typeof(sbyte));
            _nodeTypes.Add("Int16", typeof(Int16));
            _nodeTypes.Add("UInt16", typeof(UInt16));
            _nodeTypes.Add("Int32", typeof(Int32));
            _nodeTypes.Add("UInt32", typeof(UInt32));
            _nodeTypes.Add("Int64", typeof(Int64));
            _nodeTypes.Add("UInt64", typeof(UInt64));
            _nodeTypes.Add("Float", typeof(float));
            _nodeTypes.Add("Double", typeof(double));
            _nodeTypes.Add("String", typeof(string));

        }

        #endregion
        public async Task<bool> ConnectToServer(string endpoint)
        {
            var res = await _clientService.ConnectAsync(endpoint);

            return res.isSuccess;
        }

        public async Task<Result<Dictionary<string, string>>> GetNodesValues()
        {
            if (_clientService.IsConnected == false)
            {
                Console.WriteLine($"Error: {_clientService.IsConnected}");
                return Result<Dictionary<string, string>>.Failure("Not Connected");
            }

            try
            {

                return Result<Dictionary<string, string>>.Failure(string.Empty);



            }
            catch (ServiceResultException serviceException)
            {
                Console.WriteLine($"ServiceResultException: {serviceException.Message}");
                return Result<Dictionary<string, string>>.Failure(serviceException.Message);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Exception: {ex.Message}");
                return Result<Dictionary<string, string>>.Failure(ex.Message);
            }
        }

        public async Task<Result<int>> GetCurrentSlotNumber()
        {
            if (_clientService.IsConnected == false)
            {
                Console.WriteLine($"Error: {_clientService.IsConnected}");
                return Result<int>.Failure("Not Connected");
            }

            var nodeToRead = await _nodeManager.GetNode(ConveyorNodes.CURRENT_SLOT);

            try
            {
                var val = await _clientService.ReadNodeAsync<int>(nodeToRead.Register,(ushort) nodeToRead.Namespace);
                return Result<int>.Success(val);
            }
            catch (ServiceResultException serviceException)
            {
                Console.WriteLine($"ServiceResultException: {serviceException.Message}");
                return Result<int>.Failure(serviceException.Message);
            }
        }

        public async Task<Result<object>> GetNodeValue(PlcNode nodeId)
        {
            if (_clientService.IsConnected == false)
            {
                Console.WriteLine($"Error: {_clientService.IsConnected}");
                return Result<object>.Failure("Not Connected");
            }

            try
            {

                var value = await _clientService.ReadNodeAsync<object>(nodeId.Register, (ushort)nodeId.Namespace);

                return Result<object>.Success(value);

            }
            catch (ServiceResultException serviceException)
            {
                Console.WriteLine($"ServiceResultException: {serviceException.Message}");
                return Result<object>.Failure(serviceException.Message);
            }
        }

        public async Task<Result<bool>> WriteNodeValue<T>(uint node, T value)
        {
            var nodeToWrite = new PlcNode
            {
                Register = node,
                Namespace = 1,
            };

            return await SetNodeValue<T>(nodeToWrite, value);
        }

        public async Task<Result<bool>> SetNodeValue<T>(PlcNode nodeToWrite, T value)
        {
            if (_clientService.IsConnected == false)
            {
                Console.WriteLine($"Error: {_clientService.IsConnected}");
                return Result<bool>.Failure("Not Connected");
            }

            try
            {

                await _clientService.WriteNodeAsync<T>(nodeToWrite.Register,(ushort)nodeToWrite.Namespace, value);
                return Result<bool>.Success(true);


            }
            catch (ServiceResultException serviceException)
            {
                Console.WriteLine($"ServiceResultException: {serviceException.Message}");
                return Result<bool>.Failure(serviceException.Message);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Exception: {ex.Message}");
                return Result<bool>.Failure(ex.Message);
            }

        }

        public async Task<Result<bool>> SlowConveyorSpped()
        {
            if (_clientService.IsConnected == false)
            {
                Console.WriteLine($"Error: {_clientService.IsConnected}");
                return Result<bool>.Failure("Not Connected");
            }

            try
            {
                var runNode     = await _nodeManager.GetNode(ConveyorNodes.CONVEYOR_SPEED);
                var creepNode   = await _nodeManager.GetNode(ConveyorNodes.CREEP_SPEED);
                var currNode    = await _nodeManager.GetNode(ConveyorNodes.CURR_SPEED);


                await _clientService.WriteNodeAsync(currNode.Register, (ushort)currNode.Namespace, 10);

                return Result<bool>.Success(true);
            }
            catch (ServiceResultException serviceException)
            {
                Console.WriteLine($"ServiceResultException: {serviceException.Message}");
                return Result<bool>.Failure(serviceException.Message);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Exception: {ex.Message}");
                return Result<bool>.Failure(ex.Message);
            }
        }

        public async Task<Result<bool>> SetTargetSlot(Int16 targetSlot)
        {
            if (_clientService.IsConnected == false)
            {
                Console.WriteLine($"Error: {_clientService.IsConnected}");
                return Result<bool>.Failure("Not Connected");
            }
            try
            {

                var node = await  _nodeManager.GetNode(ConveyorNodes.TARGET_SLOT);

                if (node == null)
                {
                    throw new ArgumentNullException("Node is null");
                }

                await _clientService.WriteNodeAsync(node.Register ,(ushort) node.Namespace, targetSlot);
                return Result<bool>.Success(true);


            }
            catch (ServiceResultException serviceException)
            {
                Console.WriteLine($"ServiceResultException: {serviceException.Message}");
                return Result<bool>.Failure(serviceException.Message);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Exception: {ex.Message}");
                return Result<bool>.Failure(ex.Message);
            }
        }

        public async Task<Result<Int16>> GetTargetSlot()
        {
            Int16 slotValue;
            try
            {
                slotValue = await _clientService.ReadNodeAsync<Int16>(1, 2);
                return Result<Int16>.Success(slotValue);
            }
            catch (ServiceResultException serviceException)
            {
                Console.WriteLine($"ServiceResultException: {serviceException.Message}");
                return Result<Int16>.Failure(serviceException.Message);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Exception: {ex.Message}");
                return Result<Int16>.Failure(ex.Message);
            }
        }

        public async Task<Result<bool>> GoToTargetSlot()
        {
            try
            {
                Int16 currentSlot = await _clientService.ReadNodeAsync<Int16>(1, 3);
                Int16 targetSlot = await _clientService.ReadNodeAsync<Int16>(1, 2);
                if (currentSlot == targetSlot)
                {
                    return Result<bool>.Success(true);
                }

                // Run the conveyor
                await RunConveyor();

                do
                {
                    targetSlot = await _clientService.ReadNodeAsync<Int16>(1, 2);

                    if (Math.Abs(targetSlot - currentSlot) <= 15)
                    {
                        // Slow Conveyor Down;
                        SlowConveyorSpeed();
                    } else if (Math.Abs(targetSlot - currentSlot) == 0)
                    {
                        await StopConveyor();
                    }

                } while (currentSlot != targetSlot);

                return Result<bool>.Success(true);  


            } 
            catch (ServiceResultException serviceException)
            {
                Console.WriteLine($"ServiceResultException: {serviceException.Message}");
                return Result<bool>.Failure(serviceException.Message);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Exception: {ex.Message}");
                return Result<bool>.Failure(ex.Message);
            }
        }


        public async Task<Result<bool>> ReverseConveyorDirection()
        {
            try
            {
                await _clientService.WriteNodeAsync(80, 1, true);
                return Result<bool>.Success(true);
            }
            catch (ServiceResultException serviceException)
            {
                Console.WriteLine($"ServiceResultException: {serviceException.Message}");
                return Result<bool>.Failure(serviceException.Message);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Exception: {ex.Message}");
                return Result<bool>.Failure(ex.Message);
            }
        }

            // Conveyor Control Methods
            #region Conveyor Controls

        public async Task<Result<bool>> StopConveyor()
        {
            try
            {
                var node = await _nodeManager.GetNode(ConveyorNodes.CONVEYOR_RUN);

                if (node == null) throw new ArgumentNullException("Node is null");

                await _clientService.WriteNodeAsync(node.Register, (ushort)node.Namespace, false);
                return Result<bool>.Success(true);
            }
            catch (ServiceResultException serviceException)
            {
                Console.WriteLine($"ServiceResultException: {serviceException.Message}");
                return Result<bool>.Failure(serviceException.Message);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Exception: {ex.Message}");
                return Result<bool>.Failure(ex.Message);
            }
        }


        public async Task<Result<bool>> RunConveyor()
        {
            try
            {
                var node = await _nodeManager.GetNode(ConveyorNodes.CONVEYOR_RUN);
                if (node == null) throw new ArgumentNullException("Node is null");

                string logPath = "C:\\Users\\msspe\\source\\repos\\WCI-SUV-Application\\node_registers.log";
                string nodeInfo = $"Timestamp: {DateTime.Now}\n" +
                                 $"Node Register: {node.Register}\n" +
                                 $"Node Namespace: {node.Namespace}\n" +
                                 $"Node Type: {node.GetType().Name}\n" +
                                 $"Operation: RunConveyor\n" +
                                 "----------------------------------------\n";
                try
                {
                    File.WriteAllText(logPath, nodeInfo);  // Try synchronous write instead
                    File.AppendAllText(logPath, "Write succeeded\n");  // Additional verification
                }
                catch (Exception fileEx)
                {
                    File.WriteAllText("C:\\Users\\msspe\\Desktop\\error_log.txt",
                        $"Failed to write node log: {fileEx.Message}\n{fileEx.StackTrace}");
                }

                await _clientService.WriteNodeAsync(node.Register, (ushort)node.Namespace, true);
                return Result<bool>.Success(true);
            }
            catch (ServiceResultException serviceException)
            {
                File.WriteAllText("C:\\Users\\msspe\\Desktop\\error_log.txt",
                    $"ServiceResultException: {serviceException.Message}\n{serviceException.StackTrace}");
                return Result<bool>.Failure(serviceException.Message);
            }
            catch (Exception ex)
            {
                File.WriteAllText("C:\\Users\\msspe\\Desktop\\error_log.txt",
                    $"General Exception: {ex.Message}\n{ex.StackTrace}");
                return Result<bool>.Failure(ex.Message);
            }
        }


        public async Task<Result<bool>> SlowConveyorSpeed()
        {
            try
            {
                Int16 currSpeed = await _clientService.ReadNodeAsync<Int16>(1, 6);
                await _clientService.WriteNodeAsync(1, 6, (Int16)(currSpeed / 2));
                return Result<bool>.Success(true);
            }
            catch (ServiceResultException serviceException)
            {
                Console.WriteLine($"ServiceResultException: {serviceException.Message}");
                return Result<bool>.Failure(serviceException.Message);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Exception: {ex.Message}");
                return Result<bool>.Failure(ex.Message);
            }
        }


        public async Task<Result<bool>> AccelerateConveyorSpeed()
        {
            try
            {
                Int16 currSpeed = await _clientService.ReadNodeAsync<Int16>(1, 6);
                await _clientService.WriteNodeAsync(1, 6, (Int16)(currSpeed * 2));
                return Result<bool>.Success(true);
            }
            catch (ServiceResultException serviceException)
            {
                Console.WriteLine($"ServiceResultException: {serviceException.Message}");
                return Result<bool>.Failure(serviceException.Message);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Exception: {ex.Message}");
                return Result<bool>.Failure(ex.Message);
            }
        }


        public async Task<Result<bool>> SetConveyorSpeed(Int16 newSpeed)
        {
            try
            {
                await _clientService.WriteNodeAsync(1, 6, newSpeed);
                return Result<bool>.Success(true);
            }
            catch (ServiceResultException serviceException)
            {
                Console.WriteLine($"ServiceResultException: {serviceException.Message}");
                return Result<bool>.Failure(serviceException.Message);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Exception: {ex.Message}");
                return Result<bool>.Failure(ex.Message);
            }
        }

        public async Task<Result<bool>> ConveyorJogFoward()
        {
            try
            {
                var node = await _nodeManager.GetNode(ConveyorNodes.CONVEYOR_FWRD);

                if (node == null)
                {
                    return Result<bool>.Failure("Node does not exist");
                }

                await _clientService.WriteNodeAsync(node.Register, (ushort)node.Namespace, true);
                return Result<bool>.Success(true);

            }
            catch (ServiceResultException serviceException)
            {
                Console.WriteLine($"ServiceResultException: {serviceException.Message}");
                return Result<bool>.Failure(serviceException.Message);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Exception: {ex.Message}");
                return Result<bool>.Failure(ex.Message);
            }
        }

        public async Task<Result<bool>> ConveyorJogReverse()
        {
            try
            {
                var node = await _nodeManager.GetNode(ConveyorNodes.CONVEYOR_REV);

                if (node == null)
                {
                    return Result<bool>.Failure("Node does not exist");
                }

                await _clientService.WriteNodeAsync(node.Register, (ushort)node.Namespace, true);
                return Result<bool>.Success(true);

            }
            catch (ServiceResultException serviceException)
            {
                Console.WriteLine($"ServiceResultException: {serviceException.Message}");
                return Result<bool>.Failure(serviceException.Message);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Exception: {ex.Message}");
                return Result<bool>.Failure(ex.Message);
            }
        }

        public bool Dispose()
        {
            try
            {
                _clientService.Dispose();
                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Exception: {ex.Message}");
                return false;
            }
        }


        private void ConstructWriteRequest(string command, string ns, string i, string type ,string value)
        {
            if (string.IsNullOrEmpty(command) || string.IsNullOrEmpty(ns) || string.IsNullOrEmpty(i) || string.IsNullOrEmpty(value))
            {
                Console.WriteLine("Error: Invalid Command");
                return;
            }

            var node = new NodeId(ushort.Parse(i), ushort.Parse(ns));

            Type typeToWrite = _nodeTypes[type];



        }


        private Tuple<Int16,Int16> ParseNodeIdString(string nodeid)
        {
            Int16 namespacex = 0;
            Int16 nodeId = 0;

            foreach (var item in nodeid.Split(';'))
            {
                if (item.Contains("ns="))
                {
                    namespacex = Int16.Parse(item.Split('=')[1]);
                }
                else if (item.Contains("i="))
                {
                    nodeId = Int16.Parse(item.Split('=')[1]);
                }
            }
            return new Tuple<Int16, Int16>(namespacex, nodeId);

        }

        #endregion


    }
}
