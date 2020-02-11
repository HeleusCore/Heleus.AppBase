using System;
using System.Threading.Tasks;
using Heleus.Chain;
using Heleus.Cryptography;

namespace Heleus.Apps.Shared
{
    public class ServiceNodePage : StackPage
    {
        readonly ButtonRow _import;
        readonly ServiceNode _serviceNode;
        readonly ThemedColorStyle _color;

        async Task ServiceNodeInfo(ButtonRow button)
        {
            await Navigation.PushAsync(new ServiceNodeInfoPage(_serviceNode));
        }

        public ServiceNodePage(ServiceNode serviceNode) : base("ServiceNodePage")
        {
            Subscribe<ServiceAccountImportEvent>(AccountImport);
            Subscribe<ServiceAccountAuthorizedEvent>(AccountAuth);
            Subscribe<ServiceNodeChangedEvent>(EndpointChanged);
            Subscribe<AccountRenamedEvent>(AccountRenamed);

            _serviceNode = serviceNode;
            _color = new ThemedColorStyle(ThemeColorStyle.None, serviceNode.AccentColor);

            AddTitleRow("Title");

            AddHeaderRow("Endpoint");

            var name = AddEntryRow(serviceNode.Name, "Name");
            name.SetDetailViewIcon(Icons.Pencil);
            name.Edit.TextChanged += Edit_TextChanged;

            var button = AddButtonRow("AccentColor", ChangeColor);
            button.SetDetailView(new ExtBoxView { BackgroundColor = serviceNode.AccentColor });

            button = AddButtonRow("ServiceNodeInfo", ServiceNodeInfo);
            button.SetDetailViewIcon(Icons.Info);
            AddFooterRow();

            if(!_serviceNode.HasUnlockedServiceAccount)
            {
                ServiceNodesPage.AddAuthorizeSection(_serviceNode, this, false);
            }

            AddHeaderRow("ServiceAccounts");

            foreach (var account in serviceNode.SortedServiceAccounts)
            {
                AddButton(account);
            }

            _import = AddButtonRow("ImportButton", Import);
            _import.SetDetailViewIcon(Icons.CloudDownload, 25);

            AddButtonRow("RestoreButton", Restore).SetDetailViewIcon(Icons.Retweet);

            AddFooterRow();
        }

        async Task Restore(ButtonRow arg)
        {
            await Navigation.PushAsync(new RestoreAccountPage(_serviceNode, ServiceNodeManager.MinimumServiceAccountPassphraseLength, 1));
        }

        void Edit_TextChanged(object sender, Xamarin.Forms.TextChangedEventArgs e)
        {
            _serviceNode.SetName(e.NewTextValue);
        }

        async Task ChangeColor(ButtonRow button)
        {
            await Navigation.PushAsync(new ColorPickerPage(_color, (newColor) =>
            {
                (button.DetailView as ExtBoxView).BackgroundColor = newColor;
                _serviceNode.SetAccentColor(newColor);
            }));
        }

        void AddButton(KeyStore account)
        {
            var view = new ServiceAccountView(account);

            var button = AddButtonViewRow(view, Account);
            button.Tag = account;
        }

        void Update()
        {
            var rows = GetHeaderSectionRows("ServiceAccounts");
            foreach (var row in rows)
            {
                if (row is ButtonViewRow<ServiceAccountView> button && row.Tag is ServiceAccountKeyStore account)
                {
                    button.View.Update(account);
                }
            }
        }

        void AccountUpdate(ServiceNode serviceNode, ServiceAccountKeyStore account)
        {
            if (serviceNode == _serviceNode)
            {
                ServiceNodesPage.RemoveAuthorizeSection(this);

                AddIndex = _import;
                AddIndexBefore = true;
                AddButton(account);
                Update();
            }
        }

        Task AccountRenamed(AccountRenamedEvent arg)
        {
            Update();
            return Task.CompletedTask;
        }

        Task AccountAuth(ServiceAccountAuthorizedEvent arg)
        {
            AccountUpdate(arg.ServiceNode, arg.ServiceAccount);
            return Task.CompletedTask;
        }

        Task AccountImport(ServiceAccountImportEvent arg)
        {
            AccountUpdate(arg.ServiceNode, arg.ServiceAccount);
            return Task.CompletedTask;
        }

        Task EndpointChanged(ServiceNodeChangedEvent arg)
        {
            if(arg.ServiceNode == _serviceNode)
                Update();

            return Task.CompletedTask;
        }

        async Task Import(ButtonRow arg)
        {
            await Navigation.PushAsync(new ImportAccountPage(KeyStoreTypes.ServiceAccount, ServiceNode.MinPasswordLength, _serviceNode.ImportServiceAccount, 1));
        }

        async Task Account(ButtonViewRow<ServiceAccountView> arg)
        {
            var account = arg.Tag as ServiceAccountKeyStore;

            var cancel = Tr.Get("Common.Cancel");
            var export = T("Export");
            var edit = T("Edit");
            var submit = T("SubmitAccounts");

            var result = await DisplayActionSheet(Tr.Get("Common.Action"), cancel, null, export, edit, submit);
            if (result == export)
            {
                var password = _serviceNode.GetServiceAccountPassword(account.ChainId, account.AccountId, account.KeyIndex);
                await Navigation.PushAsync(new ExportAccountPage(account, ServiceNode.MinPasswordLength, password));
            }
            else if (result == edit)
            {
                await Navigation.PushAsync(new EditAccountPage(account, _serviceNode.Client));
            }
            else if (result == submit)
            {
                await Navigation.PushAsync(new SubmitAccountsPage(_serviceNode.GetSubmitAccounts<SubmitAccount>(account.KeyIndex)));
            }
        }
    }
}
