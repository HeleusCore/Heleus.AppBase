using System.Collections.Generic;
using Heleus.ProfileService;

namespace Heleus.Apps.Shared
{
    public class ProfileDataResult
    {
        public readonly ProfileInfo ProfileInfo;
        public readonly ProfileDownloadResult ProfileInfoResult;

        public readonly ProfileDownloadResult ProfileJsonResult;
        public readonly List<ProfileItemJson> ProfileJsonItems;

        public readonly ProfileDownloadResult ImageResult;
        public readonly byte[] Image;

        public ProfileDataResult(ProfileDownloadResult profileInfoResult, ProfileInfo profileInfo)
        {
            ProfileInfoResult = ProfileJsonResult = ImageResult = profileInfoResult;
            ProfileInfo = profileInfo;
        }

        public ProfileDataResult(ProfileDownloadResult profileInfoResult, ProfileInfo profileInfo, ProfileDownloadResult profileJsonResult, List<ProfileItemJson> profileJsonItems, ProfileDownloadResult imageResult, byte[] image) : this(profileInfoResult, profileInfo)
        {
            ProfileJsonResult = profileJsonResult;
            ProfileJsonItems = profileJsonItems;
            ImageResult = imageResult;
            Image = image;
        }
    }
}
