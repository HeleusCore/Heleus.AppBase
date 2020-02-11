using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Heleus.Base;
using Heleus.Chain;
using Heleus.Cryptography;
using Heleus.Network.Client;
using Heleus.Service;
using TinyJson;

namespace Heleus.Apps.Shared
{
    public enum AuthorizationResult
    {
        NotConnected,
        Requesting,
        AlreadyAvailable,
        Done
    }

    public enum RestoreResult
    {
        DownloadError,
        InvalidAccountId,
        NotFound,
        AlreadyAvailable,
        Ok
    }

    public class ServiceNodeManager
    {
#if DEBUG
        public const int MinimumSecretKeyPassphraseLength = 2;
        public const int MinimumServiceAccountPassphraseLength = 2;
#else
        public const int MinimumSecretKeyPassphraseLength = 16;
        public const int MinimumServiceAccountPassphraseLength = 32;
#endif

        public static ServiceNodeManager Current { get; private set; }

        public readonly long MinimumServiceVersion;
        public readonly string RequiredServiceName;

        public readonly Storage DocumentStorage;
        public readonly Storage CacheStorage;
        public readonly PubSub PubSub;
        public readonly ISettingsCredential CredentialProvider;

        public readonly ClientBase DefaultClient;

        public readonly TransactionDownloadManagerFlags TransactionDownloadManagerFlags;

        public bool HadUnlockedServiceNode { get; private set; }
        public bool Ready { get; private set; }

        readonly ISettingsData _settingsData;
        readonly DebugInfo _debugInfo;
        readonly int _defaultChainId;
        readonly Uri _defaultEndPoint;
        ServiceNode _newDefaultServiceEndpoint;

        internal void AddServiceNodeForCoreChain(ServiceNode serviceNode)
        {
            if(serviceNode.ChainId == Protocol.CoreChainId)
                _serviceNodes.Add(serviceNode);
        }

        internal void AddServiceNodeForCli(ServiceNode serviceNode)
        {
            _serviceNodes.Add(serviceNode);
        }

        public bool HasDebugEndPoint => !string.IsNullOrWhiteSpace(_debugInfo?.endpoint);
        readonly List<ServiceNode> _serviceNodes = new List<ServiceNode>();
        public IReadOnlyList<ServiceNode> ServiceNodes => Ready ? _serviceNodes : new List<ServiceNode>();

        /*
        public bool HasUnlockedServiceNode
        {
            get
            {
                if (!Ready)
                    return false;

                foreach (var endpoint in _serviceNodes)
                {
                    if (endpoint.Active && endpoint.HasUnlockedServiceAccount)
                        return true;
                }

                return false;
            }
        }
        */

        public Uri DefaultEndPoint
        {
            get
            {
#if DEBUG
                var ep = _debugInfo?.endpoint;
                if (!string.IsNullOrWhiteSpace(ep))
                    return new Uri(ep);
#endif
                return _defaultEndPoint;
            }
        }

        public int DefaultChainId
        {
            get
            {
#if DEBUG
                if (_debugInfo != null)
                {
                    if (_debugInfo.chainid > Protocol.CoreChainId)
                        return _debugInfo.chainid;
                }
#endif
                return _defaultChainId;
            }
        }

        public ServiceNode FirstDefaultServiceNode
        {
            get
            {
                foreach(var node in _serviceNodes)
                {
                    if (node.Active && !node.CustomEndPoint)
                        return node;
                }

                return null;
            }
        }

        public ServiceNode FirstServiceNode
        {
            get
            {
                foreach(var node in _serviceNodes)
                {
                    if (node.Active)
                        return node;
                }

                return null;
            }
        }

        public ServiceNode FirstUnlockedServiceNode
        {
            get
            {
                foreach (var node in _serviceNodes)
                {
                    if (node.Active && node.HasUnlockedServiceAccount)
                        return node;
                }

                return null;
            }
        }

        public ServiceNode NewDefaultServiceNode
        {
            get
            {
                if (_newDefaultServiceEndpoint == null)
                    _newDefaultServiceEndpoint = new ServiceNode(DefaultEndPoint, DefaultChainId, false, Guid.NewGuid().ToString(), this);

                return _newDefaultServiceEndpoint;
            }
        }

