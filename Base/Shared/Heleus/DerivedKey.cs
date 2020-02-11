using System.Threading.Tasks;
using Heleus.Cryptography;
using Heleus.Base;
using System;

namespace Heleus.Apps.Shared
{
    public class DerivedKey : IPackable
    {
        public readonly long AccountId;
        public readonly int ChainId;
        public readonly short KeyIndex;
        public readonly Encryption Encryption;

        Key _key;

        public DerivedKey(long accountId, int chainId, short keyIndex, Encryption encryption)
        {
            AccountId = accountId;
            ChainId = chainId;
            KeyIndex = keyIndex;
            Encryption = encryption;
        }

        public DerivedKey(Unpacker unpacker)
        {
            unpacker.Unpack(out AccountId);
            unpacker.Unpack(out ChainId);
            unpacker.Unpack(out KeyIndex);
            Encryption = Encryption.Restore(new ArraySegment<byte>(unpacker.UnpackByteArray()));
        }

        public void Pack(Packer packer)
        {
            packer.Pack(AccountId);
            packer.Pack(ChainId);
            packer.Pack(KeyIndex);
            packer.Pack(Encryption.Data);
        }

        public Task<Key> DecryptKey(string password)
        {
            if (_key != null)
                return Task.FromResult(_key);

            return Task.Run(() =>
            {
                try
                {
                    _key = Key.Restore(Encryption.Decrypt(password));
                }
                catch { }

                return _key;
            });
        }

        public byte[] ToByteArray()
        {
            using (var packer = new Packer())
            {
                Pack(packer);

                return packer.ToByteArray();
            }
        }
    }
}
