using System;
using System.Threading.Tasks;
using Heleus.Base;
using Heleus.Network.Client;
using Heleus.Network.Client.Record;
using Heleus.Transactions;
using Heleus.Transactions.Features;

namespace Heleus.Apps.Shared
{
    public enum DecryptedDataRecordState
    {
        None,
        DownloadFailed,
        Decrypted,
        SecretKeyMissing,
        DecryptionError
    }

    public enum DecryptedRecordDataSource
    {
        Attachement,
        DataFeature
    }

    public class DecryptedRecordData<T> where T : Record
    {
        public readonly TransactionDownloadData<Transaction> TransactionDownload;
        public Transaction Transaction => TransactionDownload.Transaction;
        public readonly ServiceNode ServiceNode;

        public EncrytpedRecord<T> EncryptedRecord { get; private set; }
        public T Record { get; private set; }

        public DecryptedDataRecordState DecryptetState { get; private set; } = DecryptedDataRecordState.None;

        readonly string _name;
        readonly Chain.Index _index;
        readonly DecryptedRecordDataSource _source;
        readonly short _dataItemIndex;

        public DecryptedRecordData(TransactionDownloadData<Transaction> transaction, ServiceNode serviceNode, Chain.Index index, string name, DecryptedRecordDataSource source, short dataItemIndex = 0)
        {
            _name = name;
            _index = index;
            _source = source;
            _dataItemIndex = dataItemIndex;

            TransactionDownload = transaction;
            ServiceNode = serviceNode;

            ProcessTransactionData();

            if (EncryptedRecord == null)
            {
                DecryptetState = DecryptedDataRecordState.DownloadFailed;
                return;
            }

            var recordData = TransactionDownload.GetDecryptedData(_name);
            if (recordData != null)
            {
                Record = (T)Activator.CreateInstance(typeof(T), new Unpacker(recordData));
                DecryptetState = DecryptedDataRecordState.Decrypted;
            }
        }

        void ProcessTransactionData()
        {
            if(_source == DecryptedRecordDataSource.Attachement)
            {
                if (TransactionDownload.Transaction is AttachementDataTransaction)
                {
                    if (EncryptedRecord == null)
                    {
                        var noteAttachement = TransactionDownload.GetAttachementData(_name);
                        if (noteAttachement != null)
                        {
                            try
                            {
                                using (var unpacker = new Unpacker(noteAttachement))
                                {
                                    EncryptedRecord = new EncrytpedRecord<T>(unpacker);
                                }
                            }
                            catch
                            {
                            }
                        }
                    }
                }
            }
            else if (_source == DecryptedRecordDataSource.DataFeature)
            {
                if (TransactionDownload.Transaction.TryGetFeature<Data>(Data.FeatureId, out var data) && data.GetItem(_dataItemIndex, out var item))
                {
                    using (var unpacker = new Unpacker(item.BinaryValue))
                    {
                        EncryptedRecord = new EncrytpedRecord<T>(unpacker);
                    }
                }
            }
        }

        public async Task<DecryptedDataRecordState> Decrypt()
        {
            if (EncryptedRecord == null)
            {
                if (_source == DecryptedRecordDataSource.Attachement)
                    await ServiceNode.GetTransactionDownloadManager(Transaction.ChainIndex).DownloadTransactionAttachement(TransactionDownload);

                ProcessTransactionData();

                if (EncryptedRecord == null)
                {
                    DecryptetState = DecryptedDataRecordState.DownloadFailed;
                    return DecryptetState;
                }
            }

            if (Record != null)
                return DecryptetState;

            var data = TransactionDownload.GetDecryptedData(_name);
            if (data != null)
            {
                Record = (T)Activator.CreateInstance(typeof(T));
                DecryptetState = DecryptedDataRecordState.Decrypted;
                return DecryptetState;
            }

            var secrets = ServiceNode.GetSecretKeys(_index, EncryptedRecord.KeyInfo.SecretId);
            if (secrets.Count == 0)
            {
                DecryptetState = DecryptedDataRecordState.SecretKeyMissing;
                return DecryptetState;
            }

            foreach (var secret in secrets)
            {
                Record = await EncryptedRecord.GetRecord(secret);
                if (Record != null)
                {
                    TransactionDownload.AddDecryptedAttachement(_name, Record.ToByteArray());
                    await TransactionDownload.TransactionManager.StoreDecryptedTransactionData(TransactionDownload);

                    DecryptetState = DecryptedDataRecordState.Decrypted;
                    return DecryptetState;
                }
            }

            DecryptetState = DecryptedDataRecordState.DecryptionError;
            return DecryptetState;
        }
    }
}