        public ServiceNodeManager(int defaultChainId, Uri defaultEndpoint, long minimumServiceVersion, string requiredServiceName, ISettingsCredential credentialProvider, ISettingsData settingsData, PubSub pubSub, TransactionDownloadManagerFlags transactionDownloadManagerFlags = TransactionDownloadManagerFlags.None)
        {

            Transactions.Transaction.Init();

            if (Current != null)
                throw new Exception("Only one instance allowed.");

            Current = this;

            try
            {

#if DEBUG
                var debugInfoData = EmbeddedResource.GetEmbeddedResource<ServiceNode>("debuginfo.txt");
                if (debugInfoData != null)
                {
                    var debugInfoJson = Encoding.UTF8.GetString(debugInfoData);
                    _debugInfo = debugInfoJson.FromJson<DebugInfo>();
                }
#else
                _debugInfo = null;
#endif
            }
            catch (Exception ex)
            {
                Log.IgnoreException(ex);
            }

#if !CLI

            _settingsData = settingsData;
            CredentialProvider = credentialProvider;
#endif

            PubSub = pubSub;

            _defaultChainId = defaultChainId;
            _defaultEndPoint = defaultEndpoint;

            MinimumServiceVersion = minimumServiceVersion;
            RequiredServiceName = requiredServiceName;

            TransactionDownloadManagerFlags = transactionDownloadManagerFlags;

            DefaultClient = new ClientBase(DefaultEndPoint, DefaultChainId);
            DocumentStorage = new Storage(StorageInfo.DocumentStorage.RootPath);
            CacheStorage = new Storage(StorageInfo.CacheStorage.RootPath);

            if(_settingsData != null)
            {
                var data = _settingsData.GetData($"nameof(ServiceNodeManager)Settings");
                if(data != null)
                {
                    using(var unpacker = new Unpacker(data))
                    {
                        ChunkReader.Read(unpacker, (reader) =>
                        {
                            var hadUnlockServiceNode = false;
                            reader.Read(nameof(HadUnlockedServiceNode), ref hadUnlockServiceNode);
                            HadUnlockedServiceNode = hadUnlockServiceNode;
                        });
                    }
                }
            }
        }

        public async Task LoadServiceNodes()
        {
            var last = HadUnlockedServiceNode;

            var hasDefaultServiceNode = false;
            if (_settingsData != null)
            {
                var data = _settingsData.GetData(nameof(ServiceNodeManager));
                if (data != null)
                {
                    using (var unpacker = new Unpacker(data))
                    {
                        var count = unpacker.UnpackInt();
                        for (var i = 0; i < count; i++)
                        {
                            unpacker.Unpack(out string endPointString);
                            var endPoint = new Uri(endPointString);
                            unpacker.Unpack(out int chainId);
                            unpacker.Unpack(out bool customEndPoint);
                            unpacker.Unpack(out string guid);

                            var isDefault = false;
                            if (!customEndPoint && chainId == DefaultChainId)
                            {
                                isDefault = true;
                                hasDefaultServiceNode = true;
                                endPoint = DefaultEndPoint;
                            }

                            var serviceEndpoint = new ServiceNode(endPoint, chainId, customEndPoint, guid, unpacker, this);
                            await serviceEndpoint.LoadStoredServiceAccounts();
                            if(!HadUnlockedServiceNode)
                                HadUnlockedServiceNode = serviceEndpoint.HasUnlockedServiceAccount;

                            _serviceNodes.Add(serviceEndpoint);

                            if (isDefault && _newDefaultServiceEndpoint == null && !serviceEndpoint.HasUnlockedServiceAccount)
                                _newDefaultServiceEndpoint = serviceEndpoint;
                        }
                    }
                }

                if (last != HadUnlockedServiceNode)
                    Save();
            }
#if !CLI

            if (DefaultChainId != Protocol.CoreChainId)
            {
                if (!hasDefaultServiceNode)
                    _serviceNodes.Add(NewDefaultServiceNode);
            }
#endif

            Ready = true;

            await PubSub.PublishAsync(new ServiceNodesLoadedEvent());
            if (last != HadUnlockedServiceNode)
            {
                Save();
                // just in case
                var first = FirstUnlockedServiceNode;
                if(first != null)
                    await PubSub.PublishAsync(new ServiceAccountAuthorizedEvent((ServiceAccountKeyStore)first.ServiceAccounts.Values.First(), first));
            }
        }

