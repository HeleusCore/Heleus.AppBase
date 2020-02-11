using System;
using System.Threading.Tasks;
using Heleus.Chain;
using Heleus.Cryptography;
using Heleus.Network.Client;

namespace Heleus.Apps.Shared
{
    public class EditAccountPage : StackPage
    {
        readonly KeyStore _keyStore;
        readonly HeleusClient _client;

        readonly EntryRow _name;

        async Task Save(ButtonRow arg)
        {
            _keyStore.Name = _name.Edit.Text;

            await _client.StoreAccount(_keyStore);

            await UIApp.PubSub.PublishAsync(new AccountRenamedEvent(_keyStore));
            await MessageAsync("Saved");

            await Navigation.PopAsync();
        }

        public EditAccountPage(KeyStore keyStore, HeleusClient client) : base("EditAccountPage")
        {
            AddTitleRow("Title");

            AddHeaderRow("Name");

            _name = AddEntryRow(keyStore.Name, "NameRow");
            Status.Add(_name.Edit, T("NameStatus"), StatusValidators.NoEmptyString);

            AddFooterRow();

            AddSubmitRow("Save", Save);


            _keyStore = keyStore;
            _client = client;
        }
    }
}
