using Heleus.ProfileService;
using TinyJson;

namespace Heleus.Apps.Shared
{
    public static class ProfileSearchResultExtenstion
    {
        public static ProfileInfo GetProfileInfo(this SearchResult searchResult, int index)
        {
            var profiles = searchResult.Profiles;
            if (index >= 0 && index < profiles.Count)
                return profiles[index]?.FromJson<ProfileInfoJson>()?.GetProfileInfo();

            return null;
        }
    }
}
