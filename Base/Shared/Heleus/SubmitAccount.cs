using System;
using Heleus.Base;
using Heleus.Chain;
using Heleus.Cryptography;

namespace Heleus.Apps.Shared
{
    public class SubmitAccount
    {
        public readonly ServiceNode ServiceNode;
        public readonly short KeyIndex;
        public readonly Chain.Index Index;
        public readonly bool RequiresSecretKey;

        public long AccountId => ServiceNode.AccountId;
        public int ChainId => ServiceNode.ChainId;

        string _id;
        public string ID
        {
            get
            {
                if(_id == null)
                    _id = $"{Index.HexString}.{KeyIndex}.{ServiceNode.Id}";

                return _id;
            }
        }

        public KeyStore ServiceAccount
        {
            get
            {
                ServiceNode.ServiceAccounts.TryGetValue(KeyIndex, out var account);
                return account;
            }
        }

        public SecretKeyManager SecretKeyManager
        {
            get
            {
                ServiceNode.SecretKeyManagers.TryGetValue(KeyIndex, out var manager);
                return manager;
            }
        }

        public SecretKey DefaultSecretKey
        {
            get
            {
                return SecretKeyManager?.GetDefaultSecretKey(Index);
            }
        }

        public virtual string Name
        {
            get
            {
                return ServiceAccount.Name;
            }
        }

        public virtual string Detail
        {
            get
            {
                return ServiceNode.Name;
            }
        }

        public SubmitAccount(ServiceNode serviceNode, short keyIndex, Chain.Index index, bool requiresSecretKey)
        {
            if (serviceNode == null || index == null)
                throw new ArgumentException();

            ServiceNode = serviceNode;
            KeyIndex = keyIndex;
            Index = index;
            RequiresSecretKey = requiresSecretKey;
        }
    }

    public class GroupSubmitAccount : SubmitAccount
    {
        public readonly long GroupId;

        public GroupSubmitAccount(ServiceNode serviceNode, long groupId, short keyIndex, Chain.Index index, bool requiresSecretKey) : base(serviceNode, keyIndex, index, requiresSecretKey)
        {
            GroupId = groupId;
        }
    }
}
