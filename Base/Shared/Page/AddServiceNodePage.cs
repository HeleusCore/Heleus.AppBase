using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Heleus.Chain.Core;
using Heleus.Network.Client;

namespace Heleus.Apps.Shared
{
    public class AddServiceNodePage : StackPage
    {
        readonly EntryRow _endpoint;
        readonly EntryRow _chainId;
        readonly ServiceInfoView _serviceInfoView;

        ServiceNode _serviceNode;
        ChainInfo _chainInfo;

        async Task Query(ButtonRow arg)
        {
            IsBusy = true;

            var chainId = int.Parse(_chainId.Edit.Text);
            var endPoint = _endpoint.Edit.Text;

            var customEndpoint = true;

            _chainInfo = null;
            if (string.IsNullOrEmpty(endPoint))
            {
                var chainDownload = await ServiceNodeManager.Current.DefaultClient.DownloadChainInfo(chainId);
                if (chainDownload.ResultType != DownloadResultTypes.Ok)
                {
                    await ErrorAsync("ChainInfoDownloadFailed");
                    IsBusy = false;
                    return;
                }

                _chainInfo = chainDownload.Data;
                if (_chainInfo == null)
                {
                    await ErrorAsync("ChainNotFound");
                    IsBusy = false;
                    return;
                }

                endPoint = _chainInfo.GetPublicEndpoints().FirstOrDefault();
                if (string.IsNullOrEmpty(endPoint))
                {
                    await ErrorAsync("NoPublicEndpoint");
                    IsBusy = false;
                    return;
                }

                customEndpoint = false;
            }

            _serviceNode = ServiceNodeManager.Current.NewServiceNode(chainId, new Uri(endPoint), customEndpoint);

            if (_chainInfo == null)
                _chainInfo = await _serviceNode.GetChainInfo();

            var serviceInfo = await _serviceNode.GetServiceInfo();

            _serviceInfoView.Update(_chainInfo, serviceInfo, endPoint);

            IsBusy = false;

            if (_chainInfo != null && serviceInfo != null)
            {
                ServiceNodesPage.RemoveAuthorizeSection(this);

                if (ServiceNodeManager.Current.IsServiceInfoValid(serviceInfo))
                {
                    _serviceInfoView.IsValidServiceEndpoint = true;
                    ServiceNodesPage.AddAuthorizeSection(_serviceNode, this, false, 2);
                    _serviceNode.SetName(_chainInfo.Name);

                    await MessageAsync("Success");
                }
                else
                {
                    _serviceInfoView.IsValidServiceEndpoint = false;
                    await ErrorAsync("InvalidServiceInfo");
                }
            }
            else
            {
                ServiceNodesPage.RemoveAuthorizeSection(this);
                await ErrorAsync("InvalidServiceNode");
            }
        }

        async Task ServiceInfo(ButtonViewRow<ServiceInfoView> arg)
        {
            if (_chainInfo != null)
            {
                var cancel = Tr.Get("Common.Cancel");
                var website = Tr.Get("Common.ViewWebsite");
                var profile = Tr.Get("ServiceNodePage.Profile");

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

        public AddServiceNodePage(int chainId = 0, string endpoint = null) : base("AddServiceNodePage")
        {
            AddTitleRow("Title");

            AddHeaderRow("ServiceNode");

            _chainId = AddEntryRow(null, "ChainId");
            Status.Add(_chainId.Edit, T("ChainIdStatus"), StatusValidators.PositiveNumberValidator);

            _endpoint = AddEntryRow(null, "Endpoint");
            Status.Add(_endpoint.Edit, T("EndpointStatus"), StatusValidators.ValidUriOrEmpty);

#if DEBUG
            _chainId.Edit.Text = ServiceNodeManager.Current.DefaultChainId.ToString();
            _endpoint.Edit.Text = ServiceNodeManager.Current.DefaultEndPoint.AbsoluteUri;
#endif

            if (chainId > 0)
                _chainId.Edit.Text = chainId.ToString();
            if (!string.IsNullOrEmpty(endpoint))
                _endpoint.Edit.Text = endpoint;

            AddSubmitRow("Query", Query);
            AddFooterRow();

            AddHeaderRow("ServiceInfo");

            _serviceInfoView = new ServiceInfoView(ServiceNodeManager.Current.MinimumServiceVersion, ServiceNodeManager.Current.RequiredServiceName, null, null, null);
            AddButtonViewRow(_serviceInfoView, ServiceInfo);

            AddFooterRow();
        }
    }
}
