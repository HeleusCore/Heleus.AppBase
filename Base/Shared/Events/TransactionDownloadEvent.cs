using System.Collections.Generic;
using Heleus.Network.Client;
using Heleus.Operations;
using Heleus.Transactions;

namespace Heleus.Apps.Shared
{
    public enum TransactionSortMode
    {
        TimestampAscending,
        TimestampDescening
    }

    public class TransactionDownloadEvent<T> where T : Operation
    {
        public class Item
        {
            public readonly TransactionDownloadResult<T> Result;
            public readonly TransactionDownload<T> Download;
            public readonly ServiceNode ServiceNode;

            public Item(TransactionDownloadResult<T> result, TransactionDownload<T> download, ServiceNode serviceNode)
            {
                Result = result;
                Download = download;
                ServiceNode = serviceNode;
            }
        }

        readonly List<Item> _items = new List<Item>();
        public IReadOnlyList<Item> Items => _items;

        public void AddResult(TransactionDownloadResult<T> result, TransactionDownload<T> download, ServiceNode serviceNode)
        {
            _items.Add(new Item(result, download, serviceNode));
        }

        public bool HasMore
        {
            get
            {
                foreach(var item in _items)
                {
                    if (item.Result.NextPreviousId != Operation.InvalidTransactionId)
                        return true;
                }

                return false;
            }
        }

        public List<TransactionDownloadData<T>> GetSortedTransactions(TransactionSortMode sortMode)
        {
            var result = new List<TransactionDownloadData<T>>();

            foreach(var item in _items)
            {
                foreach(var transactionItem in item.Result.Transactions)
                {
                    result.Add(transactionItem);
                }
            }

            if(sortMode == TransactionSortMode.TimestampAscending)
                result.Sort((a, b) => a.Transaction.Timestamp.CompareTo(b.Transaction.Timestamp));
            else if (sortMode == TransactionSortMode.TimestampDescening)
                result.Sort((a, b) => b.Transaction.Timestamp.CompareTo(a.Transaction.Timestamp));

            return result;
        }
    }
}
