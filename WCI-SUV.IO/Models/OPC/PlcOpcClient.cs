using Microsoft.Extensions.Logging;
using Opc.Ua;
using Opc.Ua.Client;
using Opc.Ua.Configuration;
using Opc.Ua.Export;
using System;
using System.Threading;
using System.Threading.Tasks;
using WCI_SUV.Core.Common;
using WCI_SUV.Core.Interface;

namespace WCI_SUV.IO.Models.OPC
{
    public class PlcOpcClient : IOpcClient, IDisposable
    {
        private Session _session;
        private readonly ApplicationInstance _applicationInstance;
        private readonly ILogger<PlcOpcClient> _logger;
        private readonly SemaphoreSlim _connectionLock = new(1, 1);
        private readonly ApplicationConfiguration _config;

        private const int DEFAULT_TIMEOUT = 30000; // Reduced from 550000ms
        private const int RETRY_ATTEMPTS = 1;
        private const int KEEPALIVE_INTERVAL = 5000;

        public bool IsConnected => _session?.Connected ?? false;

        public PlcOpcClient()
        {
            var loggerFactory = LoggerFactory.Create(builder =>
            {
                builder.AddConsole();
            });

            _logger = loggerFactory.CreateLogger<PlcOpcClient>();
            _applicationInstance = new ApplicationInstance
            {
                ApplicationName = "OpcUaClient",
                ApplicationType = ApplicationType.Client
            };

            _config = CreateDefaultConfiguration();
        }

        private async Task<ApplicationConfiguration> InitializeConfiguration()
        {
            var config = await _applicationInstance.LoadApplicationConfiguration(".Config.xml", false);
            config.ClientConfiguration.DefaultSessionTimeout = DEFAULT_TIMEOUT;
            config.TransportQuotas.OperationTimeout = DEFAULT_TIMEOUT;

            await _applicationInstance.CheckApplicationInstanceCertificate(false, 2048);
            return config;
        }

        private ApplicationConfiguration CreateDefaultConfiguration()
        {
            return new ApplicationConfiguration
            {
                ApplicationName = "OpcUaClient",
                ApplicationType = ApplicationType.Client,
                SecurityConfiguration = new SecurityConfiguration
                {
                    ApplicationCertificate = new CertificateIdentifier(),
                    TrustedPeerCertificates = new CertificateTrustList(),
                    TrustedIssuerCertificates = new CertificateTrustList(),
                    RejectedCertificateStore = new CertificateTrustList(),
                    AutoAcceptUntrustedCertificates = true
                },
                TransportQuotas = new TransportQuotas { OperationTimeout = 15000 },
                ClientConfiguration = new ClientConfiguration { DefaultSessionTimeout = 60000 }
            };
        }

        public async Task<Result<bool>> ConnectAsync(string endpointUrl)
        {
            await _connectionLock.WaitAsync();
            try
            {
                if (IsConnected)
                {
                    _logger.LogWarning("Already connected to OPC-UA server.");
                    return Result<bool>.Failure("Already connected to OPC-UA server");
                }

                for (int attempt = 1; attempt <= RETRY_ATTEMPTS; attempt++)
                {
                    try
                    {
                        // Use the provided endpoint URL directly
                        var endpointDescription = CoreClientUtils.SelectEndpoint(endpointUrl, false);
                        var endpointConfiguration = EndpointConfiguration.Create(_config);
                        endpointConfiguration.OperationTimeout = DEFAULT_TIMEOUT;

                        var configuredEndpoint = new ConfiguredEndpoint(null, endpointDescription, endpointConfiguration);

                        _session = await Session.Create(
                            _config,
                            configuredEndpoint,
                            false,
                            "OpcUaClient",
                            DEFAULT_TIMEOUT,
                            new UserIdentity(new AnonymousIdentityToken()),
                            null);

                        if (_session?.Connected == true)
                        {
                            ConfigureSession();
                            _logger.LogInformation("Connected to OPC-UA server at {Endpoint}", endpointUrl);
                            return Result<bool>.Success(true);
                        }
                    }
                    catch (Exception ex) when (attempt < RETRY_ATTEMPTS)
                    {
                        _logger.LogWarning(ex, "Connection attempt {Attempt} failed.", attempt);
                        await Task.Delay(1000 * attempt);
                    }
                }

                return Result<bool>.Failure("Failed to connect to OPC-UA server after 3 attempts");
            }
            finally
            {
                _connectionLock.Release();
            }
        }

        private void ConfigureSession()
        {
            _session.KeepAliveInterval = KEEPALIVE_INTERVAL;
            //_session.KeepAlive += Session_KeepAlive;
        }

        private void Session_KeepAlive(Session session, KeepAliveEventArgs e)
        {
            if (e.Status != null && ServiceResult.IsBad(e.Status))
            {
                _logger.LogError("Session keep-alive error: {Status} at {Time}", e.Status, e.CurrentTime);
                try
                {
                    session?.Close();
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error closing session during keep-alive");
                }
            }
        }

