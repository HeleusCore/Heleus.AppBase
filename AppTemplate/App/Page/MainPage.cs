using System;
using System.Threading.Tasks;

namespace Heleus.Apps.Shared
{
    public class MainPage : StackPage
    {
        public MainPage() : base("MainPage")
        {
            Subscribe<ServiceAccountAuthorizedEvent>(AccountAuth);
            Subscribe<ServiceAccountImportEvent>(AccountImport);

            SetupPage();
        }

        void SetupPage()
        {
            StackLayout.Children.Clear();

            AddTitleRow("Title");

            if (!ServiceNodeManager.Current.HadUnlockedServiceNode)
            {
                AddHeaderRow("Test");

                AddTextRow("Info", Tr.Get("App.FullName"));

                AddFooterRow();

                ServiceNodesPage.AddAuthorizeSection(ServiceNodeManager.Current.NewDefaultServiceNode, this, false);
            }
            else
            {
                AddHeaderRow();

                AddButtonRow("Test", async (button) =>
                {
                    IsBusy = true;

                    IsBusy = false;
                });

                AddFooterRow();
            }
        }

        Task AccountImport(ServiceAccountImportEvent arg)
        {
            SetupPage();

            return Task.CompletedTask;
        }

        Task AccountAuth(ServiceAccountAuthorizedEvent arg)
        {
            SetupPage();

            return Task.CompletedTask;
        }
    }
}
