using Heleus.Chain.Core;

namespace Heleus.Apps.Shared
{
    public class ServiceNodeView : RowView
    {
        readonly ExtLabel _name;
        readonly ExtLabel _active;
        readonly ExtLabel _endpoint;
        readonly ExtLabel _custom;
        readonly ExtLabel _accountId;

        public readonly ServiceNode ServiceNode;

        public ServiceNodeView(ServiceNode serviceEndpoint, bool showActive = true) : base("ServiceNodeView")
        {
            ServiceNode = serviceEndpoint;

            if(showActive)
                (_, _active) = AddRow("Active", null);

            (_, _name) = AddRow("Name", null);
            (_, _endpoint) = AddRow("Endpoint", null);
            if(serviceEndpoint.CustomEndPoint)
                (_, _custom) = AddRow("Custom", null);

            (_, _accountId) = AddLastRow("AccountId", null);

            Update();
        }

        public void Update()
        {
            if(_active != null)
                _active.Text = ServiceNode.Active ? Tr.Get("Common.DialogYes") : Tr.Get("Common.DialogNo");

            _name.Text = ServiceNode.Name ?? ServiceNode.Endpoint.AbsoluteUri;
            _endpoint.Text = ServiceNode.Endpoint.AbsoluteUri;
            if(_custom != null)
                _custom.Text = ServiceNode.CustomEndPoint ? Tr.Get("Common.DialogYes") : Tr.Get("Common.DialogNo");

            if (ServiceNode.AccountId >= CoreAccount.NetworkAccountId)
                _accountId.Text = ServiceNode.AccountId.ToString();
            else
                _accountId.Text = "-";
        }
    }
}
