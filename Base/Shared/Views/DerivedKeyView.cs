namespace Heleus.Apps.Shared
{
    public class DerivedKeyView : RowView
    {
        readonly ExtLabel _serviceId;
        readonly ExtLabel _accountId;

        public DerivedKeyView(DerivedKey derivedKey) : base("DerivedKeyView")
        {
            (_, _accountId) = AddRow("AccountId", null);
            (_, _serviceId) = AddLastRow("ChainId", null);

            Update(derivedKey);
        }

        public void Update(DerivedKey derivedKey)
        {
            if (derivedKey == null)
            {
                _serviceId.Text = "-";
                _accountId.Text = "-";
            }
            else
            {
                _serviceId.Text = derivedKey.ChainId.ToString();
                _accountId.Text = Tr.Get("Common.AccountIdTemplate", derivedKey.AccountId, derivedKey.KeyIndex);
            }
        }
    }
}
