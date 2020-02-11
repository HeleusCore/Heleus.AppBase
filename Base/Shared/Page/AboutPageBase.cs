using System;
using Xamarin.Forms;
using System.Threading.Tasks;

namespace Heleus.Apps.Shared
{
	class AboutPageBase : StackPage
	{
		async Task Rate(ButtonRow row)
		{
			await Task.Delay(0);
			UIApp.RateApp();
		}

		ButtonRow AddBrand(string translation, string name, char icon, Color color)
		{
			if (Tr.Get(translation, out string link))
			{
				var row = AddLinkRow(name, link);
				var ico = row.SetDetailViewIcon(icon, IconSet.Brands);
				ico.TextColor = color;
				ico.ColorStyle = null;
				return row;
			}
			return null;
		}

		protected void AddAbout()
        {
			var image = new ExtImage
			{
				Source = new ResourceImageSource("logo.png"),
				HorizontalOptions = LayoutOptions.Center,
				WidthRequest = 300,
				HeightRequest = 100,
				Margin = new Thickness(0, 15, 0, 0)
			};

			var version = new ExtLabel
			{
				Text = "Version: " + Tr.Get("App.Version"),
				HorizontalTextAlignment = TextAlignment.Center,
				HorizontalOptions = LayoutOptions.Center,

				ColorStyle = Theme.TextColor,
				FontStyle = Theme.DetailFont
			};

			AddView(image);
			AddView(version);

            if (Tr.Get("Link.ReportIssue", out string issuelink))
            {
                AddHeaderRow();
                AddLinkRow("Common.ReportIssue", issuelink);

                if(Tr.Get("Link.RequestFeature", out string featureLink))
                {
                    AddLinkRow("Common.RequestFeature", featureLink);
                }

                AddFooterRow();
            }

			AddHeaderRow();

#pragma warning disable RECS0110 // Condition is always 'true' or always 'false'
#pragma warning disable CS0162 // Unreachable code detected
            if (UIApp.CanRate)
            {
                var appName = Tr.Get("App.Name");
                var rate = AddButtonRow("Common.Rate", Rate, appName);
				var icon = rate.SetDetailViewIcon(Icons.Star, IconSet.Solid);
				icon.TextColor = Color.FromRgb(220, 220, 0);
				icon.ColorStyle = null;
			}
#pragma warning restore CS0162 // Unreachable code detected
#pragma warning restore RECS0110 // Condition is always 'true' or always 'false'

            AddBrand("Link.Github", "Github", Icons.Github, Theme.TextColor.Color); //Color.White);
			AddBrand("Link.Medium", "Medium", Icons.Medium, Theme.TextColor.Color); //Color.White);
            AddBrand("Link.Twitter", "Twitter", Icons.Twitter, Theme.TextColor.Color); //Color.FromRgb(29, 202, 255));
            AddBrand("Link.Reddit", "Reddit", Icons.Reddit, Theme.TextColor.Color); //Color.Orange);
            AddBrand("Link.Facebook", "Facebook", Icons.Facebook, Theme.TextColor.Color); //Color.FromRgb(59, 89, 152));
            AddBrand("Link.Youtube", "Youtube", Icons.Youtube, Theme.TextColor.Color); //Color.Red);
            AddBrand("Link.Instagram", "Instagram", Icons.Instagram, Theme.TextColor.Color); //Color.Purple);
            AddFooterRow();

			AddHeaderRow();
			AddLinkRow("Common.Website", Tr.Get("Link.Website"));
			AddLinkRow("Common.Contact", Tr.Get("Link.Contact"));
			AddLinkRow("Common.TermsOfUse", Tr.Get("Link.TermsOfUse"));
			AddLinkRow("Common.Privacy", Tr.Get("Link.Privacy"));

			AddFooterRow("About", Tr.Get("App.FullName"), DateTime.UtcNow.Year, Tr.Get("App.Developer"));
		}

		public AboutPageBase() : base ("AboutPage")
		{

		}
	}
}

