using Heleus.Chain;
using Heleus.Cryptography;
using Heleus.Network.Client;

namespace Heleus.Apps.Shared
{
    public class AccountImportEvent : AccountEvent
    {
        public AccountImportEvent(KeyStore account) : base(account)
        {
        }
    }
}
