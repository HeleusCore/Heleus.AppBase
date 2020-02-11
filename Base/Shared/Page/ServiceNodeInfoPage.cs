using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Heleus.Chain.Core;

namespace Heleus.Apps.Shared
{
    public class ServiceNodeInfoPage : StackPage
    {
        readonly ServiceNode _serviceNode;
        readonly ServiceInfoView _serviceInfoView;

        ChainInfo _chainInfo;

        async Task ServiceInfo(ButtonViewRow<ServiceInfoView> arg)
        {
            if (_chainInfo != null)
            {
                var cancel = Tr.Get("Common.Cancel");
                var website = Tr.Get("Common.ViewWebsite");
                var profile = T("Profile");

                var actions = new List<string>();
                if (!string.IsNullOrEmpty(_chainInfo.Website))
                    actions.Add(website);

                actions.Add(profile);

                if (actions.Count > 0)
                {
                    var result = await DisplayActionSheet(Tr.Get("Common.Action"), cancel, null, actions.ToArray());
                    if (result == website)
                    {
                        UIApp.OpenUrl(new Uri(_chainInfo.Website));
                    }
                    else if (result == profile)
                    {
                        await Navigation.PushAsync(new ViewProfilePage(_chainInfo.AccountId));
                    }
                }
            }
        }

        public ServiceNodeInfoPage(ServiceNode serviceNode) : base("ServiceNodePage")
        {
            _serviceNode = serviceNode;

            AddTitleRow("Title");

            AddHeaderRow("ServiceNodeInfo");

            _serviceInfoView = new ServiceInfoView(ServiceNodeManager.Current.MinimumServiceVersion, ServiceNodeManager.Current.RequiredServiceName, null, null, null);

            AddButtonViewRow(_serviceInfoView, ServiceInfo);

            AddFooterRow();
            IsBusy = true;
        }

        public override async Task InitAsync()
        {
            _chainInfo = await _serviceNode.GetChainInfo();
            var serviceInfo = await _serviceNode.GetServiceInfo();

            _serviceInfoView.Update(_chainInfo, serviceInfo, _serviceNode.Endpoint.AbsoluteUri);

            IsBusy = false;
        }
    }
}
