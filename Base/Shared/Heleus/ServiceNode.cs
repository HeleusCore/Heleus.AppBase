using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Heleus.Base;
using Heleus.Chain;
using Heleus.Chain.Core;
using Heleus.Cryptography;
using Heleus.Messages;
using Heleus.Network.Client;
using Heleus.Network.Results;
using Heleus.Service;
using Xamarin.Forms;

namespace Heleus.Apps.Shared
{
    public class ServiceNode : IPackable
    {
        public const string DefaultServiceNodeEndpointIdentifier = "-";
        public const int MinPasswordLength = 6;

        public static string GetHexFromEndpoint(Uri endpoint)
        {
            if (endpoint == null)
                return DefaultServiceNodeEndpointIdentifier;

            return Hex.TextToHexString(endpoint.AbsoluteUri);
        }

        public static Uri GetEndpointFromHex(string hex)
        {
            if (string.IsNullOrWhiteSpace(hex) || hex == DefaultServiceNodeEndpointIdentifier)
                return null;

            try
            {
                var url = Hex.FromTextHexString(hex);
                if (url.IsValdiUrl(false))
                    return new Uri(url, UriKind.Absolute);
            }
            catch (Exception ex)
            {
                Log.IgnoreException(ex);
            }

            return null;
        }

        readonly string _guid;

        bool _checkingAuthorization;
        readonly List<Key> _authorizationKeys = new List<Key>();

        public readonly Storage DocumentStorage;
        public readonly Storage CacheStorage;

        readonly Dictionary<uint, TransactionDownloadManager> _downloadManagers = new Dictionary<uint, TransactionDownloadManager>();
        public TransactionDownloadManager GetTransactionDownloadManager(uint chainIndex)
        {
            if(_downloadManagers.TryGetValue(chainIndex, out var manager))
            {
                return manager;
            }

            manager = new TransactionDownloadManager(CacheStorage, Client, ChainType.Data, ChainId, chainIndex, _serviceNodeManager.TransactionDownloadManagerFlags);
            _downloadManagers[chainIndex] = manager;

            return manager;
        }

        public readonly bool CustomEndPoint;

        public long AccountId { get; private set; }
        public Uri Endpoint => Client.ChainEndPoint;
        public string EndpointHex => CustomEndPoint ? GetHexFromEndpoint(Endpoint) : DefaultServiceNodeEndpointIdentifier;
        public int ChainId => Client.ChainId;

        string _id;
        public string Id
        {
            get
            {
                if(_id == null)
                    _id = $"{Endpoint.Host}.{ChainId}.{_guid}";
                return _id;
            }
        }

        public readonly HeleusClient Client;

        public bool HasServiceAccount => _serviceAccounts.Count > 0;
        public IReadOnlyDictionary<short, KeyStore> ServiceAccounts => _serviceAccounts;
        public IReadOnlyList<KeyStore> SortedServiceAccounts
        {
            get
            {
                var accounts = new List<KeyStore>(_serviceAccounts.Values);
                accounts.Sort((a, b) => a.KeyIndex.CompareTo(b.KeyIndex));
                return accounts;
            }
        }

        internal void AddServiceAccountForCoreChain(CoreAccountKeyStore keyStore)
        {
            _serviceAccounts[keyStore.KeyIndex] = keyStore;
            AccountId = keyStore.AccountId;
        }

        readonly Dictionary<short, KeyStore> _serviceAccounts = new Dictionary<short, KeyStore>();

        readonly ServiceNodeManager _serviceNodeManager;
        readonly Dictionary<short, SecretKeyManager> _keyManagers = new Dictionary<short, SecretKeyManager>();
        public IReadOnlyDictionary<short, SecretKeyManager> SecretKeyManagers => _keyManagers;

        public List<SecretKey> GetSecretKeys(Chain.Index index, ulong secretId)
        {
            var result = new List<SecretKey>();
            foreach(var keyManager in _keyManagers.Values)
            {
                var key = keyManager.GetSecretKey(index, secretId);
                if (key != null)
                    result.Add(key);
            }

            return result;
        }

        public bool Active { get; private set; } = true;
        public string Name { get; private set; }
        public string GetName() => Name ?? Endpoint.AbsoluteUri;
        public Color AccentColor { get; private set; }

        public bool SetActive(bool active)
        {
            if (Active == active)
                return false;

            Active = active;

            _serviceNodeManager.Save();
            _serviceNodeManager.PubSub.Publish(new ServiceNodeChangedEvent(this));
            return true;
        }

