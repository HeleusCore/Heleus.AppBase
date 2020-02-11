using System;
using System.Text;
using System.Threading.Tasks;
using Heleus.Base;
using Heleus.Cryptography;

namespace Heleus.Apps.Shared
{
    public class DataStorage
    {
        public static long HashName(string name)
        {
            var hash = Hash.Generate(HashTypes.Sha1, Encoding.UTF8.GetBytes(name));
            return BitConverter.ToInt64(hash.RawData.Array, 5);
        }

        public readonly long ChainId;

        readonly Lazy<DiscStorage> _transactionStorage;

        public DataStorage(Storage storage, int chainId)
        {
            ChainId = chainId;
            _transactionStorage = new Lazy<DiscStorage>(() => new DiscStorage(storage, $"datastorage_{chainId}", 64, 0, DiscStorageFlags.UnsortedDynamicIndex));
        }

        public Task<T> Load<T>(string name)
        {
            return Task.Run(() =>
            {
                try
                {
                    var storage = _transactionStorage.Value;
                    var id = HashName(name);
                    var data = storage.GetBlockData(id);
                    if (data != null)
                        return (T)Activator.CreateInstance(typeof(T), new Unpacker(data));

                }
                catch (Exception ex)
                {
                    Log.IgnoreException(ex);
                }

                return default(T);
            });
        }

        public Task<bool> Save(string name, IPackable packable)
        {
            return Task.Run(() =>
            {
                try
                {
                    var storage = _transactionStorage.Value;
                    var id = HashName(name);
                    using (var packer = new Packer())
                    {
                        packable.Pack(packer);
                        var data = packer.ToByteArray();
                        if (data != null)
                        {
                            if (storage.ContainsIndex(id))
                                storage.UpdateEntry(id, data);
                            else
                                storage.AddEntry(id, data);
                            storage.Commit();
                        }
                    }
                }
                catch (Exception ex)
                {
                    Log.IgnoreException(ex);
                }

                return false;
            });
        }
    }
}
