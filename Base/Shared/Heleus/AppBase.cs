using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Heleus.Base;
using Heleus.Network.Client;
using Heleus.Transactions;

namespace Heleus.Apps.Shared
{
    public abstract class AppBase
    {
        public static AppBase Current { get; protected set; }

        readonly Dictionary<(string, uint), AccountTransactionDownload> _dataAccountTransactionDownloads = new Dictionary<(string, uint), AccountTransactionDownload>();
        readonly Dictionary<(string, uint), Dictionary<Chain.Index, AccountIndexTransactionDownload>> _dataAccountIndexTransactionDownloads = new Dictionary<(string, uint), Dictionary<Chain.Index, AccountIndexTransactionDownload>>();

        readonly Dictionary<string, string> _lastUsedServiceNodes = new Dictionary<string, string>();
        readonly Dictionary<string, string> _lastUsedSubmitAccounts = new Dictionary<string, string>();

        public virtual void Init()
        {
            UIApp.PubSub.Subscribe<ServiceAccountImportEvent>(this, AccountImport);
            UIApp.PubSub.Subscribe<ServiceAccountAuthorizedEvent>(this, AccountAuthorized);
            UIApp.PubSub.Subscribe<ServiceNodeChangedEvent>(this, ServiceNodeChanged);
            UIApp.PubSub.Subscribe<ServiceNodesLoadedEvent>(this, ServiceNodesLoaded);

            var lastUsedData = UIApp.Current.SettingsData.GetData("LastUsed");
            if (lastUsedData != null)
            {
                try
                {
                    using (var unpacker = new Unpacker(lastUsedData))
                    {
                        var count = unpacker.UnpackInt();
                        for (var i = 0; i < count; i++)
                        {
                            var key = unpacker.UnpackString();
                            var value = unpacker.UnpackString();
                            _lastUsedServiceNodes[key] = value;
                        }

                        count = unpacker.UnpackInt();
                        for(var i = 0; i < count; i++)
                        {
                            var key = unpacker.UnpackString();
                            var value = unpacker.UnpackString();
                            _lastUsedSubmitAccounts[key] = value;
                        }
                    }
                }
                catch (Exception ex)
                {
                    Log.IgnoreException(ex);
                }
            }

            UpdateSubmitAccounts();
        }

        void SaveLastUsed()
        {
            using(var packer = new Packer())
            {
                packer.Pack(_lastUsedServiceNodes.Count);
                foreach(var item in _lastUsedServiceNodes)
                {
                    packer.Pack(item.Key);
                    packer.Pack(item.Value);
                }

                packer.Pack(_lastUsedSubmitAccounts.Count);
                foreach (var item in _lastUsedSubmitAccounts)
                {
                    packer.Pack(item.Key);
                    packer.Pack(item.Value);
                }

                UIApp.Current.SettingsData.SetData("LastUsed", packer.ToByteArray(), true);
            }

            UIApp.Current.SaveSettings();
        }

        public virtual long CurrentAccountId
        {
            get
            {
                var serviceNode = GetLastUsedServiceNode();
                if (serviceNode != null)
                    return serviceNode.AccountId;

                return 0;
            }
        }

        public void SetLastUsedServiceNode(ServiceNode serviceNode, string key = "default")
        {
            if(serviceNode == null)
            {
                _lastUsedServiceNodes.Remove(key);
            }
            else
            {
                _lastUsedServiceNodes[key] = serviceNode.Id;
            }

            SaveLastUsed();
        }

        public virtual ServiceNode GetLastUsedServiceNode(string key = "default")
        {
            if(_lastUsedServiceNodes.TryGetValue(key, out var id))
            {
                foreach(var serviceNode in ServiceNodeManager.Current.ServiceNodes)
                {
                    if (serviceNode.Active && serviceNode.Id == id)
                        return serviceNode;
                }
            }
            return null;
        }

        public void SetLastUsedSubmitAccount(SubmitAccount submitAccount, string key = "default")
        {
            if (submitAccount == null)
            {
                _lastUsedSubmitAccounts.Remove(key);
            }
            else
            {
                _lastUsedSubmitAccounts[key] = submitAccount.ID;
            }

            SaveLastUsed();
        }

        public virtual T GetLastUsedSubmitAccount<T>(string key = "default") where T : SubmitAccount
        {
            if(_lastUsedSubmitAccounts.TryGetValue(key, out var id))
            {
                foreach(var serviceNode in ServiceNodeManager.Current.ServiceNodes)
                {
                    var account = serviceNode.GetSubmitAccountById<T>(id);
                    if (account != null)
                        return account;
                }
            }
            return null;
        }

