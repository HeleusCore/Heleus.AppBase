using System;
using System.IO;
using System.Threading.Tasks;
using Heleus.Operations;
using Heleus.ProfileService;
using Xamarin.Forms;

namespace Heleus.Apps.Shared
{
    public class ProfileRow : LabelRow
    {
        public static void SetupProfileRow(LabelRow row, int imageMargin = 0)
        {
            var imgView = new ExtImage();

            row.SetDetailView(imgView, 0);
            imgView.Margin = new Thickness(imageMargin, 0, 0, 0);

            row.Label.FormattedText = new FormattedString
            {
                Spans =
                {
                    new Span().SetStyle(Theme.TextFont, Theme.TextColor),
                    new Span().SetStyle(Theme.MicroFont, Theme.TextColor)
                }
            };

            row.Label.Margin = new Thickness(imageMargin, 2, row.Label.Margin.Right, 2);

            AbsoluteLayout.SetLayoutFlags(imgView, AbsoluteLayoutFlags.YProportional);
            AbsoluteLayout.SetLayoutBounds(imgView, new Rectangle(0, 0.5, AbsoluteLayout.AutoSize, AbsoluteLayout.AutoSize));

            row.SizeChanged += (sender, e) =>
            {
                var height = (int)row.Height;
                var request = (int)imgView.HeightRequest;
                if (height > 0 && height != request)
                {
                    imgView.HeightRequest = height;
                    imgView.WidthRequest = height;

                    row.Label.Margin = new Thickness(height + 5 + imageMargin, 2, row.Label.Margin.Right, 2);
                }
            };
        }

        public static void UpdateProfileRow(LabelRow row, long accountId, ProfileDataResult profileData, ProfileInfo profileInfo)
        {
            var profile = profileInfo ?? profileData?.ProfileInfo;

            var realName = profile?.RealName ?? $"Account {accountId}";
            var profileName = "\n-";
            if (profile?.ProfileName != null)
                profileName = $"\n@{profile.ProfileName}";

            if (accountId <= 0)
            {
                realName = "-";
                profileName = "";
            }

            if (row.Label.FormattedText != null && row.Label.FormattedText.Spans.Count == 2)
            {
                var nameSpan = row.Label.FormattedText.Spans[0];
                var profileNameSpan = row.Label.FormattedText.Spans[1];

                if (nameSpan.Text != realName)
                    nameSpan.Text = realName;
                if (profileNameSpan.Text != profileName)
                    profileNameSpan.Text = profileName;
            }

            //if (profileData == null)
            //    return;

            var useImageDummy = true;
            if (profile != null)
                useImageDummy = profile.ImageTransactionId <= Operation.InvalidTransactionId;
            var image = profileData?.Image;

            if (row.DetailView is ExtImage imageView)
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

        public readonly long AccountId;

        public void Refresh()
        {
            UIApp.Run(() => DownloadProfileData());
        }

        async Task DownloadProfileData()
        {
            var profileDataReult = await ProfileManager.Current.GetProfileData(AccountId, ProfileDownloadType.DownloadIfNotAvailable, true);
            UpdateProfileRow(this, AccountId, profileDataReult, null);
        }

        public ProfileRow(long accountId)
        {
            SetupProfileRow(this);
            UpdateProfileRow(this, accountId, null, null);

            AccountId = accountId;
            Refresh();
        }
    }

    public class ProfileButtonRow : ButtonRow
    {
        long _accountId;
        public long AccountId
        {
            get => _accountId;

            set
            {
                if (_accountId != value)
                {
                    _accountId = value;
                    ProfileRow.UpdateProfileRow(this, _accountId, null, null);
                    Refresh();
                }
            }
        }

        public void Refresh()
        {
            UIApp.Run(DownloadProfileData);
        }

        public void Update(long accountId, ProfileDataResult profileData, ProfileInfo profileInfo)
        {
            _accountId = accountId;
            ProfileRow.UpdateProfileRow(this, _accountId, profileData, profileInfo);
            if (profileData == null)
                Refresh();
        }

        async Task DownloadProfileData()
        {
            var profileDataReult = await ProfileManager.Current.GetProfileData(AccountId, ProfileDownloadType.DownloadIfNotAvailable, true);
            ProfileRow.UpdateProfileRow(this, AccountId, profileDataReult, null);
        }

        public ProfileButtonRow(long accountId, ProfileInfo profileInfo, ProfileDataResult profileData, Func<ProfileButtonRow, Task> action, int imageMargin = 0) : base(Icons.RowMore, null)
        {
            ButtonAction = async (button) =>
            {
                if (action != null)
                    await action.Invoke(this);
            };

            ProfileRow.SetupProfileRow(this);

            _accountId = accountId;

            ProfileRow.UpdateProfileRow(this, _accountId, profileData, profileInfo);

            if(profileData == null)
                Refresh();
        }

        public ProfileButtonRow(ProfileInfo profileInfo, Func<ProfileButtonRow, Task> action, int imageMargin = 0) : this(profileInfo.AccountId, profileInfo, null, action, imageMargin)
        {
        }

        public ProfileButtonRow(long accountId, Func<ProfileButtonRow, Task> action, int imageMargin = 0) : this(accountId, null, null, action, imageMargin)
        {
        }
    }
}
