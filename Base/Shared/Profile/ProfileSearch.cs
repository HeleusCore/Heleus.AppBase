using System.Collections.Generic;
using System.Threading.Tasks;
using Heleus.ProfileService;

namespace Heleus.Apps.Shared
{
    public class ProfileSearch
    {
        public bool IsValidSearchText(string text)
        {
            if (!string.IsNullOrEmpty(text))
            {
                if (text.Length >= ProfileServiceInfo.MinSearchLength)
                {
                    return true;
                }

                if (long.TryParse(text, out var id) && id > 0)
                {
                    return true;
                }
            }
            return false;
        }

        public async Task<List<ProfileInfo>> Search(string text)
        {
            var profiles = new List<ProfileInfo>();
            if (!IsValidSearchText(text))
                return profiles;

            var accountFound = false;

            long.TryParse(text, out var accountId);
            if (accountId > 0)
            {
                var profileInfo = (await ProfileManager.Current.GetProfileInfo(accountId, ProfileDownloadType.DownloadIfNotAvailable, false)).Profile;
                if (profileInfo != null)
                {
                    profiles.Add(profileInfo);
                }
                else
                {
                    accountFound = await IsValidServiceAccount(accountId);
                }
            }
            else
            {
                profiles.AddRange(await ProfileManager.Current.SearchProfiles(text));
            }

            profiles = await ProcessProfiles(profiles);
            if (accountFound)
                profiles.Add(new ProfileInfo(accountId, null, null, 0, 0, 0, 0));

            return profiles;
        }

        protected virtual Task<bool> IsValidServiceAccount(long accountId)
        {
            return Task.FromResult(false);
        }

        protected virtual Task<List<ProfileInfo>> ProcessProfiles(List<ProfileInfo> profiles)
        {
            return Task.FromResult(profiles);
        }
    }
}
