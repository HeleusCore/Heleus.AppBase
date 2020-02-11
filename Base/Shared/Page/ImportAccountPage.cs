using System;
using System.Threading.Tasks;
using Heleus.Chain;
using Heleus.Cryptography;

namespace Heleus.Apps.Shared
{
    public enum ImportAccountResult
    {
        Ok,
        CoreAccountAlreadyPresent,
        AccountAlreadyPresent,
        PasswordInvalid,
        ValidationFailed,
        InvalidKeyType,
        InvalidChainId,
        InvalidAccountId
    }

    public class ImportAccountPage : StackPage
	{
		readonly EditorRow _key;
		readonly EntryRow _password;
		readonly KeyStoreTypes _allowedKeyType;
        readonly Func<KeyStore, string, Task<ImportAccountResult>> _importFunc;
        readonly int _popCount;

        async Task Import(ButtonRow button)
		{
            IsBusy = true;
			var clientAccount = GetClientAccount();
			var result = await _importFunc.Invoke(clientAccount, _password.Edit.Text);
            IsBusy = false;

            if (result == ImportAccountResult.Ok)
            {
                await MessageAsync("Success");
                await PopAsync(_popCount);
            }
            else
            {
                await ErrorTextAsync(string.Format(T("ImportFailed"), Tr.Get("ImportAccountResult." + result)));
            }
		}

        async Task Load(ButtonRow buttonRow)
        {
            var file = await UIApp.OpenFilePicker2();
            if(file.Valid)
            {
                var data = new byte[file.Stream.Length];
                file.Stream.Read(data, 0, data.Length);
                var text = System.Text.Encoding.UTF8.GetString(data);
                _key.Edit.Text = text;
            }
        }

        public ImportAccountPage(KeyStoreTypes allowedKeyType, int passwordLength, Func<KeyStore, string, Task<ImportAccountResult>> importFunc, int popCount) : base("ImportAccountPage")
		{
			_allowedKeyType = allowedKeyType;
            _importFunc = importFunc;
            _popCount = popCount;

			AddTitleRow("Title");

            AddHeaderRow("PasteInfo");
            _key = AddEditorRow(null, null);
            _key.SetDetailViewIcon(Icons.Coins);

            var load = AddButtonRow("Load", Load);
            load.SetDetailViewIcon(Icons.FileImport);

            var accountView = new ClientAccountView();
            AddViewRow(accountView);
            AddFooterRow();

            Status.Add(_key.Edit, T("KeyStatus", Tr.Get("KeyStoreTypes." + allowedKeyType)), (sv, edit, newText, oldText) =>
            {
                var account = GetClientAccount();
                accountView.Update(account);

                return IsAllowedKeyType(account);
            });

            AddIndex = AddSubmitRow("ImportButton", Import);
            AddIndexBefore = true;
            _password = AddPasswordRow("", "Password");
            Status.Add(_password.Edit, Tr.Get("Common.PasswordStatus", passwordLength), (sv, edit, newText, oldText) =>
            {
                return StatusValidators.PasswordValidator(passwordLength, sv, edit, newText, oldText);
            });

            Status.ReValidate();

            FocusElement = _key.Edit;
        }

        bool IsAllowedKeyType(KeyStore account)
        {
            if (account == null)
                return false;

            return (_allowedKeyType == account.KeyStoreType);
        }

        KeyStore GetClientAccount()
		{
			var text = _key.Edit.Text;
			try
			{
                var split = text?.Split('|');
                if (split == null)
                    return null;

				return KeyStore.Restore(split[split.Length - 1].Trim());
			}
			catch { }
			return null;
		}
	}
}
