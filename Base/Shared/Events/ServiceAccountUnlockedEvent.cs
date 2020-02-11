using Heleus.Chain;
using Heleus.Cryptography;
using Heleus.Network.Client;

namespace Heleus.Apps.Shared
{
    public class ServiceAccountUnlockedEvent : ServiceAccountEvent
    {
        public ServiceAccountUnlockedEvent(ServiceAccountKeyStore serviceAccount, ServiceNode serviceEndpoint) : base(serviceAccount, serviceEndpoint)
        {
        }
    }
}