        public async Task<T> ReadNodeAsync<T>(uint registerId, ushort namespacex)
        {
            EnsureConnected();

            try
            {
                using var timeout = new CancellationTokenSource(DEFAULT_TIMEOUT);
                var nodeId = new NodeId(registerId, namespacex);
                var value = await _session.ReadValueAsync(nodeId, timeout.Token);

                if (StatusCode.IsBad(value.StatusCode))
                {
                    throw new ServiceResultException(value.StatusCode);
                }

                return (T)Convert.ChangeType(value.Value, typeof(T));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to read node {Register} in namespace {Namespace}",
                    registerId, namespacex);
                throw;
            }
        }

        public async Task WriteNodeAsync<T>(uint registerId, ushort namespacex, T value)
        {
            EnsureConnected();

            try
            {
                var nodeId = new NodeId(registerId, namespacex);
                var writeValue = new WriteValue
                {
                    NodeId = nodeId,
                    AttributeId = Attributes.Value,
                    Value = new DataValue(new Variant(value))
                };

                using var timeout = new CancellationTokenSource(DEFAULT_TIMEOUT);
                var response = await _session.WriteAsync(
                    null,
                    new WriteValueCollection { writeValue },
                    timeout.Token);

                if (response.Results == null || response.Results.Count == 0 ||
                    StatusCode.IsBad(response.Results[0]))
                {
                    throw new ServiceResultException(response.Results?[0] ??
                        StatusCodes.BadUnknownResponse);
                }

                _logger.LogDebug("Successfully wrote to NodeId {NodeId}", nodeId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to write to node {Register} in namespace {Namespace}",
                    registerId, namespacex);
                throw;
            }
        }

        private void EnsureConnected()
        {
            if (!IsConnected)
            {
                throw new InvalidOperationException("Not connected to OPC-UA server");
            }
        }