        protected virtual Task ServiceNodesLoaded(ServiceNodesLoadedEvent arg)
        {
            UpdateSubmitAccounts();
            return Task.CompletedTask;
        }

        protected virtual Task AccountAuthorized(ServiceAccountAuthorizedEvent evt)
        {
            UpdateSubmitAccounts();
            return Task.CompletedTask;
        }

        protected virtual Task AccountImport(ServiceAccountImportEvent evt)
        {
            UpdateSubmitAccounts();
            return Task.CompletedTask;
        }

        protected virtual Task ServiceNodeChanged(ServiceNodeChangedEvent evt)
        {
            return Task.CompletedTask;
        }

        public virtual void UpdateSubmitAccounts()
        {
        }

        public async Task<TransactionDownloadEvent<Transaction>> DownloadAccountTransactions(uint chainIndex, Action<AccountTransactionDownload> setup)
        {
            var evt = new TransactionDownloadEvent<Transaction>();

            foreach (var serviceNode in ServiceNodeManager.Current.ServiceNodes)
            {
                if (!serviceNode.HasUnlockedServiceAccount)
                    continue;

                if (!_dataAccountTransactionDownloads.TryGetValue((serviceNode.Id, chainIndex), out var download))
                {
                    download = new AccountTransactionDownload(serviceNode.AccountId, serviceNode.GetTransactionDownloadManager(chainIndex));
                    _dataAccountTransactionDownloads[(serviceNode.Id, chainIndex)] = download;
                }

                setup?.Invoke(download);
                var result = await download.DownloadTransactions();

                foreach (var td in result.Transactions)
                    td.Tag = serviceNode;

                evt.AddResult(result, download, serviceNode);
            }

            await UIApp.PubSub.PublishAsync(evt);

            return evt;
        }

        public async Task<TransactionDownloadEvent<Transaction>> DownloadAccountIndexTransactions(uint chainIndex, Chain.Index index, Action<AccountIndexTransactionDownload> setup)
        {
            var evt = new TransactionDownloadEvent<Transaction>();

            foreach (var serviceNode in ServiceNodeManager.Current.ServiceNodes)
            {
                if (!serviceNode.HasUnlockedServiceAccount)
                    continue;

                if (!_dataAccountIndexTransactionDownloads.TryGetValue((serviceNode.Id, chainIndex), out var indexLookup))
                {
                    indexLookup = new Dictionary<Chain.Index, AccountIndexTransactionDownload>();
                    _dataAccountIndexTransactionDownloads[(serviceNode.Id, chainIndex)] = indexLookup;
                }

                if (!indexLookup.TryGetValue(index, out var download))
                {
                    download = new AccountIndexTransactionDownload(serviceNode.AccountId, index, serviceNode.GetTransactionDownloadManager(chainIndex));
                    indexLookup[index] = download;
                }

                setup?.Invoke(download);
                var result = await download.DownloadTransactions();

                foreach (var td in result.Transactions)
                    td.Tag = serviceNode;

                evt.AddResult(result, download, serviceNode);
            }

            await UIApp.PubSub.PublishAsync(evt);

            return evt;
        }

        protected async Task<HeleusClientResponse> SetSubmitAccount(SubmitAccount submitAccount, bool requiresSecretKey = false)
        {
            var serviceNode = submitAccount?.ServiceNode;
            if (serviceNode == null)
            {
                return new HeleusClientResponse(HeleusClientResultTypes.ServiceNodeMissing);
            }

            var account = submitAccount.ServiceAccount;
            if (account == null)
            {
                return new HeleusClientResponse(HeleusClientResultTypes.ServiceNodeAccountMissing);
            }

            if (requiresSecretKey)
            {
                var secretKey = submitAccount.DefaultSecretKey;
                if (secretKey == null)
                    return new HeleusClientResponse(HeleusClientResultTypes.ServiceNodeSecretKeyMissing);
            }

            if (!await serviceNode.Client.SetServiceAccount(account, string.Empty, false))
                return new HeleusClientResponse(HeleusClientResultTypes.InternalError);

            return null;
        }

        public string GetRequestCode(ServiceNode serviceNode, uint chainIndex, string action, params object[] parameters)
        {
            var str = string.Join("/", parameters);
            return $"https://heleuscore.com/{Tr.Get("App.Name").ToLower()}/request/{action}/{serviceNode.ChainId}/{chainIndex}/{serviceNode.EndpointHex}/{str}";
        }
    }

    public abstract class AppBase<T> : AppBase where T : AppBase<T>, new()
    {
        public static readonly new T Current;

        static AppBase()
        {
            AppBase.Current = Current = new T();
        }
    }
}
