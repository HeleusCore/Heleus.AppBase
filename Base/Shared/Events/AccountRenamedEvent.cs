using Heleus.Chain;
using Heleus.Cryptography;

namespace Heleus.Apps.Shared
{
    public class AccountRenamedEvent
    {
        public readonly KeyStore KeyStore;

        public AccountRenamedEvent(KeyStore keyStore)
        {
            KeyStore = keyStore;
        }
    }
}
