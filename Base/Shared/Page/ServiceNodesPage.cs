using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Heleus.Chain;
using Heleus.Cryptography;
using Xamarin.Forms;

namespace Heleus.Apps.Shared
{
    [Flags]
    public enum ServiceNodesPageSelectionFlags
    {
        None,
        ActiveRequired,
        UnlockedAccountRequired
    }

    public class ServiceNodesPage : StackPage
    {
        public const string PageTitle = "ServiceNodesPage.Title";
        public const char PageIcon = Icons.Server;

        readonly Action<ServiceNode> _select;
        readonly HeaderRow _anchor;

        async Task AddServiceNode(ButtonRow buttonRow)
        {
            await Navigation.PushAsync(new AddServiceNodePage());
        }

        async static Task RequestAuthorization(ButtonRow button)
        {
            var item = button.Tag as Tuple<ServiceNode, StackPage, int>;
            var serviceEndpoint = item.Item1;
            var page = item.Item2;
            var popCount = item.Item3;

            await page.Navigation.PushAsync(new AuthorizeAccountPage(serviceEndpoint, ServiceNodeManager.MinimumServiceAccountPassphraseLength, popCount));
        }

        async static Task Import(ButtonRow button)
        {
            var item = button.Tag as Tuple<ServiceNode, StackPage, int>;
            var serviceEndpoint = item.Item1;
            var page = item.Item2;
            var popCount = item.Item3;

            await page.Navigation.PushAsync(new ImportAccountPage(KeyStoreTypes.ServiceAccount, ServiceNode.MinPasswordLength, serviceEndpoint.ImportServiceAccount, popCount));
        }

        async static Task Restore(ButtonRow button)
        {
            var item = button.Tag as Tuple<ServiceNode, StackPage, int>;
            var serviceEndpoint = item.Item1;
            var page = item.Item2;
            var popCount = item.Item3;

            await page.Navigation.PushAsync(new RestoreAccountPage(serviceEndpoint, ServiceNodeManager.MinimumServiceAccountPassphraseLength, popCount));
        }

        public static void AddAuthorizeSection(StackPage page, bool addTitle = true, int popCount = 1)
        {
            AddAuthorizeSection(ServiceNodeManager.Current.NewDefaultServiceNode, page, addTitle, popCount);
        }

        public static void AddAuthorizeSection(ServiceNode serviceNode, StackPage page, bool addTitle = true, int popCount = 1)
        {
            if (addTitle)
                page.AddTitleRow("AuthorizePage.Heleus");

            var header = page.AddHeaderRow("Authorize");
            header.Label.Text = Tr.Get("AuthorizePage.Authorize", Tr.Get("App.FullName"));

            var b = page.AddSubmitButtonRow("AuthorizePage.RequestAuthorization", RequestAuthorization);
            b.Tag = Tuple.Create(serviceNode, page, popCount);

            b = page.AddButtonRow("AuthorizePage.ImportAccount", Import);
            b.SetDetailViewIcon(Icons.CloudDownload, 25);
            b.Tag = Tuple.Create(serviceNode, page, popCount);

            b = page.AddButtonRow("AuthorizePage.RestoreAccount", Restore);
            b.SetDetailViewIcon(Icons.Retweet, 25);
            b.Tag = Tuple.Create(serviceNode, page, popCount);

            page.AddInfoRow("AuthorizePage.Info", Tr.Get("App.FullName"));

            page.AddSeparatorRow();

            var img = page.AddImageRow(1.0 / 3.0);
            img.ImageLayout.Margin = new Thickness(0, 0);
            img.ImageView.Source = new ResourceImageSource("heleus.png");

            page.AddLinkRow("Common.LearnMore", Tr.Get("Link.Authorize"));
            page.AddLinkRow("Common.GetHeleusApp", Tr.Get("Link.HeleusAppDownload"));

            page.AddFooterRow();
        }

        public static void RemoveAuthorizeSection(StackPage page)
        {
            if (page.GetRow("AuthorizePage.Heleus") is HeaderRow title)
                page.StackLayout.Children.Remove(title);

            page.RemoveHeaderSection("Authorize");
        }

