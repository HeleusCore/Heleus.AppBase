using System;
using System.Collections.Generic;
using Heleus.Base;
using Heleus.Chain;
using Heleus.Cryptography;

namespace Heleus.Apps.Shared
{
	public class SecretKeyManager
	{
		public class SecretKeyContainer
		{
			public readonly SecretKey SecretKey;
			public readonly Chain.Index Index;

			public SecretKeyContainer(SecretKey secretKey, Chain.Index index)
			{
				SecretKey = secretKey;
				Index = index;
			}
		}

        static List<SecretKeyManager> _managers = new List<SecretKeyManager>();
        public static IReadOnlyList<SecretKeyManager> Managers => _managers;

        public readonly ServiceNode ServiceNode;
        public readonly ServiceAccountKeyStore Account;

        readonly ISettingsCredential _credentials;

		readonly Dictionary<Chain.Index, Dictionary<ulong, SecretKeyContainer>> _secretKeys = new Dictionary<Chain.Index, Dictionary<ulong, SecretKeyContainer>>();
		readonly Dictionary<Chain.Index, ulong> _defaultKeys = new Dictionary<Chain.Index, ulong>();
        readonly HashSet<ulong> _disabledKeys = new HashSet<ulong>();

        public string Id => $"SecretKeys.{ServiceNode.Id}.{Account.AccountId}.{Account.KeyIndex}";

		public SecretKeyManager(ServiceNode serviceNode, ServiceAccountKeyStore account, ISettingsCredential credentials)
		{
            ServiceNode = serviceNode;
            Account = account;
            _credentials = credentials;
            _managers.Add(this);

            try
            {
                var data64 = _credentials?.GetCredential(Id);
                if (!string.IsNullOrEmpty(data64))
                {
                    var data = Convert.FromBase64String(data64);
                    using (var unpacker = new Unpacker(data))
                    {
                        var count = unpacker.UnpackInt();
                        for (var i = 0; i < count; i++)
                        {
                            var index = new Chain.Index(unpacker);
                            var secretKey = new SecretKey(unpacker);

                            AddSecretKey(index, secretKey, false, false);
                        }

                        count = unpacker.UnpackInt();
                        for (var i = 0; i < count; i++)
                        {
                            var key = new Chain.Index(unpacker);
                            var value = unpacker.UnpackULong();

                            _defaultKeys[key] = value;
                        }

                        unpacker.Unpack(_disabledKeys);
                    }
                }
            }
            catch (Exception ex)
            {
                Log.IgnoreException(ex);
            }
        }

        void Save()
        {
            using (var packer = new Packer())
            {
                var secretKeys = new List<SecretKeyContainer>();
                foreach (var lookup in _secretKeys.Values)
                {
                    foreach(var container in lookup.Values)
                        secretKeys.Add(container);
                }

                var count = secretKeys.Count;
                packer.Pack(count);

                foreach(var container in secretKeys)
                {
                    packer.Pack(container.Index);
                    packer.Pack(container.SecretKey);
                }


                count = _defaultKeys.Count;
                packer.Pack(count);

                foreach(var @default in _defaultKeys)
                {
                    packer.Pack(@default.Key);
                    packer.Pack(@default.Value);
                }

                packer.Pack(_disabledKeys);

                var data = packer.ToByteArray();
                var data64 = Convert.ToBase64String(data);

                _credentials?.AddCredential(Id, data64);
            }
        }

        public void AddSecretKey(Chain.Index index, SecretKey secretKey, bool makeDefaultIfNotAvailable = true)
        {
            AddSecretKey(index, secretKey, makeDefaultIfNotAvailable, true);
        }

        void AddSecretKey(Chain.Index index, SecretKey secretKey, bool makeDefaultIfNotAvailable, bool save)
		{
            if (secretKey.KeyInfo.ChainId != Account.ChainId)
                return;

			if (!_secretKeys.TryGetValue(index, out var lookup))
			{
				lookup = new Dictionary<ulong, SecretKeyContainer>();
				_secretKeys[index] = lookup;
			}

			var secretId = secretKey.KeyInfo.SecretId;
			var hasDefault = _defaultKeys.ContainsKey(index);
			lookup[secretId] = new SecretKeyContainer(secretKey, index);

			if (!hasDefault && makeDefaultIfNotAvailable)
				_defaultKeys[index] = secretId;

            if(save)
                Save();
		}

#if DEBUG
        public void DisableSecretKey(ulong secretId)
        {
            _disabledKeys.Add(secretId);
            Save();
        }