        public async Task GetNamespaceUri()
        {
            try
            {
                var namespaceUris = _session.NamespaceUris.ToArray();

                var namespaceIndex = 1;

                // Check if the index is valid
                if (namespaceIndex < 0 || namespaceIndex >= namespaceUris.Length)
                {
                    Console.WriteLine("Invalid namespace index.");
                    return;
                }

                // Get the namespace URI by index
                var namespaceUri = namespaceUris[namespaceIndex];
                Console.WriteLine($"Browsing Namespace: {namespaceUri}");

                // Browse the nodes in this namespace (start from root folder)
                var rootNodeId = ObjectIds.ObjectsFolder; // Starting node (Objects folder)
                var browseResults = await BrowseNodeAsync(rootNodeId, namespaceUri);

                // Display the results
                foreach (var reference in browseResults)
                {
                    Console.WriteLine($"Node: {reference.DisplayName.Text}, NodeId: {reference.NodeId}");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
            }
        }

        private async Task<List<ReferenceDescription>> BrowseNodeAsync(NodeId nodeId, string namespaceUri)
        {
            var variables = new List<ReferenceDescription>();
            var browseDesc = new BrowseDescription
            {
                NodeId = new NodeId(Objects.RootFolder, 1),
                BrowseDirection = BrowseDirection.Forward,
                ReferenceTypeId = ReferenceTypeIds.HierarchicalReferences,
                IncludeSubtypes = true,
                NodeClassMask = (uint)NodeClass.Variable,
                ResultMask = (uint)BrowseResultMask.All
            };

            var nullNodes = new List<NodeId>();

            var response = await _session.BrowseAsync(
                new RequestHeader(),
                null,  // view
                nullNodes,  // nodeToBrowse
                0,  // maxResultsToReturn
                BrowseDirection.Forward,
                ReferenceTypeIds.HierarchicalReferences,
                true,  // includeSubtypes
                (uint)NodeClass.Variable,  // nodeClassMask
                CancellationToken.None  // resultMask
            );

            return variables;
        }

        public async Task<NodeId?> BrowseGlobalVariables()
        {
            try
            {
                // Define the path to navigate
                var path = new[] { "DeviceSet", "FX5-CPU", "GlobalVars" };

                // Start from the root folder
                NodeId currentNode = ObjectIds.ObjectsFolder;

                foreach (var segment in path)
                {
                    // Create the BrowseDescription for the current node
                    var browseDescription = new BrowseDescription
                    {
                        NodeId = currentNode,
                        BrowseDirection = BrowseDirection.Forward,
                        IncludeSubtypes = true,
                        NodeClassMask = (uint)NodeClass.Object | (uint)NodeClass.Variable,
                        ResultMask = (uint)BrowseResultMask.All
                    };

                    // Call BrowseAsync with the correct signature
                    var browseResponse = await _session.BrowseAsync(
                        requestHeader: null,
                        view: null,
                        requestedMaxReferencesPerNode: 0,
                        nodesToBrowse: new BrowseDescriptionCollection { browseDescription },
                        ct: default
                    );

                    // Validate the response
                    if (browseResponse.Results == null || browseResponse.Results.Count == 0 || browseResponse.Results[0].References == null)
                    {
                        Console.WriteLine($"Failed to browse node: {currentNode}");
                        return null;
                    }

                    // Find the child node matching the current path segment
                    var nextNode = browseResponse.Results[0].References
                        .FirstOrDefault(r => r.BrowseName.Name == segment);

                    if (nextNode == null)
                    {
                        Console.WriteLine($"Node '{segment}' not found under '{currentNode}'.");
                        return null;
                    }

                    // Update the current node to the matched child node
                    currentNode = ExpandedNodeId.ToNodeId(nextNode.NodeId, _session.NamespaceUris);
                }

                // Return the final NodeId (GlobalVars)
                return currentNode;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error navigating to GlobalVars: {ex.Message}");
                return null;
            }
        }


        public async Task<List<Tuple<T1,T2,T3>>> BrowseRegistersInGlobalVars<T1,T2,T3>(NodeId globalVarsNodeId)
        {
            List<Tuple<T1, T2, T3>> results = new List<Tuple<T1, T2, T3>>();
            try
            {
                var browseDescription = new BrowseDescription
                {
                    NodeId = globalVarsNodeId,  // Start browsing from GlobalVars
                    BrowseDirection = BrowseDirection.Forward, // Forward direction to get children
                    IncludeSubtypes = true,  // Include subtypes if necessary
                    NodeClassMask = (uint)NodeClass.Variable | (uint)NodeClass.Object, // Looking for Variables and Objects (registers)
                    ResultMask = (uint)BrowseResultMask.All    // Get all relevant information
                };

                byte[] continuationPoint = null;  // To store the continuation point
                bool isMoreData = true;

                while (isMoreData)
                {
                    BrowseResultCollection browseResults;
                    if (continuationPoint == null || continuationPoint.Length == 0)
                    {
                        // Initial browse request
                        var browseResponse = await _session.BrowseAsync(
                            requestHeader: null,
                            view: null,  // Default view
                            requestedMaxReferencesPerNode: 1000,  // Set the number of references to return
                            nodesToBrowse: new BrowseDescriptionCollection { browseDescription },
                            ct: CancellationToken.None
                        );

                        browseResults = browseResponse.Results;
                        continuationPoint = browseResults[0].ContinuationPoint;
                    }
                    else
                    {
                        // Subsequent browse requests using the continuation point
                        var browseNextResponse = await _session.BrowseNextAsync(
                            requestHeader: null,
                            releaseContinuationPoints: false,
                            continuationPoints: new ByteStringCollection { continuationPoint },
                            ct: CancellationToken.None
                        );

                        browseResults = browseNextResponse.Results;
                        continuationPoint = browseResults[0].ContinuationPoint;
                    }

                    // Process the results
                    if (browseResults != null && browseResults.Count > 0)
                    {
                        foreach (var reference in browseResults[0].References)
                        {
                            // Console.WriteLine($"Register: {reference.DisplayName.Text}, NodeId: {reference.NodeId}");

                            // Fetch the data type of the node
                            var dataType = await GetNodeDataTypeAsync(ExpandedNodeId.ToNodeId(reference.NodeId, _session.NamespaceUris));
                            // Console.WriteLine($"  DataType: {dataType}");

                            string name = reference.DisplayName.Text.ToString();
                            string nodeId = reference.NodeId.ToString();
                            string dataTypeString = dataType.ToString();

                            Tuple<T1, T2, T3> result = new Tuple<T1, T2, T3>((T1)Convert.ChangeType(name, typeof(T1)), (T2)Convert.ChangeType(nodeId, typeof(T2)), (T3)Convert.ChangeType(dataTypeString, typeof(T3)));
                            results.Add(result);
                        }
                    }

                    // If continuation point is empty, we've finished fetching all data
                    isMoreData = continuationPoint != null && continuationPoint.Length > 0;
                }

                Console.WriteLine("Browsing completed.");
                return results;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error browsing registers: {ex.Message}");
                return results;
            }
        }

        private async Task<string> GetNodeDataTypeAsync(NodeId nodeId)
        {
            try
            {
                var readValueId = new ReadValueId
                {
                    NodeId = nodeId,
                    AttributeId = Attributes.DataType // Fetch the DataType attribute
                };

                var readResponse = await _session.ReadAsync(
                    null,
                    0,
                    TimestampsToReturn.Neither,
                    new ReadValueIdCollection { readValueId },
                    CancellationToken.None
                );

                if (readResponse.Results != null && readResponse.Results.Count > 0)
                {
                    // The DataType attribute value is a NodeId
                    var dataTypeNodeId = (NodeId)readResponse.Results[0].Value;

                    // Translate the NodeId into its name
                    return _session.NodeCache.GetDisplayText(dataTypeNodeId);
                }

                return "Unknown";
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error fetching DataType for NodeId {nodeId}: {ex.Message}");
                return "Error";
            }
        }




        public void Dispose()
        {
            try
            {
                if (_session?.Connected == true)
                {
                    _session.Close();
                }
                _session?.Dispose();
                _connectionLock.Dispose();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during disposal");
            }
        }

       
    }
}