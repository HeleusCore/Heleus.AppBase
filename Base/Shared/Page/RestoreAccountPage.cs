using System;
using System.Threading.Tasks;
using Heleus.Base;
using Heleus.Chain;
using Heleus.Cryptography;

namespace Heleus.Apps.Shared
{
    public class RestoreAccountPage : StackPage
    {
        readonly ServiceNode _serviceNode;
        readonly int _popCount;

        readonly EntryRow _accountId;
        readonly EntryRow _passphrase;
        readonly EditorRow _key;

        public RestoreAccountPage(ServiceNode serviceNode, int minPassphraseLength, int popCount) : base("RestoreAccountPage")
        {
            _serviceNode = serviceNode;
            _popCount = popCount;

            AddTitleRow("Title");

            AddHeaderRow("Info");

            _accountId = AddEntryRow(null, "AccountId");

            EnableStatus();

            if (serviceNode.AccountId > 0)
            {
                _accountId.Edit.Text = serviceNode.AccountId.ToString();
                _accountId.IsEnabled = false;
            }
            else
            {
                Status.AddBusyView(_accountId);
                Status.Add(_accountId.Edit, T("AccountStatus"), StatusValidators.PositiveNumberValidator);
            }

            _passphrase = AddEntryRow(null, "Passphrase", minPassphraseLength);
            var button = AddButtonRow("Generate", Generate);
            Status.AddBusyView(button);

            AddFooterRow();

            AddHeaderRow("SignatureKey");

            _key = AddEditorRow(null, "SignatureKey");
            _key.SetDetailViewIcon(Icons.Key);
            Status.Add(_key.Edit, T("KeyStatus"), (sv, edit, newText, oldText) =>
            {
                return StatusValidators.HexValidator(64, sv, edit, newText, oldText);
            });

            AddFooterRow();

            AddSubmitRow("Restore", Restore);
        }

        async Task Generate(ButtonRow arg)
        {
            IsBusy = true;

            var secretKey = await PassphraseSecretKeyInfo.NewPassphraseSecretKey(_serviceNode.ChainId, _passphrase.Edit.Text);
            _key.Edit.Text = Hex.ToString(secretKey.SecretHash);

            Status.ReValidate();
            IsBusy = false;
        }

        async Task Restore(ButtonRow arg)
        {
            IsBusy = true;

            var seed = Hex.FromString(_key.Edit.Text);
            var key = Key.GenerateEd25519(seed);
            var accountId = long.Parse(_accountId.Edit.Text);

            var result = await _serviceNode.RequestRestore(accountId, key);

            IsBusy = false;

            if (result == RestoreResult.Ok)
            {
                await MessageAsync("Success");
                await PopAsync(_popCount);
            }
            else
            {
                if (result == RestoreResult.AlreadyAvailable)
                {
                    (var sn, var ac) = ServiceNodeManager.Current.GetServiceAccount(key);
                    await ErrorTextAsync(Tr.Get("RestoreResult.AlreadyAvailable", sn.GetName(), ac.Name));
                }
                else
                {
                    await ErrorTextAsync(Tr.Get("RestoreResult." + result));
                }
            }
        }
    }
}
