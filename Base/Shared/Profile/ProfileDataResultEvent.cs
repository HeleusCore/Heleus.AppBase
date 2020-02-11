namespace Heleus.Apps.Shared
{
    public class ProfileDataResultEvent
    {
        public readonly long AccountId;
        public readonly ProfileDataResult ProfileData;

        public ProfileDataResultEvent(long accountId, ProfileDataResult profileDataResult)
        {
            AccountId = accountId;
            ProfileData = profileDataResult;
        }
    }
}
