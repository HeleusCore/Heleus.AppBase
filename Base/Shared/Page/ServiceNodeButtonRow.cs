using System;
using System.Threading.Tasks;
using Xamarin.Forms;

namespace Heleus.Apps.Shared
{
    public class ServiceNodeButtonRow : ButtonRow
    {
        readonly ExtContentPage _page;
        readonly string _lastUsedServiceNodeKey;
        readonly ServiceNodesPageSelectionFlags _flags;

        ServiceNode _serviceNode;
        public ServiceNode ServiceNode
        {
            get => _serviceNode;

            set
            {
                _serviceNode = value;
                if (_serviceNode != null && !_serviceNode.Active)
                    _serviceNode = null;

                UpdateButton();
                if(SelectionChanged != null)
                    UIApp.Run(() => SelectionChanged.Invoke(this));
            }
        }

        public Func<ServiceNodeButtonRow, Task> SelectionChanged;

        void UpdateButton()
        {
            RowLayout.SetAccentColor(_serviceNode != null ? _serviceNode.AccentColor : Color.Transparent);

            var name = new Span
            {
                Text = _serviceNode?.GetName() ?? "-"
            };

            name.SetStyle(Theme.TextFont, Theme.TextColor);

            var detail = new Span
            {
                Text = _serviceNode?.Endpoint != null ? $"\n{_serviceNode.Endpoint.AbsoluteUri}" : ""
            };
            detail.SetStyle(Theme.MicroFont, Theme.TextColor);

            Label.Margin = new Thickness(Label.Margin.Left, 2, Label.Margin.Right, 2);

            Label.FormattedText = new FormattedString
            {
                Spans =
                {
                    name, detail
                }
            };
        }

        async Task SelectServiceNode(ButtonRow button)
        {
            if(!ServiceNodeManager.Current.Ready)
            {
                await _page.MessageAsync("ServiceNodeManagerNotReady");
                return;
            }

            await _page.Navigation.PushAsync(new ServiceNodesPage((serviceNode) =>
            {
                if (ServiceNode != serviceNode)
                {
                    ServiceNode = serviceNode;
                    AppBase.Current.SetLastUsedServiceNode(serviceNode, _lastUsedServiceNodeKey);
                }
            }, _flags));
        }

        ServiceNodeButtonRow(ServiceNode serviceNode, ExtContentPage page) : base(Icons.RowMore, null)
        {
            SetDetailViewIcon(Icons.Server);
            Label.Margin = new Thickness(Label.Margin.Left, 2, Label.Margin.Right, 2);

            ServiceNode = serviceNode;

            page.Subscribe<ServiceNodeChangedEvent>(ServiceNodeChanged);
            _page = page;
        }

        public ServiceNodeButtonRow(ServiceNode serviceNode, ExtContentPage page, Func<ServiceNodeButtonRow, Task> action) : this(serviceNode, page)
        {
            ButtonAction = async (button) =>
            {
                if (action != null)
                {
                    await action.Invoke(this);
                }
            };
        }

        public ServiceNodeButtonRow(ExtContentPage page, ServiceNodesPageSelectionFlags flags = ServiceNodesPageSelectionFlags.None, string lastUsedServiceNodeKey = "default") : this(AppBase.Current.GetLastUsedServiceNode(lastUsedServiceNodeKey), page)
        {
            ButtonAction = SelectServiceNode;
            _lastUsedServiceNodeKey = lastUsedServiceNodeKey;
            _flags = flags;
        }

        Task ServiceNodeChanged(ServiceNodeChangedEvent arg)
        {
            UpdateButton();
            return Task.CompletedTask;
        }
    }
}