        public bool SetName(string name)
        {
            if (Name == name)
                return false;

            Name = name;

            _serviceNodeManager.Save();
            _serviceNodeManager.PubSub.Publish(new ServiceNodeChangedEvent(this));
            return true;
        }

        public void SetAccentColor(Color color)
        {
            AccentColor = color;
            _serviceNodeManager.Save();
            _serviceNodeManager.PubSub.Publish(new ServiceNodeChangedEvent(this));
        }

        readonly Dictionary<short, Dictionary<Chain.Index, SubmitAccount>> _submitAccounts = new Dictionary<short, Dictionary<Chain.Index, SubmitAccount>>();

        public T GetSubmitAccountById<T>(string id) where T : SubmitAccount
        {
            foreach(var lookUp in _submitAccounts.Values)
            {
                foreach(var account in lookUp.Values)
                {
                    if (account.ID == id && account is SubmitAccount)
                        return account as T;
                }
            }

            return null;
        }

        public bool AddSubmitAccount(SubmitAccount submitAccount, bool @override = false)
        {
            if (submitAccount == null)
                throw new ArgumentException(nameof(submitAccount));

            var keyIndex = submitAccount.KeyIndex;
            var index = submitAccount.Index;

            if (!_serviceAccounts.ContainsKey(keyIndex))
                throw new ArgumentException(nameof(submitAccount.KeyIndex));

            if (!_submitAccounts.TryGetValue(keyIndex, out var lookUp))
            {
                lookUp = new Dictionary<Chain.Index, SubmitAccount>();
                _submitAccounts[keyIndex] = lookUp;
            }

            lookUp.TryGetValue(index, out var account);

            if(account == null || @override)
            {
                lookUp[index] = submitAccount;
                return true;
            }

            return false;
        }

        public List<T> GetSubmitAccounts<T>(Func<T, bool> validate) where T : SubmitAccount
        {
            var result = new List<T>();

            foreach (var lookUp in _submitAccounts.Values)
            {
                foreach (var item in lookUp.Values)
                {
                    if (item is T submitAccount && validate.Invoke(submitAccount))
                        result.Add(submitAccount);
                }
            }

            return result;
        }

        public List<T> GetSubmitAccounts<T>() where T : SubmitAccount
        {
            var result = new List<T>();

            foreach(var lookUp in _submitAccounts.Values)
            {
                foreach(var item in lookUp.Values)
                {
                    if (item is T submitAccount)
                        result.Add(submitAccount);
                }
            }

            return result;
        }

        public List<T> GetSubmitAccounts<T>(short keyIndex) where T : SubmitAccount
        {
            var result = new List<T>();

            if(_submitAccounts.TryGetValue(keyIndex, out var lookUp))
            {
                foreach(var item in lookUp.Values)
                {
                    if (item is T submitAccount)
                        result.Add(submitAccount);
                }
            }

            return result;
        }

        public List<T> GetSubmitAccounts<T>(Chain.Index index) where T : SubmitAccount
        {
            var result = new List<T>();

            foreach (var lookUp in _submitAccounts.Values)
            {
                foreach (var item in lookUp.Values)
                {
                    if (item.Index != index)
                        continue;

                    if (item is T submitAccount)
                        result.Add(submitAccount);
                }
            }

            return result;
        }

        public T GetSubmitAccount<T>(short keyIndex, Chain.Index index) where T : SubmitAccount
        {
            if(_submitAccounts.TryGetValue(keyIndex, out var lookUp))
            {
                lookUp.TryGetValue(index, out var submitAccount);
                return submitAccount as T;
            }

            return default;
        }

        public bool HasSubmitAccount(short keyIndex, Chain.Index index)
        {
            if (_submitAccounts.TryGetValue(keyIndex, out var lookUp))
            {
                return lookUp.ContainsKey(index);
            }

            return false;
        }

        public bool HasUnlockedServiceAccount
        {
            get
            {
                foreach(var account in _serviceAccounts.Values)
                {
                    if (account.IsDecrypted)
                        return true;
                }

                return false;
            }
        }

        public KeyStore FirstUnlockedServiceAccount
        {
            get
            {
                foreach (var account in _serviceAccounts.Values)
                {
                    if (account.IsDecrypted)
                        return account;
                }

                return null;
            }
        }

        string _derivedPassword;
        public string DerivedPassword
        {
            get
            {
                if (_derivedPassword == null)
                    _derivedPassword = Hex.ToString(Rand.NextSeed(32));

                return _derivedPassword;
            }
        }

