using Heleus.Apps.Shared;
using Heleus.Base;
using Heleus.Transactions;

namespace Heleus.Apps.Shared
{
    public class DataTransactionView : RowView
    {
        public DataTransactionView(Transaction transaction) : base("DataTransactionView")
        {
            AddRow("TransactionId", transaction.TransactionId.ToString());
            if(transaction is DataTransaction dataTransaction)
                AddRow("AccountId", Tr.Get("Common.AccountIdTemplate", transaction.AccountId, dataTransaction.SignKeyIndex));

            AddRow("ChainId", transaction.TargetChainId.ToString());
            AddLastRow("Date", Time.DateTimeString(transaction.Timestamp));
        }
    }
}
