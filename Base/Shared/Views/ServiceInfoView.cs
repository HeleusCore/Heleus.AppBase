using Heleus.Chain.Core;
using Heleus.Service;

namespace Heleus.Apps.Shared
{
    public class ServiceInfoView : RowView
    {
        readonly ExtLabel _available;
        readonly ExtLabel _webSite;
        readonly ExtLabel _owner;
        readonly ExtLabel _endpoint;
        readonly ExtLabel _nameInfo;
        readonly ExtLabel _name;
        readonly ExtLabel _version;
        readonly ExtLabel _versionInfo;

        public ServiceInfoView(long requiredVersion, string requiredName, ChainInfo chainInfo, ServiceInfo serviceInfo, string endpoint) : base("ServiceInfoView")
        {
            (_, _available) = AddRow("Available", null);
            (_, _endpoint) = AddRow("Endpoint", null);

            (_nameInfo, _name) = AddRow("Name", null);
            _nameInfo.Text = Tr.Get("ServiceInfoView.Name", requiredName);

            (_versionInfo, _version) = AddRow("Version", null);
            _versionInfo.Text = Tr.Get("ServiceInfoView.Version", requiredVersion);

            (_, _webSite) = AddRow("Website", null);
            (_, _owner) = AddLastRow("Owner", null);

            Update(chainInfo, serviceInfo, endpoint);
        }

        public void Update(ChainInfo chainInfo, ServiceInfo serviceInfo, string endpoint)
        {
            if (chainInfo != null && serviceInfo != null)
            {
                _available.Text = Tr.Get("Common.DialogYes");
                _webSite.Text = chainInfo.Website ?? "-";
                _owner.Text = chainInfo.AccountId.ToString();
                _name.Text = serviceInfo.Name;
                _version.Text = serviceInfo.Version.ToString();

                if (string.IsNullOrWhiteSpace(endpoint))
                    _endpoint.Text = "-";
                else
                    _endpoint.Text = endpoint;

            }
            else
            {
                IsValidServiceEndpoint = true;

                _available.Text = Tr.Get("Common.DialogNo");
                _webSite.Text = "-";
                _owner.Text = "-";
                _name.Text = "-";
                _version.Text = "-";
                _endpoint.Text = "-";
            }
        }

        public bool IsValidServiceEndpoint
        {
            set
            {
                if (value)
                {
                    _name.ColorStyle = _nameInfo.ColorStyle = _version.ColorStyle = _versionInfo.ColorStyle = Theme.TextColor;
                }
                else
                {
                    _name.ColorStyle = _nameInfo.ColorStyle = _version.ColorStyle = _versionInfo.ColorStyle = Theme.ErrorColor;
                }
            }
        }
    }
}
