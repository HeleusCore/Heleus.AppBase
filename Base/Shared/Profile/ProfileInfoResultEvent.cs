namespace Heleus.Apps.Shared
{
    public class ProfileInfoResultEvent
    {
        public readonly long AccountId;
        public readonly ProfileInfoResult ProfileInfo;

        public ProfileInfoResultEvent(long accountId, ProfileInfoResult profileInfoResult)
        {
            AccountId = accountId;
            ProfileInfo = profileInfoResult;
        }
    }
}
