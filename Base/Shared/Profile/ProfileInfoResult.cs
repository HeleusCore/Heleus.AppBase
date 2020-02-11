using Heleus.Base;
using Heleus.ProfileService;

namespace Heleus.Apps.Shared
{
    public class ProfileInfoResult : IPackable
    {
        public readonly ProfileDownloadResult Result;
        public readonly long LastChecked;
        public readonly ProfileInfo Profile;

        public static ProfileInfoResult NetworkError => new ProfileInfoResult(ProfileDownloadResult.NetworkError);

        ProfileInfoResult(ProfileDownloadResult result)
        {
            Result = result;
            LastChecked = Time.Timestamp;
        }

        public ProfileInfoResult(ProfileInfo profile) : this(profile != null ? ProfileDownloadResult.Available : ProfileDownloadResult.NotAvailable)
        {
            Profile = profile;
        }

        public ProfileInfoResult(Unpacker unpacker)
        {
            Result = (ProfileDownloadResult)unpacker.UnpackByte();
            unpacker.Unpack(out LastChecked);
            if (Result == ProfileDownloadResult.Available)
                Profile = new ProfileInfo(unpacker);
        }

        public void Pack(Packer packer)
        {
            packer.Pack((byte)Result);
            packer.Pack(LastChecked);

            if (Result == ProfileDownloadResult.Available)
                packer.Pack(Profile);
        }

        public byte[] ToByteArray()
        {
            using (var packer = new Packer())
            {
                Pack(packer);
                return packer.ToByteArray();
            }
        }
    }
}
