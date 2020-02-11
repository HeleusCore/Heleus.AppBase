using System.Threading;
using System.Threading.Tasks;
using Heleus.Base;
using Heleus.Network.Client;

namespace Heleus.Apps.Shared
{
    public abstract class NodeBase : IPackable
    {
        public int ChainId => ServiceNode.ChainId;
        public long AccountId => ServiceNode.AccountId;
        public readonly ServiceNode ServiceNode;

        protected HeleusClient _client => ServiceNode.Client;
        readonly SemaphoreSlim _saveLock = new SemaphoreSlim(1);

        public async Task SaveAsync()
        {
            await _saveLock.WaitAsync();

            try
            {
                using (var packer = new Packer())
                {
                    Pack(packer);

                    var data = packer.ToByteArray();
                    await ServiceNode.CacheStorage.WriteFileBytesAsync(GetType().Name, data);
                }
            }
            catch
            {
                _ = SaveAsync();
            }

            _saveLock.Release();
        }

        protected NodeBase(ServiceNode serviceNode)
        {
            ServiceNode = serviceNode;
        }

        public abstract void Pack(Packer packer);

        protected async Task<HeleusClientResponse> SetSubmitAccount(SubmitAccount submitAccount, bool requiresSecretKey = false)
        {
            var serviceNode = submitAccount?.ServiceNode;
            if (serviceNode == null || serviceNode != ServiceNode)
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

        public string GetRequestCode(string action, params object[] parameters)
        {
            var str = string.Join("/", parameters);
            return $"https://heleuscore.com/{Tr.Get("App.Name").ToLower()}/request/{action}/{ChainId}/{ServiceNode.EndpointHex}/{str}";
        }
    }
}
