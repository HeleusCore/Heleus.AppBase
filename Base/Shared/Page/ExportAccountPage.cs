using System;
using System.Threading.Tasks;
using Heleus.Chain;

namespace Heleus.Apps.Shared
{
	public class ExportAccountPage : StackPage
	{
		readonly KeyStore _clientAccount;
		readonly EntryRow _password;
		readonly EditorRow _key;

        readonly bool _newPassword;

		async Task Export(ButtonRow button)
		{
            IsBusy = true;

			try
			{
                var pw = _password.Edit.Text;
                string exp = string.Empty;

                if(_newPassword)
                {
                    KeyStore newStore = null;

                    await Task.Run(() =>
                    {
                        if (_clientAccount is CoreAccountKeyStore coreAccountKeyStore)
                            newStore = new CoreAccountKeyStore(coreAccountKeyStore.Name, coreAccountKeyStore.AccountId, coreAccountKeyStore.DecryptedKey, pw);
                        else if (_clientAccount is ServiceAccountKeyStore serviceAccountKeyStore)
                            newStore = new ServiceAccountKeyStore(serviceAccountKeyStore.Name, serviceAccountKeyStore.SignedPublicKey, serviceAccountKeyStore.DecryptedKey, pw);
                        else if (_clientAccount is ChainKeyStore chainAccountKeyStore)
                            newStore = new ChainKeyStore(chainAccountKeyStore.Name, chainAccountKeyStore.PublicChainKey, chainAccountKeyStore.DecryptedKey, pw);
                    });

                    exp = $"{newStore.Name}|{newStore.HexString}";

                    _key.Edit.Text = exp;
                    UIApp.CopyToClipboard(exp);
                }
                else
                {
                    await _clientAccount.DecryptKeyAsync(pw, true);

                    exp = $"{_clientAccount.Name}|{_clientAccount.HexString}";

                    _key.Edit.Text = exp;
                    UIApp.CopyToClipboard(exp);
                }

                if(await ConfirmAsync("Success"))
                {
                    await UIApp.SaveFilePicker2(System.Text.Encoding.UTF8.GetBytes(exp), $"{_clientAccount.Name}.heleus");
                }
			}
			catch
			{
				await ErrorAsync("Failed");
			}

            IsBusy = false;
		}

		public ExportAccountPage(KeyStore clientAccount, int passwordLength, string password = null) : base("ExportAccountPage")
		{
            EnableStatus();
			_clientAccount = clientAccount;

            if (password != null)
                _newPassword = clientAccount.DecryptKeyAsync(password, false).Result;

			AddTitleRow("Title");

			AddHeaderRow();
			AddViewRow(new ClientAccountView(clientAccount));
            _key = AddEditorRow(null, null);
            _key.SetDetailViewIcon(Icons.Coins);
            AddFooterRow();

            EnableStatus();

            AddIndex = AddSubmitRow("ExportButton", Export);
            AddIndexBefore = true;
            _password = AddPasswordRow("", "Password");
            Status.Add(_password.Edit, Tr.Get(_newPassword ? "Common.NewPasswordStatus" :  "Common.PasswordStatus", passwordLength), (sv, edit, newText, oldText) =>
            {
                return StatusValidators.PasswordValidator(passwordLength, sv, edit, newText, oldText);
            });

            Status.ReValidate();
        }
	}
}