        public ServiceNode(Uri endpoint, int chainId, bool customEndpoint, string guid, ServiceNodeManager serviceNodeManager, HeleusClient client = null)
        {
            _serviceNodeManager = serviceNodeManager;
            _guid = guid.Replace("-", ".");

#if CLI
            guid = string.Empty;
#endif

            CustomEndPoint = customEndpoint;

            if(endpoint.AbsoluteUri.StartsWith(serviceNodeManager.DefaultEndPoint.AbsoluteUri, StringComparison.Ordinal) && chainId == serviceNodeManager.DefaultChainId)
            {
                AccentColor = Theme.SecondaryColor.DefaultColor;
                Name = Tr.Get("Common.OfficalService", Tr.Get("App.FullName"));
            }
            else
            {
                AccentColor = Color.FromRgba(Rand.NextInt(255), Rand.NextInt(255), Rand.NextInt(255), 255);
            }

            var pathName = $"{endpoint.Host}.{chainId}.{_guid}";
            DocumentStorage = new Storage(Path.Combine(serviceNodeManager.DocumentStorage.Root.FullName, pathName));
            CacheStorage = new Storage(Path.Combine(serviceNodeManager.CacheStorage.Root.FullName, pathName));

            Client = client;
            if(Client == null)
            {
                Client = new HeleusClient(endpoint, chainId, DocumentStorage, true)
                {
                    MessageHandler = HandleMessage
                };
            }
        }

        public ServiceNode(Uri endpoint, int chainId, bool customEndpoint, string guid, Unpacker unpacker, ServiceNodeManager serviceEndpointManager) : this(endpoint, chainId, customEndpoint, guid, serviceEndpointManager)
        {
            unpacker.Unpack(out string name);
            Name = name;
            unpacker.Unpack(out bool active);
            Active = active;

            unpacker.Unpack(out byte r);
            unpacker.Unpack(out byte g);
            unpacker.Unpack(out byte b);
            AccentColor = Color.FromRgba(r, g, b, 255);
            unpacker.Unpack(out long accountId);
            AccountId = accountId;

            //LoadStoredServiceAccounts().Wait();
        }

        public void Pack(Packer packer)
        {
            packer.Pack(Endpoint.AbsoluteUri);
            packer.Pack(ChainId);
            packer.Pack(CustomEndPoint);
            packer.Pack(_guid);

            packer.Pack(Name);
            packer.Pack(Active);
            packer.Pack((byte)(AccentColor.R * 255));
            packer.Pack((byte)(AccentColor.G * 255));
            packer.Pack((byte)(AccentColor.B * 255));
            packer.Pack(AccountId);
        }

        public async Task<ChainInfo> GetChainInfo()
        {
            await Client.SetTargetChain(ChainId);
            return Client.ChainInfo;
        }

        public async Task<ServiceInfo> GetServiceInfo()
        {
            return (await Client.DownloadServiceInfo(ChainId)).Data?.Item;
        }

        Task Resume(ResumeEvent appResume)
        {
            _ = CheckAuthorization(3);
            return Task.CompletedTask;
        }

        public string GetServiceAccountPassword(int chainId, long accountId, short keyIndex)
        {
            var credentialKey = $"ServiceAccountPassword_{chainId}_{accountId}_{keyIndex}";

            var password = _serviceNodeManager.CredentialProvider.GetCredential(credentialKey);
            if (password == null)
            {
                password = Convert.ToBase64String(Rand.NextSeed(32));
                _serviceNodeManager.CredentialProvider.AddCredential(credentialKey, password);
            }

            return password;
        }

        public async Task LoadStoredServiceAccounts()
        {
#if !CLI
            var accounts = Client.GetServiceAccounts(ChainId);
            foreach (var account in accounts)
            {
                if (account.AccountId != AccountId)
                    continue;

                var accountPassword = GetServiceAccountPassword(account.ChainId, account.AccountId, account.KeyIndex);

                if (await account.DecryptKeyAsync(accountPassword, false))
                {
                    _keyManagers[account.KeyIndex] = new SecretKeyManager(this, account, _serviceNodeManager.CredentialProvider);
                    _serviceAccounts[account.KeyIndex] = account;
                }
            }
#endif
        }

