using System;
using System.Threading.Tasks;
using Heleus.Base;
using Heleus.Chain;
using Heleus.Chain.Maintain;

namespace Heleus.Apps.Shared
{
    public class RevenueView : RowView
    {
        readonly ExtLabel _available;
        readonly ExtLabel _payout;
        readonly ExtLabel _total;

        public RevenueView(AccountRevenueInfo revenueInfo) : base("RevenueView")
        {
            (_, _available) = AddRow("Available", null);
            _available.ColorStyle = Theme.InfoColor;

            (_, _payout) = AddRow("Payout", null);
            (_, _total) = AddLastRow("TotalRevenue", null);

            Update(revenueInfo);
        }

        public void Update(AccountRevenueInfo revenueInfo)
        {
            if(revenueInfo == null)
            {
                _available.Text = "-";
                _payout.Text = "-";
                _total.Text = "-";
            }
            else
            {
                var available = revenueInfo.TotalRevenue - revenueInfo.Payout;
                _available.Text = Currency.ToString(available);
                _payout.Text = Currency.ToString(revenueInfo.Payout);
                _total.Text = Currency.ToString(revenueInfo.TotalRevenue);
            }
        }
    }

    public class RevenuePage : StackPage
    {
        readonly ServiceNodeButtonRow _serviceNode;

        public RevenuePage() : base("RevenuePage")
        {
            AddTitleRow("Title");

            AddHeaderRow("Common.ServiceNode");
            _serviceNode = AddRow(new ServiceNodeButtonRow(this, ServiceNodesPageSelectionFlags.ActiveRequired | ServiceNodesPageSelectionFlags.UnlockedAccountRequired));
            _serviceNode.SelectionChanged = ServiceNodeChanged;
            AddInfoRow("Common.ServiceNodeInfo");
            AddFooterRow();

            UIApp.Run(() => Update());
        }

        Task ServiceNodeChanged(ServiceNodeButtonRow obj)
        {
            UIApp.Run(Update);
            return Task.CompletedTask;
        }

        async Task Update()
        {
            var serviceNode = _serviceNode.ServiceNode;

            if(serviceNode == null)
                return;

            IsBusy = true;

            var revenueInfo = (await serviceNode.Client.DownloadRevenueInfo(serviceNode.ChainId, serviceNode.AccountId)).Data?.Item;

            RemoveHeaderSection("Revenue");
            RemoveHeaderSection("RecentRevenue");

            AddIndex = GetRow("Title");
            AddIndex = AddHeaderRow("Revenue");

            if (revenueInfo == null)
            {
                AddIndex = AddInfoRow("NotAvailable");
                AddFooterRow();
            }
            else
            {
                AddIndex = AddViewRow(new RevenueView(revenueInfo));

                var lastRevenues = revenueInfo.LastRevenues;
                if (lastRevenues.Count > 0)
                {
                    AddIndex = AddHeaderRow("RecentRevenue");

                    foreach (var item in lastRevenues)
                    {
                        var row = AddTextRow(null);
                        row.SetMultilineText(Currency.ToString(item.Value), Time.DateString(Protocol.TickToTimestamp(item.Key)));

                        AddIndex = row;
                    }

                    AddIndex = AddFooterRow();
                }

                if (revenueInfo.TotalRevenue - revenueInfo.Payout > 0)
                {
                    AddIndex = AddSubmitButtonRow("RequestRevenue", Request);
                }
                AddFooterRow();
            }

            IsBusy = false;
        }

        Task Request(ButtonRow arg)
        {
            var serviceNode = _serviceNode.ServiceNode;

            if (serviceNode == null)
                return Task.CompletedTask;

            UIApp.OpenUrl(new Uri($"https://heleuscore.com/heleus/request/revenue/{serviceNode.ChainId}/"));

            return Task.CompletedTask;
        }
    }
}
