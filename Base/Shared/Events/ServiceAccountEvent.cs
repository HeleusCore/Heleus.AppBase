using Heleus.Chain;
using Heleus.Cryptography;

namespace Heleus.Apps.Shared
{

    public class ServiceAccountEvent : ServiceNodeEvent
    {
        public readonly ServiceAccountKeyStore ServiceAccount;

        protected ServiceAccountEvent(ServiceAccountKeyStore serviceAccount, ServiceNode serviceEndpoint) : base(serviceEndpoint)
        {
            ServiceAccount = serviceAccount;
        }
    }

    public class ServiceAccountImportEvent : ServiceAccountEvent
    {
        public ServiceAccountImportEvent(ServiceAccountKeyStore serviceAccount, ServiceNode serviceEndpoint) : base(serviceAccount, serviceEndpoint)
        {
        }
    }
}
