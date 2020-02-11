using Heleus.Chain;
using Heleus.Cryptography;

namespace Heleus.Apps.Shared
{
    public class AccountEvent
    {
        public readonly KeyStore Account;

        protected AccountEvent(KeyStore account)
        {
            Account = account;
        }
    }
}
