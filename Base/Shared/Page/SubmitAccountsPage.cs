using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Heleus.Apps.Shared
{
    public class SubmitAccountsPage<T> : StackPage where T : SubmitAccount
    {
        readonly Action<T> _select;

        void AddAccountRow(T submitAccount)
        {
            var row = new SubmitAccountButtonRow<T>(submitAccount, this, Account)
            {
                Tag = submitAccount
            };

            AddRow(row);
        }

        async Task Account(SubmitAccountButtonRow<T> button)
        {
            var submitAccount = button.SubmitAccount;

            if (_select != null)
            {
                if(submitAccount.RequiresSecretKey)
                {
                    var cancel = Tr.Get("Common.Cancel");
                    var select = T("Select");
                    var secret = T("SelectSecrets");

                    var result = await DisplayActionSheet(Tr.Get("Common.Action"), cancel, null, select, secret);
                    if(result == select)
                    {
                        _select.Invoke(submitAccount);
                        await Navigation.PopAsync();
                    }
                    else if (result == secret)
                    {
                        await Navigation.PushAsync(new SecretKeysPage(submitAccount));
                    }
                }
                else
                {
                    _select.Invoke(submitAccount);
                    await Navigation.PopAsync();
                }

                return;
            }

            if(submitAccount.RequiresSecretKey)
                await Navigation.PushAsync(new SecretKeysPage(submitAccount));
        }

        public SubmitAccountsPage(List<T> submitAccounts) : base("SubmitAccountsPage")
        {
            AddTitleRow("Title");

            AddHeaderRow("SubmitAccounts");

            foreach (var submitAccount in submitAccounts)
                AddAccountRow(submitAccount);

            AddFooterRow();
        }

        public SubmitAccountsPage(List<T> submitAccounts, Action<T> select) : this(submitAccounts)
        {
            _select = select;
        }
    }

    public class SubmitAccountsPage : SubmitAccountsPage<SubmitAccount>
    {
        public SubmitAccountsPage(List<SubmitAccount> submitAccounts) : base(submitAccounts)
        {
        }

        public SubmitAccountsPage(List<SubmitAccount> submitAccounts, Action<SubmitAccount> select) : base(submitAccounts, select)
        {
        }
    }
}
