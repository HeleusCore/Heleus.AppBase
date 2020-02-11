using System.Collections.Generic;
using System.Threading.Tasks;
using Heleus.Chain;
using Heleus.Network.Results;
using Heleus.ProfileService;
using Heleus.Transactions.Features;

namespace Heleus.Apps.Shared
{
    public class ServiceProfileSearch : ProfileSearch
    {
        public readonly ServiceNode ServiceNode;

        public ServiceProfileSearch(ServiceNode serviceNode)
        {
            ServiceNode = serviceNode;
        }

        protected override async Task<bool> IsValidServiceAccount(long accountId)
        {
            var result = await PreviousAccountTransaction.DownloadLastTransactionInfo(ServiceNode.Client, ChainType.Service, ServiceNode.ChainId, 0, accountId);
            return (result != null && result.ResultType == ResultTypes.Ok);
        }

        protected override async Task<List<ProfileInfo>> ProcessProfiles(List<ProfileInfo> profiles)
        {
            var result = new List<ProfileInfo>();
            var ids = new List<long>(profiles.Count);
            foreach (var profile in profiles)
                ids.Add(profile.AccountId);

            var download = await PreviousAccountTransaction.DownloadLastTransactionInfoBatch(ServiceNode.Client, ChainType.Service, ServiceNode.ChainId, 0, ids);

            var batch = download?.Item;
            if (batch != null)
            {
                for (var i = 0; i < profiles.Count; i++)
                {
                    var profile = profiles[i];
                    (var available, _, _) = batch.GetInfo(i);

                    if (available)
                        result.Add(profile);
                }
            }

            return result;
        }
    }
}
