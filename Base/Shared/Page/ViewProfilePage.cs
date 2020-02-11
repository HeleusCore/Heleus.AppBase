using System;
using System.Threading.Tasks;

namespace Heleus.Apps.Shared
{
    public class ViewProfilePage : StackPage
    {
        protected readonly long _accountId;
        readonly bool _showEdit;

        void UpdateEdit()
        {
            if (!_showEdit)
                return;

            if (GetRow("EditProfile") != null)
                return;

            var footer = ProfilePageSections.GetFooterRow(this);
            if(footer != null)
            {
                AddIndex = footer;
                AddIndexBefore = true;
                AddSubmitButtonRow("EditProfile", EditProfile);
            }
        }

        async Task EditProfile(ButtonRow button)
        {
            if (await ConfirmAsync("ConfirmProfileEdit"))
                UIApp.OpenUrl(new Uri("https://heleuscore.com/heleus/request/editprofile"));
        }

        public ViewProfilePage(long accountId, bool showEdit = false) : base("ViewProfilePage")
        {
            Subscribe<ProfileDataResultEvent>(ProfileData);
            _accountId = accountId;
            _showEdit = showEdit;

            AddTitleRow("Title");

            var profileData = ProfileManager.Current.GetCachedProfileData(accountId);
            if (profileData != null)
            {
                ProfilePageSections.AddProfileSections(this, profileData, "Profile");
                UpdateEdit();
            }
            else
                IsBusy = true;

            UIApp.Run(async () =>
            {
                await ProfileManager.Current.GetProfileData(accountId, ProfileDownloadType.QueryStoredData, true);
                await ProfileManager.Current.GetProfileData(accountId, ProfileDownloadType.ForceDownload, true);
            });
        }

        Task ProfileData(ProfileDataResultEvent arg)
        {
            if(arg.AccountId == _accountId)
            {
                IsBusy = false;

                if (ProfilePageSections.HasProfileSections(this))
                    ProfilePageSections.UpdateProfileSections(this, arg.ProfileData);
                else
                    ProfilePageSections.AddProfileSections(this, arg.ProfileData, "Profile");

                UpdateEdit();
            }

            return Task.CompletedTask;
        }
    }
}
