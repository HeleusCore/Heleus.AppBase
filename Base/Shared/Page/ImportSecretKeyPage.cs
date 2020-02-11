using System;
using System.Threading.Tasks;
using Heleus.Base;
using Heleus.Chain;
using Heleus.Cryptography;

namespace Heleus.Apps.Shared
{
    public class ImportSecretKeyPage : StackPage
    {
        readonly SubmitAccount _submitAccount;

        readonly SecretKeyView _keyView;
        readonly EditorRow _key;
        readonly EntryRow _password;

        async Task Import(ButtonRow button)
        {
            try
            {
                var exportedKey = GetSecretKey();
                var secretKey = exportedKey?.Decrypt(_password.Edit.Text);
                if (secretKey != null)
                {
                    if(secretKey.KeyInfo.ChainId == _submitAccount.ServiceAccount.ChainId)
                    {
                        if (_submitAccount.SecretKeyManager.GetSecretKey(_submitAccount.Index, secretKey.KeyInfo.SecretId) == null)
                        {
                            _submitAccount.SecretKeyManager.AddSecretKey(_submitAccount.Index, secretKey);
                            await UIApp.PubSub.PublishAsync(new NewSecretKeyEvent(_submitAccount, secretKey));

                            await MessageAsync("Success");
                            await Navigation.PopAsync();
                        }
                        else
                        {
                            await ErrorAsync("AlreadyAvailable");
                        }
                    }
                    else
                    {
                        await ErrorAsync("WrongChainId");
                    }
                }
                else
                {
                    await ErrorAsync("WrongPassword");
                }
            }
            catch(Exception ex)
            {
                Log.IgnoreException(ex);
                await ErrorAsync("Failure");
            }
        }

        public ImportSecretKeyPage(SubmitAccount submitAccount) : base("ImportSecretKeyPage")
        {
            _submitAccount = submitAccount;

            AddTitleRow("Title");

            AddHeaderRow("PasteInfo");
            _key = AddEditorRow(null, null);
            _key.SetDetailViewIcon(Icons.Key);

            _keyView = new SecretKeyView(null, true);
            AddViewRow(_keyView);

            AddFooterRow();

            Status.Add(_key.Edit, T("KeyStatus"), (sv, edit, newText, oldText) =>
            {
                try
                {
                    var secretKey = GetSecretKey();
                    if(secretKey != null)
                    {
                        _keyView.Update(secretKey.KeyInfo);
                        if(secretKey.KeyInfo.ChainId == _submitAccount.ServiceAccount.ChainId)
                            return true;
                    }
                    else
                    {
                        _keyView.Update(null);
                    }
                }
#pragma warning disable RECS0022 // A catch clause that catches System.Exception and has an empty body
                catch { }
#pragma warning restore RECS0022 // A catch clause that catches System.Exception and has an empty body

                return false;
            });

            AddIndex = AddSubmitRow("ImportButton", Import);
            AddIndexBefore = true;
            _password = AddPasswordRow("", "Password");

            Status.ReValidate();

            FocusElement = _key.Edit;
        }

        ExportedSecretKey GetSecretKey()
        {
            var split = _key.Edit.Text?.Split('|');
            if (split == null)
                return null;

            try
            {
                return new ExportedSecretKey(split[split.Length - 1]);
            }
            catch (Exception ex)
            {
                Log.IgnoreException(ex);
            }

            return null;
        }
    }
}
