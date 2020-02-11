using System;
using System.IO;
using System.Threading.Tasks;
using Heleus.Operations;
using Heleus.ProfileService;
using Xamarin.Forms;

namespace Heleus.Apps.Shared
{
    public class ProfileNameView : StackLayout
    {
        public ProfileNameView(string profileName, string realName, string bio)
        {
            InputTransparent = true;
            Spacing = 0;
            SizeChanged += ViewSizeChanged;

            var hasBio = !string.IsNullOrEmpty(bio);

            var title = new ExtLabel { Margin = new Thickness(0, 0, 0, hasBio ? 5 : 0), InputTransparent = true };

            var realNameSpan = new Span
            {
                Text = realName
            };
            realNameSpan.SetStyle(Theme.RowTitleFont, Theme.TextColor);

            var profileNameSpan = new Span
            {
                Text = $"\n@{profileName}"
            };
            profileNameSpan.SetStyle(Theme.TextFont, Theme.TextColor);

            title.FormattedText = new FormattedString
            {
                Spans =
                {
                    realNameSpan, profileNameSpan
                }
            };

            Children.Add(title);

            if (hasBio)
            {
                var value = new ExtLabel { Text = bio, FontStyle = Theme.RowFont, ColorStyle = Theme.TextColor, Margin = new Thickness(0, 0, 0, 0), InputTransparent = true };
                Children.Add(value);
            }
        }

        void ViewSizeChanged(object sender, EventArgs e)
        {
            var w = (int)Width;
            if (Width <= 0)
                return;

            foreach (var child in Children)
            {
                if (child is ExtLabel label)
                {
                    if (w != (int)label.WidthRequest)
                        label.WidthRequest = w;
                }
            }
        }
    }

    public static class ProfilePageSections
    {
        public static bool AddProfileSections(StackPage page, ProfileDataResult profileData, string headerText, bool addProfileItems = true)
        {
            var row = page.AddHeaderRow(headerText);
            row.Identifier = "ProfilePage.ProfileSection";

            page.AddIndexBefore = false;
            page.AddIndex = row;
            page.AddFooterRow().Identifier = "ProfilePage.ProfileSectionEnd";

            return UpdateProfileSections(page, profileData, addProfileItems);

        }

        public static Task ProfileItemHandler(ButtonRow button)
        {
            var item = button.Tag as ProfileItemJson;
            try
            {
                if (item.IsWebSite())
                    UIApp.OpenUrl(new Uri(item.v));
                else if (item.IsMail())
                    UIApp.OpenUrl(new Uri("mailto:" + item.v));
            }
            catch { }
            return Task.CompletedTask;
        }

        static ButtonRow AddProfileButton(StackPage page, ProfileItemJson item)
        {
            var button = page.AddButtonRow(null, ProfileItemHandler);
            button.SetMultilineText(item.k, item.v);
            button.Tag = item;

            if (item.IsWebSite())
                button.FontIcon.Icon = Icons.RowLink;
            else if (item.IsMail())
                button.FontIcon.Icon = Icons.At;

            return button;
        }

        public static HeaderRow GetHeaderRow(StackPage page)
        {
            return page.GetRow<HeaderRow>("ProfilePage.ProfileSection");
        }

        public static HeaderRow GetFooterRow(StackPage page)
        {
            return page.GetRow<HeaderRow>("ProfilePage.ProfileSectionEnd");
        }

        public static bool HasProfileSections(StackPage page)
        {
            return page.GetRow<HeaderRow>("ProfilePage.ProfileSection") != null;
        }

        public static bool UpdateProfileSections(StackPage page, ProfileDataResult profileData, bool addProfileItems = true)
        {
            var section = page.GetRow<HeaderRow>("ProfilePage.ProfileSection");
            if (section == null)
                return false;

            var nameRow = page.GetRow<SeparatorRow>("ProfilePage.ProfileName");

            var oldEvent = section.Tag as ProfileDataResult;
            section.Tag = profileData;

            if(oldEvent?.ProfileInfo != null && profileData?.ProfileInfo != null)
            {
                if (oldEvent.ProfileInfo.AccountId == profileData.ProfileInfo.AccountId &&
                    oldEvent.ProfileInfo.ProfileTransactionId == profileData.ProfileInfo.ProfileTransactionId &&
                    oldEvent.ProfileInfo.ImageTransactionId == profileData.ProfileInfo.ImageTransactionId)
                    return false;
            }

            page.RemoveView(nameRow);
            page.ClearHeaderSection(section, (row) => row.Tag is ProfileItemJson);

            page.AddIndex = section;
            page.AddIndexBefore = false;

            var profileItems = profileData?.ProfileJsonItems;
            if (profileItems == null)
            {
                var title = new ExtLabel { Text = Tr.Get("ProfilePage.NoProfile"), FontStyle = Theme.RowTitleFont, ColorStyle = Theme.TextColor, InputTransparent = true };
                page.AddView(title);
            }
            else
            {
                nameRow = new SeparatorRow();
                SetupBigProfileRow(nameRow, true);
                nameRow.Identifier = "ProfilePage.ProfileName";

                page.AddView(nameRow);

                UpdateProfileRow(nameRow, profileData.ProfileInfo.AccountId, profileData, null);
                page.AddIndex = nameRow;

                if (addProfileItems)
                {
                    page.AddIndexBefore = false;

                    foreach (var item in profileItems)
                    {
                        if (item.p == ProfileItemJson.ProfileNameItem)
                            continue;
                        if (item.p == ProfileItemJson.RealNameItem)
                            continue;
                        if (item.p == ProfileItemJson.BioItem)
                            continue;

                        page.AddIndex = AddProfileButton(page, item);
                    }
                }
            }

            page.AddIndex = null;
            return true;
        }

