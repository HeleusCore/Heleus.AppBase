using System;
using Heleus.Chain;
using Heleus.Cryptography;

namespace Heleus.Apps.Shared
{
    public class ServiceAccountAuthorizedEvent : ServiceAccountEvent
    {
        public ServiceAccountAuthorizedEvent(ServiceAccountKeyStore serviceAccount, ServiceNode serviceEndpoint) : base(serviceAccount, serviceEndpoint)
        {
        }
    }
}