        public async Task<ImportAccountResult> ImportServiceAccount(KeyStore importAccount, string password)
        {
            try
            {
                await importAccount.DecryptKeyAsync(password, true);

                if (KeyStoreTypes.ServiceAccount != importAccount.KeyStoreType)
                    return ImportAccountResult.InvalidKeyType;

                if (importAccount.ChainId != ChainId)
                    return ImportAccountResult.InvalidChainId;

                foreach(var endpoint in _serviceNodeManager.ServiceNodes)
                {
                    foreach (var eaccount in endpoint.ServiceAccounts.Values)
                    {
                        if (importAccount.AccountId == eaccount.AccountId &&
                            importAccount.ChainId == eaccount.ChainId &&
                            importAccount.KeyIndex == eaccount.KeyIndex &&
                            importAccount.PublicKey == eaccount.PublicKey)

                            return ImportAccountResult.AccountAlreadyPresent;
                    }
                }

                if (HasUnlockedServiceAccount)
                {
                    if (importAccount.AccountId != AccountId)
                        return ImportAccountResult.InvalidAccountId;

                    if(_serviceAccounts.ContainsKey(importAccount.KeyIndex))
                        return ImportAccountResult.AccountAlreadyPresent;
                }

                if (!await IsValidServiceAccount(importAccount, password))
                    return ImportAccountResult.ValidationFailed;

#if CLI
                await Process(importAccount as ServiceAccountKeyStore, true);
                return ImportAccountResult.Ok;
#else

                var localPassword = GetServiceAccountPassword(importAccount.ChainId, importAccount.AccountId, importAccount.KeyIndex);
                var account = await Client.StoreAccount(importAccount.Name, (importAccount as ServiceAccountKeyStore).SignedPublicKey, importAccount.DecryptedKey, localPassword);

                if(account.IsDecrypted)
                {
                    await Process(account as ServiceAccountKeyStore, true);

                    return ImportAccountResult.Ok;
                }
#endif
            }
            catch (Exception ex)
            {
                Log.IgnoreException(ex);
            }

            return ImportAccountResult.PasswordInvalid;
        }

        public async Task<bool> IsValidServiceAccount(KeyStore account, string password)
        {
            try
            {
                await account.DecryptKeyAsync(password, true);

                if (account.KeyStoreType == KeyStoreTypes.ServiceAccount)
                {
                    var accountKey = (await Client.DownloadValidServiceAccountKey(account.AccountId, ChainId, account.KeyIndex)).Data?.Item;
                    if (accountKey != null)
                    {
                        return accountKey.KeyIndex == account.KeyIndex && accountKey.PublicKey == account.DecryptedKey.PublicKey;
                    }
                }
            }
            catch { }

            return false;
        }

        public async Task<AuthorizationResult> RequestAuthorization(Key key)
        {
            (_, var account) = _serviceNodeManager.GetServiceAccount(key);
            if(account != null)
                return AuthorizationResult.AlreadyAvailable;

            _serviceNodeManager.PubSub.Unsubscribe<ResumeEvent>(Client);
            _serviceNodeManager.PubSub.Subscribe<ResumeEvent>(Client, Resume);

            if (!_authorizationKeys.Contains(key))
                _authorizationKeys.Add(key);

            if (!await CheckAuthorization(0))
            {
                if (!Client.Isconnected)
                    return AuthorizationResult.NotConnected;

                UIApp.OpenUrl(new Uri($"https://heleuscore.com/heleus/request/authorizeservice/{ChainId}/{key.PublicKey.HexString}/"));
                return AuthorizationResult.Requesting;
            }

            return AuthorizationResult.Done;
        }

        public void RequestDerived()
        {
            UIApp.OpenUrl(new Uri($"https://heleuscore.com/heleus/request/authorizeservicederived/{ChainId}/{DerivedPassword}/"));
        }

        public async Task<RestoreResult> RequestRestore(long accountId, Key key)
        {
            (_, var account) = _serviceNodeManager.GetServiceAccount(key);
            if (account != null)
                return RestoreResult.AlreadyAvailable;

            if(AccountId != 0)
            {
                if (AccountId != accountId)
                    return RestoreResult.InvalidAccountId;
            }

            var revokeableKeyResult = (await Client.DownloadRevokeableServiceAccountKey(accountId, ChainId, key.PublicKey)).Data;

            if (revokeableKeyResult == null)
                return RestoreResult.DownloadError;

            if (revokeableKeyResult.ResultType == ResultTypes.AccountNotFound)
                return RestoreResult.InvalidAccountId;

            if (revokeableKeyResult.ResultType == ResultTypes.DataNotFound)
                return RestoreResult.NotFound;

            var revokeableKey = revokeableKeyResult.Item;
            if (revokeableKey == null)
                return RestoreResult.DownloadError;

            await StoreNewServiceAccount(revokeableKey.Item, key);

            return RestoreResult.Ok;
        }

