using System.Threading.Tasks;
using Heleus.Chain;
using Heleus.Cryptography;

namespace Heleus.Apps.Shared
{
    public class NewSecretKeyPage : StackPage
    {
        SecretKey _secretKey;

        readonly int _minPassphraseLength;
        readonly SubmitAccount _submitAccount;
        readonly SelectionRow<SecretKeyInfoTypes> _selectionRow;
        readonly SecretKeyView _keyView;
        readonly ButtonRow _addButton;

        public NewSecretKeyPage(SubmitAccount submitAccount, int minPassphraseLength) : base("NewSecretKeyPage")
        {
            _submitAccount = submitAccount;
            _minPassphraseLength = minPassphraseLength;

            AddTitleRow("Title");

            AddHeaderRow("SecretKeyType");

            _selectionRow = AddSelectionRows(new SelectionItemList<SecretKeyInfoTypes>
            {
                new SelectionItem<SecretKeyInfoTypes>(SecretKeyInfoTypes.Random, Tr.Get("SecretKeyInfoTypes.Random")),
                new SelectionItem<SecretKeyInfoTypes>(SecretKeyInfoTypes.Passphrase, Tr.Get("SecretKeyInfoTypes.Passphrase"))
            }, SecretKeyInfoTypes.Random);
            _selectionRow.SelectionChanged = SecretTypeChanged;

            AddFooterRow();

            AddHeaderRow("Options");
            AddFooterRow();

            AddHeaderRow();

            _keyView = new SecretKeyView(null, false);
            AddViewRow(_keyView);

            _addButton = AddSubmitButtonRow("Add", Add);
            _addButton.IsEnabled = false;

            AddFooterRow();
            Update();
        }

        void Update()
        {
            var options = GetRow<HeaderRow>("Options");
            ClearHeaderSection(options);
            var type = _selectionRow.Selection;

            _addButton.IsEnabled = false;
            _secretKey = null;
            _keyView.Update(null);

            if (type == SecretKeyInfoTypes.Random)
            {
                AddIndex = options;
                AddButtonRow("NewRandom", NewRandom);

                _ = NewRandom(null);
            }
            else if (type == SecretKeyInfoTypes.Passphrase)
            {
                AddIndex = options;
                AddInfoRow("PassphraseInfo", _minPassphraseLength);
                var button = AddButtonRow("NewPassphrase", NewPassphrase);
                button.IsEnabled = false;
                var entry = AddEntryRow(null, "Passphrase", _minPassphraseLength);
                entry.Edit.TextChanged += (a, evt) =>
                {
                    button.IsEnabled = evt.NewTextValue.Length >= _minPassphraseLength;
                };
            }
        }

        async Task NewPassphrase(ButtonRow arg)
        {
            var newButton = GetRow<ButtonRow>("NewPassphrase");
            var newEdit = GetRow<EntryRow>("Passphrase");

            newEdit.IsEnabled = false;
            newButton.IsEnabled = false;
            _addButton.IsEnabled = false;
            IsBusy = true;

            _secretKey = await PassphraseSecretKeyInfo.NewPassphraseSecretKey(_submitAccount.ServiceNode.ChainId, newEdit.Edit.Text);
            _keyView.Update(_secretKey.KeyInfo);

            IsBusy = false;
            newEdit.IsEnabled = true;
            _addButton.IsEnabled = true;
            newButton.IsEnabled = true;
        }

        async Task NewRandom(ButtonRow arg)
        {
            var newButton = GetRow<ButtonRow>("NewRandom");

            newButton.IsEnabled = false;
            _addButton.IsEnabled = false;
            IsBusy = true;

            _secretKey = await RandomSecretKeyInfo.NewRandomSecretKey(_submitAccount.ServiceNode.ChainId);
            _keyView.Update(_secretKey.KeyInfo);

            IsBusy = false;
            _addButton.IsEnabled = true;
            newButton.IsEnabled = true;
        }

        async Task Add(ButtonRow arg)
        {
            if (_secretKey != null)
            {
                if(_submitAccount.SecretKeyManager.GetSecretKey(_submitAccount.Index, _secretKey.KeyInfo.SecretId) != null)
                {
                    await ErrorAsync("AlreadyAvailable");
                    return;
                }

                _submitAccount.SecretKeyManager.AddSecretKey(_submitAccount.Index, _secretKey);
                await UIApp.PubSub.PublishAsync(new NewSecretKeyEvent(_submitAccount, _secretKey));
                await MessageAsync("Success");
                await Navigation.PopAsync();
            }
        }

        Task SecretTypeChanged(SecretKeyInfoTypes item)
        {
            Update();
            return Task.CompletedTask;
        }
    }
}