        async Task ServiceButton(ButtonViewRow<ServiceNodeView> arg)
        {
            if(arg.Tag is ServiceNode serviceEndpoint)
            {
                if(_select != null)
                {
                    _select.Invoke(serviceEndpoint);
                    await Navigation.PopAsync();
                    return;
                }

                var cancel = Tr.Get("Common.Cancel");
                var actions = new List<string>();

                var view = T("View");
                var activate = T("Activate");
                var deactivate = T("Deactivate");

                var activeCount = 0;
                foreach (var serviceNode in ServiceNodeManager.Current.ServiceNodes)
                {
                    if (serviceNode.Active)
                        activeCount++;
                }

                if (serviceEndpoint.Active)
                {
                    if(activeCount <= 1)
                    {
                        await Navigation.PushAsync(new ServiceNodePage(serviceEndpoint));
                        return;
                    }

                    actions.Add(view);
                    actions.Add(deactivate);
                }
                else
                {
                    actions.Add(activate);
                }

                var result = await DisplayActionSheet(Tr.Get("Common.Action"), cancel, null, actions.ToArray());

                if (result == view)
                {
                    await Navigation.PushAsync(new ServiceNodePage(serviceEndpoint));
                }
                else if (result == activate)
                {
                    serviceEndpoint.SetActive(true);
                }
                else if (result == deactivate)
                {
                    if(activeCount <= 1)
                    {
                        await ErrorAsync("OneNodeRequired");
                        return;
                    }

                    serviceEndpoint.SetActive(false);
                }
            }
        }

        void AddServiceButton(ServiceNode serviceEndpoint)
        {
            var activeOnly = _select != null;

            if (activeOnly && !serviceEndpoint.Active)
                return;

            var button = AddButtonViewRow(new ServiceNodeView(serviceEndpoint, !activeOnly), ServiceButton);
            button.Tag = serviceEndpoint;

            button.RowLayout.SetAccentColor(serviceEndpoint.AccentColor);
        }

        public ServiceNodesPage(Action<ServiceNode> select = null, ServiceNodesPageSelectionFlags flags = ServiceNodesPageSelectionFlags.None) : base("ServiceNodesPage")
        {
            _select = select;

            Subscribe<ServiceNodeAddedEvent>(EndpointAdded);
            Subscribe<ServiceNodeDefaultChangedEvent>(DefaultEndpointChanged);
            Subscribe<ServiceNodeChangedEvent>(EndpointChanged);

            AddTitleRow("Title");

            AddHeaderRow("AvailableServiceNodes");

            foreach (var endpoint in ServiceNodeManager.Current.ServiceNodes)
            {
                if ((flags & ServiceNodesPageSelectionFlags.ActiveRequired) != 0 && !endpoint.Active)
                    continue;

                if ((flags & ServiceNodesPageSelectionFlags.UnlockedAccountRequired) != 0 && !endpoint.HasUnlockedServiceAccount)
                    continue;

                AddServiceButton(endpoint);
            }

            _anchor = AddFooterRow();

            if (_select == null)
            {
                AddHeaderRow("AddServiceNode");

                AddTextRow("ServiceNodeInfo").Label.ColorStyle = Theme.InfoColor;
                AddLinkRow("Common.LearnMore", Tr.Get("Link.ServiceNode"));
                AddSubmitButtonRow("AddNewServiceNode", AddServiceNode);
                AddFooterRow();
            }
        }

        void Update()
        {
            var rows = GetHeaderSectionRows("AvailableServiceNodes");
            foreach (var row in rows)
            {
                if (row is ButtonViewRow<ServiceNodeView> button)
                {
                    button.RowLayout.SetAccentColor(button.View.ServiceNode.AccentColor);
                    button.View.Update();
                }
            }
        }

        Task EndpointChanged(ServiceNodeChangedEvent arg)
        {
            Update();
            return Task.CompletedTask;
        }

        Task DefaultEndpointChanged(ServiceNodeDefaultChangedEvent arg)
        {
            Update();
            return Task.CompletedTask;
        }

        Task EndpointAdded(ServiceNodeAddedEvent arg)
        {
            AddIndex = _anchor;
            AddIndexBefore = true;

            AddServiceButton(arg.ServiceNode);
            Update();

            return Task.CompletedTask;
        }
    }
}