        public bool IsServiceInfoValid(ServiceInfo serviceInfo)
        {
            if (serviceInfo == null)
                return false;

            return serviceInfo.Version >= MinimumServiceVersion && serviceInfo.Name.StartsWith(RequiredServiceName, StringComparison.Ordinal);
        }

        public void Save()
        {
#if CLI
            return;
#endif

            if (!Ready)
                return;

            if (_settingsData != null)
            {
                using (var packer = new Packer())
                {
                    var count = _serviceNodes.Count;
                    packer.Pack(count);
                    for (var i = 0; i < count; i++)
                    {
                        var endpoint = _serviceNodes[i];
                        endpoint.Pack(packer);
                    }

                    _settingsData.SetData(nameof(ServiceNodeManager), packer.ToByteArray(), false);
                }

                using(var packer = new Packer())
                {
                    ChunkWriter.Write(packer, (writer) =>
                    {
                        writer.Write(nameof(HadUnlockedServiceNode), HadUnlockedServiceNode);
                    });

                    _settingsData.SetData($"nameof(ServiceNodeManager)Settings", packer.ToByteArray(), true);
                }
            }
        }

        public void ServiceNodeUpdated(ServiceNode serviceNode)
        {
            if (serviceNode.HasUnlockedServiceAccount)
            {
                HadUnlockedServiceNode = true;

                if (!_serviceNodes.Contains(serviceNode))
                {
                    _serviceNodes.Add(serviceNode);
                    PubSub.Publish(new ServiceNodeAddedEvent(serviceNode));
                }
                else
                {
                    PubSub.Publish(new ServiceNodeChangedEvent(serviceNode));
                }

                Save();
            }
        }

        public ServiceNode GetServiceNode(Uri endpoint, int chainId, long requiredAccountId = 0)
        {
            foreach (var node in _serviceNodes)
            {
                if (node.Active && node.ChainId == chainId)
                {
                    if (requiredAccountId > 0)
                    {
                        if (node.AccountId != requiredAccountId)
                            continue;
                    }

                    if (node.CustomEndPoint)
                    {
                        if (endpoint != null && node.Endpoint.AbsoluteUri.StartsWith(endpoint.AbsoluteUri, StringComparison.Ordinal))
                            return node;
                    }
                    else
                    {
                        if (endpoint == null)
                            return node;
                    }
                }
            }

            return null;
        }

        public List<T> GetSubmitAccounts<T>() where T : SubmitAccount
        {
            var result = new List<T>();

            foreach (var serviceNode in _serviceNodes)
            {
                if (serviceNode.Active)
                    result.AddRange(serviceNode.GetSubmitAccounts<T>());
            }

            return result;
        }

        public List<T> GetSubmitAccounts<T>(Chain.Index index) where T : SubmitAccount
        {
            var result = new List<T>();

            foreach (var serviceNode in _serviceNodes)
            {
                if (serviceNode.Active)
                    result.AddRange(serviceNode.GetSubmitAccounts<T>(index));
            }

            return result;
        }

        public ServiceNode NewServiceNode(int chainId, Uri endpoint, bool customEndPoint)
        {
            return new ServiceNode(endpoint, chainId, customEndPoint, Guid.NewGuid().ToString(), this);
        }

        public (ServiceNode, KeyStore) GetServiceAccount(Key key)
        {
            foreach (var serviceNode in _serviceNodes)
            {
                foreach (var account in serviceNode.ServiceAccounts.Values)
                {
                    if (account.DecryptedKey == key)
                        return (serviceNode, account);
                }
            }

            return (null, null);
        }

    }
}
