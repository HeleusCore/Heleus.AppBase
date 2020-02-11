using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Heleus.Chain;
using Heleus.Cryptography;

namespace Heleus.Apps.Shared
{
    public class MissingSecretKeysPage<T> : StackPage where T : SubmitAccount
    {
        readonly List<T> _submitAccounts;
        readonly SubmitAccountButtonRow<T> _submitAccount;

        public MissingSecretKeysPage(ICollection<SecretKeyInfo> missingKeys, List<T> submitAccounts) : base("MissingSecretKeysPage")
        {
            Subscribe<NewSecretKeyEvent>(NewKey);

            _submitAccounts = submitAccounts;

            AddTitleRow("Title");

            AddHeaderRow("SubmitAccount");
            _submitAccount = AddRow(new SubmitAccountButtonRow<T>(submitAccounts.FirstOrDefault(), this, SelectSubmitAccount));
            AddFooterRow();

            AddHeaderRow("MissingSecretKeys");

            foreach (var missingKey in missingKeys)
            {
                var button = AddButtonViewRow(new SecretKeyView(missingKey, true), Import);
                button.Tag = missingKey;
            }

            AddFooterRow();
        }

        Task NewKey(NewSecretKeyEvent arg)
        {
            var rows = GetHeaderSectionRows("MissingSecretKeys");
            var submitAccount = _submitAccount.SubmitAccount;
            foreach (var row in rows)
            {
                if (row.Tag is SecretKeyInfo secretKeyInfo)
                {
                    if (submitAccount.SecretKeyManager.GetSecretKey(submitAccount.Index, secretKeyInfo.SecretId) != null)
                        RemoveView(row);
                }
            }

            return Task.CompletedTask;
        }

        async Task Import(ButtonViewRow<SecretKeyView> arg)
        {
            var submitAccount = _submitAccount.SubmitAccount;
            if (submitAccount != null)
                await Navigation.PushAsync(new ImportSecretKeyPage(submitAccount));
        }

        async Task SelectSubmitAccount(SubmitAccountButtonRow<T> arg)
        {
            await Navigation.PushAsync(new SubmitAccountsPage<T>(_submitAccounts, (submitAccount) =>
            {
                arg.SubmitAccount = submitAccount;
            }));
        }
    }
}
