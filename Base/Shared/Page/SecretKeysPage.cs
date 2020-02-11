using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Heleus.Chain;
using Heleus.Cryptography;

namespace Heleus.Apps.Shared
{
    public class SecretKeysPage : StackPage
    {
        readonly SubmitAccount _submitAccount;

        async Task Import(ButtonRow button)
        {
            if(await ConfirmAsync("Warning"))
                await Navigation.PushAsync(new ImportSecretKeyPage(_submitAccount));
        }

        async Task New(ButtonRow arg)
        {
            if (await ConfirmAsync("Warning"))
                await Navigation.PushAsync(new NewSecretKeyPage(_submitAccount, ServiceNodeManager.MinimumSecretKeyPassphraseLength));
        }

        async Task ShowAction(ButtonViewRow<SecretKeyView> button)
        {
            if (!(button.Tag is SecretKey secret))
                return;

            var cancel = Tr.Get("Common.Cancel");

            var actions = new List<string>();

            var export = T("Export");
            actions.Add(export);
            var def = T("MakeDefault");
            actions.Add(def);

#if DEBUG
            var activate = T("Activate");
            var deactivate = T("Deactivate");

            if (_submitAccount.SecretKeyManager.IsDisabled(secret.KeyInfo.SecretId))
                actions.Add(activate);
            else
                actions.Add(deactivate);
#endif

            var result = await DisplayActionSheet(Tr.Get("Common.Action"), cancel, null, actions.ToArray());
            if (result == export)
            {
                await Navigation.PushAsync(new ExportSecretKeyPage(secret));
            }
            if (result == def)
            {
                _submitAccount.SecretKeyManager.SetDefaultSecretKey(_submitAccount.Index, secret);
                UpdateViews();
            }

#if DEBUG
            if(result == activate)
            {
                _submitAccount.SecretKeyManager.EnableSecretKey(secret.KeyInfo.SecretId);
                await UIApp.PubSub.PublishAsync(new NewSecretKeyEvent(_submitAccount, secret));
            }
            else if (result == deactivate)
            {
                _submitAccount.SecretKeyManager.DisableSecretKey(secret.KeyInfo.SecretId);
                await UIApp.PubSub.PublishAsync(new NewSecretKeyEvent(_submitAccount, secret));
            }
#endif
        }

        public SecretKeysPage(SubmitAccount submitAccount) : base("SecretKeysPage")
        {
            Subscribe<NewSecretKeyEvent>(NewSecretKey);

            _submitAccount = submitAccount;

            AddTitleRow("Title");

            AddHeaderRow("New");
            AddTextRow("Info", Tr.Get("App.FullName"));

            AddButtonRow("Import", Import).SetDetailViewIcon(Icons.CloudDownload);
            AddButtonRow("NewSecretKey", New).SetDetailViewIcon(Icons.Plus);
            AddFooterRow();

            UpdateViews();
        }

        Task NewSecretKey(NewSecretKeyEvent arg)
        {
            UpdateViews();
            return Task.CompletedTask;
        }

        void UpdateViews()
        {
#if DEBUG
            var secrets = _submitAccount.SecretKeyManager.GetAllSecretKeys(_submitAccount.Index);
#else
            var secrets = _submitAccount.SecretKeyManager.GetSecretKeys(_submitAccount.Index);
#endif
            var defaultSecret = _submitAccount.SecretKeyManager.GetDefaultSecretKey(_submitAccount.Index);

            if (secrets.Count > 0)
            {
                if (GetRow("Secrets") == null)
                {
                    AddIndex = GetRow<HeaderRow>("New");
                    AddIndexBefore = true;

                    AddHeaderRow("Secrets");
                    AddRow(new SubmitAccountRow(_submitAccount), "SubmitAccount");
                    AddFooterRow();

                    AddIndex = null;
                }
            }

            var rows = GetHeaderSectionRows("Secrets");
            AddIndexBefore = false;
            AddIndex = rows.LastOrDefault() as StackRow;
            if (AddIndex == null)
                AddIndex = GetRow<HeaderRow>("SubmitAccount");

            foreach (var secret in secrets)
            {
                var found = false;
                foreach (var row in rows)
                {
                    if (row.Tag is SecretKey s)
                    {
                        var button = (row as ButtonViewRow<SecretKeyView>);
                        row.RowStyle = (s == defaultSecret) ? Theme.SubmitButton : Theme.RowButton;

                        if (s.KeyInfo.SecretId == secret.KeyInfo.SecretId)
                        {
#if DEBUG
                            button.View.UpdateActive(!_submitAccount.SecretKeyManager.IsDisabled(secret.KeyInfo.SecretId));
#endif
                            found = true;
                            break;
                        }
                    }
                }

                if (!found)
                {
                    var view = new SecretKeyView(secret.KeyInfo, false);
                    var button = AddButtonViewRow(view, ShowAction);
                    button.RowStyle = (secret == defaultSecret) ? Theme.SubmitButton : Theme.RowButton;
#if DEBUG
                    button.View.UpdateActive(!_submitAccount.SecretKeyManager.IsDisabled(secret.KeyInfo.SecretId));
#endif
                    button.Tag = secret;

                    AddIndex = button;
                }
            }

            AddIndex = null;
        }
    }
}