        public static void RemoveProfileSections(StackPage page)
        {
            page.RemoveHeaderSection("ProfilePage.ProfileSection");
        }

        class BigProfile
        {
            public ExtLabel Label;
            public PointerFrame Container;
            public ExtImage Image;

            internal void Layouted(object sender, EventArgs e)
            {
                var width = (int)(sender as View).Width;
                if(width > 0)
                {
                    if (width != (int)Label.WidthRequest)
                        Label.WidthRequest = width;


                    if (Image != null)
                    {
                        if ((int)Image.HeightRequest != width)
                            Image.HeightRequest = width;
                    }
                }
            }
        }

        public static void UpdateProfileRow(StackRow row, long accountId, ProfileDataResult profileData, ProfileInfo profileInfo)
        {
            var profile = profileInfo ?? profileData?.ProfileInfo;

            var realName = profile?.RealName ?? $"Account {accountId}";
            var profileName = "";
            if (profile != null)
                profileName = $" @{profile.ProfileName}";

            var useImageDummy = true;
            if (profile != null)
                useImageDummy = profile.ImageTransactionId <= Operation.InvalidTransactionId;
            var image = profileData?.Image;
            ExtImage imageView = null;

            var bigProfile = row.Tag as BigProfile;
            if(bigProfile != null)
            {
                var bio = ProfileItemJson.GetItemValue(profileData?.ProfileJsonItems, ProfileItemJson.BioItem);
                if (bio != null)
                    bio = $"\n{bio}";

                var nameSpan = bigProfile.Label.FormattedText.Spans[0];
                var profileNameSpan = bigProfile.Label.FormattedText.Spans[1];
                var bioSpan = bigProfile.Label.FormattedText.Spans[2];
                if (nameSpan.Text != realName)
                    nameSpan.Text = realName;
                if (profileNameSpan.Text != profileName)
                    profileNameSpan.Text = profileName;
                if (bioSpan.Text != bio)
                    bioSpan.Text = bio;

                imageView = bigProfile.Image;
            }
            else
            {
                if(row is LabelRow labelRow)
                {
                    imageView = labelRow.DetailView as ExtImage;
                    if(imageView != null && labelRow.Label.FormattedText != null && labelRow.Label.FormattedText.Spans.Count == 2)
                    {
                        var nameSpan = labelRow.Label.FormattedText.Spans[0];
                        var profileNameSpan = labelRow.Label.FormattedText.Spans[1];

                        if (nameSpan.Text != realName)
                            nameSpan.Text = realName;
                        if (profileNameSpan.Text != profileName)
                            profileNameSpan.Text = profileName;
                    }
                }
            }

            if(imageView != null)
            {
                if (image != null)
                {
                    if (imageView.SourceUrl != accountId.ToString())
                    {
                        imageView.Source = ImageSource.FromStream(() => new MemoryStream(image));
                        imageView.SourceUrl = accountId.ToString();
                    }
                }
                else
                {
                    if (useImageDummy)
                    {
                        if (imageView.SourceUrl != "dummy")
                        {
                            imageView.Source = AccountDummyImage.ImageSource;
                            imageView.SourceUrl = "dummy";
                        }
                    }
                }
            }
        }

        public static void SetupBigProfileRow(SeparatorRow row, bool hasImage)
        {
            var bigProfile = new BigProfile();
            row.Tag = bigProfile;

            var label = new ExtLabel();
            label.Margin = new Thickness(15);

            label.FormattedText = new FormattedString
            {
                Spans =
                {
                    new Span().SetStyle(Theme.RowTitleFont, Theme.TextColor),
                    new Span().SetStyle(Theme.TextFont, Theme.TextColor),
                    new Span().SetStyle(Theme.RowFont, Theme.TextColor)
                }
            };

            bigProfile.Label = label;

            row.LayoutChanged += bigProfile.Layouted;

            if (hasImage)
            {
                var image = new ExtImage();

                bigProfile.Image = image;

                AbsoluteLayout.SetLayoutFlags(image, AbsoluteLayoutFlags.SizeProportional);
                AbsoluteLayout.SetLayoutBounds(image, new Rectangle(0, 0, 1, 1));

                var container = new PointerFrame();
                bigProfile.Container = container;

                container.ColorStyle = Theme.RowHighlightColor;
                container.Content = label;

                AbsoluteLayout.SetLayoutFlags(container, AbsoluteLayoutFlags.WidthProportional | AbsoluteLayoutFlags.YProportional);
                AbsoluteLayout.SetLayoutBounds(container, new Rectangle(0, 1, 1, AbsoluteLayout.AutoSize));

                row.RowLayout.Children.Add(image);
                row.RowLayout.Children.Add(container);
            }
            else
            {
                row.RowLayout.Children.Add(label);
            }
        }
    }
}