        async Task HandleMessage(Messages.Message message)
        {
            var messageType = (ClientMessageTypes)message.MessageType;
            if (messageType == ClientMessageTypes.KeyCheckResponse)
            {
                var foundList = new List<Key>();
                foreach(var authorizationKey in _authorizationKeys)
                {
                    var signedKey = await Client.GetServiceAccountKey(authorizationKey, message as ClientKeyCheckResponseMessage);
                    if (signedKey != null)
                    {
                        await StoreNewServiceAccount(signedKey, authorizationKey);
                        foundList.Add(authorizationKey);
                    }
                }

                foreach (var found in foundList)
                    _authorizationKeys.Remove(found);

                _ = CheckAuthorization(3);
            }
        }

        public Task<bool> CheckRequestAuthorization()
        {
            return CheckAuthorization(3);
        }

        async Task<bool> CheckAuthorization(int triesLeft)
        {
            if (_authorizationKeys.Count == 0)
                return false;

            if (_checkingAuthorization)
                return false;

            _checkingAuthorization = true;

            var foundList = new List<Key>();
            foreach (var authorizationKey in _authorizationKeys)
            {
                var signedKey = await Client.CheckServiceAccountKey(authorizationKey, true);
                if (signedKey != null)
                {
                    await StoreNewServiceAccount(signedKey, authorizationKey);
                    foundList.Add(authorizationKey);
                }
            }

            foreach (var found in foundList)
                _authorizationKeys.Remove(found);

            if (_authorizationKeys.Count == 0)
                return true;

            _checkingAuthorization = false;

            triesLeft -= 1;
            if (triesLeft >= 0)
            {
                await Task.Delay(250);
                return await CheckAuthorization(triesLeft);
            }

            return false;
        }

        async Task Process(ServiceAccountKeyStore account, bool import)
        {
            foreach (var endpoint in _serviceNodeManager.ServiceNodes)
            {
                if (endpoint == this)
                    continue;

                foreach (var eaccount in endpoint.ServiceAccounts.Values)
                {
                    if (account.AccountId == eaccount.AccountId &&
                        account.ChainId == eaccount.ChainId &&
                        account.KeyIndex == eaccount.KeyIndex &&
                        account.PublicKey == eaccount.PublicKey)

                        return;
                }

                // we already have such an endpoint, redirect the account to this endpoint
                if (endpoint.Endpoint == Endpoint && endpoint.AccountId == account.AccountId && endpoint.ChainId == account.ChainId)
                {
                    await endpoint.Process(account, import);
                    return;
                }
            }

            if (string.IsNullOrEmpty(Name))
            {
                var chainInfo = Client.ChainInfo;
                if (chainInfo == null)
                {
                    chainInfo = (await Client.DownloadChainInfo(ChainId)).Data;
                }

                Name = chainInfo?.Name;
            }

            AccountId = account.AccountId;
            _serviceAccounts[account.KeyIndex] = account;

            //await Client.SetServiceAccount(DefaultServiceAccount, null, false);
            ServiceNodeManager.Current.ServiceNodeUpdated(this);
            var keyManager = new SecretKeyManager(this, account, _serviceNodeManager.CredentialProvider);
            _keyManagers[account.KeyIndex] = keyManager;

            if (import)
                await _serviceNodeManager.PubSub.PublishAsync(new ServiceAccountImportEvent(account, this));
            else
                await _serviceNodeManager.PubSub.PublishAsync(new ServiceAccountAuthorizedEvent(account, this));

            await _serviceNodeManager.PubSub.PublishAsync(new ServiceAccountUnlockedEvent(account, this));
            await _serviceNodeManager.PubSub.PublishAsync(new ServiceNodeChangedEvent(this));
        }

        async Task StoreNewServiceAccount(PublicServiceAccountKey publicKey, Key key)
        {
            if (publicKey.ChainId != ChainId)
                return;

            try
            {
                var password = GetServiceAccountPassword(publicKey.ChainId, publicKey.AccountId, publicKey.KeyIndex);
                var account = await Client.StoreAccount(Tr.Get("Common.AccountKeyName", Tr.Get("App.FullName"), publicKey.AccountId, publicKey.KeyIndex), publicKey, key, password);

                await Client.CloseConnection(DisconnectReasons.Graceful);

                if(account.IsDecrypted)
                {
                    await Process(account as ServiceAccountKeyStore, false);

                    _serviceNodeManager.PubSub.Unsubscribe<ResumeEvent>(Client);
                }
            }
            catch { }
        }
    }
}
