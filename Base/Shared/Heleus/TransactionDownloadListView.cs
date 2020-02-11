using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Heleus.Network.Client;
using Heleus.Operations;
using Heleus.Transactions;
using Xamarin.Forms;

namespace Heleus.Apps.Shared
{
    public abstract class TransactionDownloadListView<RowType> where RowType : StackRow
    {
        readonly View _header;
        protected readonly StackPage _page;
        readonly TransactionDownload<Transaction> _download;
        ButtonRow _more;

        public IReadOnlyList<RowType> Rows => new List<RowType>(_rows);
        readonly List<RowType> _rows = new List<RowType>();

        protected TransactionDownloadListView(TransactionDownload<Transaction> download, StackPage page, View header)
        {
            _page = page;
            _header = header;
            _download = download;
        }

        protected abstract RowType AddRow(StackPage page, TransactionDownloadData<Transaction> transaction);
        protected abstract long GetTransactionId(RowType row);

        protected abstract Task More(ButtonRow button);


        public void Clear()
        {
            foreach (var row in _rows)
            {
                _page.RemoveView(row);
            }
            _rows.Clear();
        }

        public void UpdateTransactions()
        {
            var transactions = new List<TransactionDownloadData<Transaction>>(_download.Transactions.Values);
            transactions.Reverse();

            if (transactions.Count == 0)
            {
                if (_page.GetRow("NoListViewItemFound") == null)
                {
                    _page.AddIndex = _header;
                    _page.AddInfoRow("NoListViewItemFound");
                }
            }
            else
            {
                _page.RemoveView(_page.GetRow("NoListViewItemFound"));

                var diff = ListDiff.Compute(_rows, transactions, (a, b) => GetTransactionId(a) == b.TransactionId);

                diff.Process(_rows, transactions,
                (message) =>
                {
                    _page.RemoveView(message);
                    return true;
                },
                (idx, item) =>
                {
                    _page.AddIndexBefore = false;
                    if (idx == 0)
                        _page.AddIndex = _header;
                    else
                        _page.AddIndex = _rows[idx - 1];

                    var r = AddRow(_page, item);

                    _rows.Insert(idx, r);
                });

                _page.AddIndex = _rows.LastOrDefault();

                if (_page.AddIndex != null)
                {
                    var last = _download.Transactions.FirstOrDefault();
                    if (last.Value != null && _download.GetPreviousTransactionId(last.Value.Transaction) != Operation.InvalidTransactionId )
                    {
                        if (_more == null)
                        {
                            _more = _page.AddButtonRow("More", More);
                        }
                    }
                    else
                    {
                        _page.RemoveView(_more);
                        _more = null;
                    }
                }
            }
        }
    }
}
