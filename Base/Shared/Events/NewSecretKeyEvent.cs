using Heleus.Chain;
using Heleus.Cryptography;

namespace Heleus.Apps.Shared
{
    public class NewSecretKeyEvent
    {
        public readonly SubmitAccount SubmitAccount;
        public readonly SecretKey SecretKey;

        public NewSecretKeyEvent(SubmitAccount submitAccount, SecretKey secretKey)
        {
            SubmitAccount = submitAccount;
            SecretKey = secretKey;
        }
    }
}
