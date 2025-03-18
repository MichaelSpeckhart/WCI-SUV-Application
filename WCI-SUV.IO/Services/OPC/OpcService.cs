using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Opc.Ua;
using Newtonsoft.Json.Linq;
using PlcNode = WCI_SUV.Core.Entities.Node;
using WCI_SUV.Core.Services;
using WCI_SUV.Core.Entities;
using WCI_SUV.Core.Common;
using WCI_SUV.Core.Interface;
using WCI_SUV.Core.Interface.OPC;
using WCI_SUV.IO.Models.OPC;

namespace WCI_SUV.IO.Services.OPC
{
    public class OpcService : IOpcService, IDisposable
    {
        private readonly PlcOpcClient _clientService;
        private readonly NodeManager _nodeManager;
        private bool _disposed = false;

        public OpcService(PlcOpcClient client, NodeManager nodeManager)
        {
            _clientService = client;
            _nodeManager = nodeManager;
        }

        public async Task<bool> ConnectToServer(string endpoint)
        {
            return (await _clientService.ConnectAsync(endpoint)).isSuccess;
        }

        public async Task<Result<Int16>> GetCurrentSlotNumber()
        {
            return await ReadNodeValue<Int16>(ConveyorNodes.CURRENT_SLOT);
        }

        public async Task<Result<Int16>> GetTargetSlot()
        {
            return await ReadNodeValue<Int16>(ConveyorNodes.TARGET_SLOT);
        }

        private async Task<Result<T>> ReadNodeValue<T>(string nodeKey)
        {
            if (!_clientService.IsConnected)
                return Result<T>.Failure("Not Connected");

            try
            {
                var node = await _nodeManager.GetNode(nodeKey);
                if (node == null) return Result<T>.Failure("Node not found");

                var value = await _clientService.ReadNodeAsync<T>(node.Register, (ushort)node.Namespace);
                return Result<T>.Success(value);
            }
            catch (Exception ex)
            {
                return Result<T>.Failure(ex.Message);
            }
        }

        public async Task<Result<bool>> SetTargetSlot(Int16 targetSlot)
        {
            return await WriteNodeValue(ConveyorNodes.TARGET_SLOT, targetSlot);
        }

        private async Task<Result<bool>> WriteNodeValue<T>(string nodeKey, T value)
        {
            if (!_clientService.IsConnected)
                return Result<bool>.Failure("Not Connected");

            try
            {
                var node = await _nodeManager.GetNode(nodeKey);
                if (node == null) return Result<bool>.Failure("Node not found");

                await _clientService.WriteNodeAsync(node.Register, (ushort)node.Namespace, value);
                return Result<bool>.Success(true);
            }
            catch (Exception ex)
            {
                return Result<bool>.Failure(ex.Message);
            }
        }

        public async Task<Result<bool>> GoToTargetSlot()
        {
            try
            {
                var currentSlotResult = await GetCurrentSlotNumber();
                var targetSlotResult = await GetTargetSlot();

                if (!currentSlotResult.isSuccess || !targetSlotResult.isSuccess)
                    return Result<bool>.Failure("Failed to get slot information");

                int currentSlot = currentSlotResult.Value;
                int targetSlot = targetSlotResult.Value;
                if (currentSlot == targetSlot) return Result<bool>.Success(true);

                await RunConveyor();

                while (currentSlot != targetSlot)
                {
                    await Task.Delay(25);
                    currentSlotResult = await GetCurrentSlotNumber();
                    if (!currentSlotResult.isSuccess)
                        return Result<bool>.Failure("Error retrieving current slot");

                    currentSlot = currentSlotResult.Value;
                    if (Math.Abs(targetSlot - currentSlot) <= 15)
                        await SlowConveyorSpeed();
                }

                await StopConveyor();
                return Result<bool>.Success(true);
            }
            catch (Exception ex)
            {
                return Result<bool>.Failure(ex.Message);
            }
        }

        public async Task<Result<bool>> SlowConveyorSpeed()
        {
            return await WriteNodeValue(ConveyorNodes.CURR_SPEED, 10);
        }

        public async Task<Result<bool>> StopConveyor()
        {
            return await WriteNodeValue(ConveyorNodes.CONVEYOR_RUN, false);
        }

        public async Task<Result<bool>> RunConveyor()
        {
            return await WriteNodeValue(ConveyorNodes.CONVEYOR_RUN, true);
        }

        public void Dispose()
        {
            if (!_disposed)
            {
                try
                {
                    _clientService?.Dispose();
                    Console.WriteLine("OPC-UA connection closed.");
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error disposing OPC connection: {ex.Message}");
                }
                _disposed = true;
            }
        }
    }
}