        public void EnableSecretKey(ulong secretId)
        {
            _disabledKeys.Remove(secretId);
            Save();
        }
#endif

        public bool SetDefaultSecretKey(Chain.Index index, ulong secretId)
		{
			var key = GetSecretKey(index, secretId);
			if (key != null)
			{
				_defaultKeys[index] = secretId;
                Save();
				return true;
			}

			return false;
		}

		public bool SetDefaultSecretKey(Chain.Index index, SecretKey secretKey, bool import = true)
		{
			var secretId = secretKey.KeyInfo.SecretId;
			var key = GetSecretKey(index, secretId);
			if (key != null)
			{
				_defaultKeys[index] = secretId;
                Save();
				return true;
			}

			if (import)
			{
				AddSecretKey(index, secretKey);
				_defaultKeys[index] = secretId;
                Save();
				return true;
			}

			return false;
		}

		public ICollection<Chain.Index> GetIndices()
		{
			return _secretKeys.Keys;
		}

#if DEBUG
        public bool IsDisabled(ulong secretId)
        {
            return _disabledKeys.Contains(secretId);
        }

        public ICollection<SecretKey> GetAllSecretKeys(Chain.Index index)
        {
            var result = new List<SecretKey>();
            if (_secretKeys.TryGetValue(index, out var lookup))
            {
                foreach (var value in lookup.Values)
                {
                    result.Add(value.SecretKey);
                }
            }

            return result;
        }
#endif

        public ICollection<SecretKey> GetSecretKeys(Chain.Index index)
		{
            var result = new List<SecretKey>();
            if (_secretKeys.TryGetValue(index, out var lookup))
			{
                foreach(var value in lookup.Values)
                {
                    var secretKey = value.SecretKey;

#if DEBUG
                    if (_disabledKeys.Contains(secretKey.KeyInfo.SecretId))
                        continue;
#endif
                    result.Add(secretKey);
                }
			}

            return result;
		}

        public List<SecretKey> GetSecretKeys(Chain.Index index, SecretKeyInfoTypes secretKeyInfoTypes)
		{
            var result = new List<SecretKey>();

			if (_secretKeys.TryGetValue(index, out var lookup))
			{
                foreach(var key in lookup.Values)
                {
#if DEBUG
                    if (_disabledKeys.Contains(key.SecretKey.KeyInfo.SecretId))
                        continue;
#endif

                    if (key.SecretKey.SecretKeyInfoType == secretKeyInfoTypes)
                        result.Add(key.SecretKey);
                }
			}

			return result;
		}

        public bool HasSecretKeyType(Chain.Index index, SecretKeyInfoTypes secretKeyInfoTypes)
        {
            if (_secretKeys.TryGetValue(index, out var lookup))
            {
                foreach (var key in lookup.Values)
                {
#if DEBUG
                    if (_disabledKeys.Contains(key.SecretKey.KeyInfo.SecretId))
                        continue;
#endif
                    if (key.SecretKey.SecretKeyInfoType == secretKeyInfoTypes)
                        return true;
                }
            }

            return false;
        }

        public SecretKey GetSecretKey(Chain.Index index, ulong secretId)
        {
#if DEBUG
            if (_disabledKeys.Contains(secretId))
                return null;
#endif
            if (_secretKeys.TryGetValue(index, out var lookup))
            {
                lookup.TryGetValue(secretId, out var secretKey);
                return secretKey?.SecretKey;
            }

            return null;
        }

        public SecretKey GetDefaultSecretKey(Chain.Index index)
		{
            if (_secretKeys.TryGetValue(index, out var lookup))
			{
				if (_defaultKeys.TryGetValue(index, out var secretId))
				{
#if DEBUG
                    if (_disabledKeys.Contains(secretId))
                        return null;
#endif

                    lookup.TryGetValue(secretId, out var secretKey);
					return secretKey?.SecretKey;
				}
			}

			return null;
		}
	}
}
