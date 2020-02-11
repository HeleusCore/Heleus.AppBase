using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Heleus.Base;
using Heleus.Chain.Core;
using Heleus.Network.Client;
using Heleus.Network.Results;
using Heleus.Operations;
using Heleus.ProfileService;
using TinyJson;

namespace Heleus.Apps.Shared
{
    public enum ProfileDownloadType
    {
        ForceDownload,
        DownloadIfNotAvailable,
        QueryStoredData
    }

    public enum ProfileDownloadResult
    {
        Available,
        NotAvailable,
        NetworkError
    }

    public class ProfileManager
    {
        public static ProfileManager Current { get; private set; }

        ClientBase _client;
        Storage _cacheStorage;
        PubSub _pubSub;

        Lazy<DiscStorage> _profileStorage;
        Lazy<DiscStorage> _jsonStorage;
        Lazy<DiscStorage> _imageStorage;

        readonly Dictionary<long, ProfileDataResult> _cachedProfileData = new Dictionary<long, ProfileDataResult>();
        readonly Dictionary<long, ProfileInfoResult> _cachedProfileInfo = new Dictionary<long, ProfileInfoResult>();

        public ProfileManager(ClientBase client, Storage cacheStorage, PubSub pubSub)
        {
            if (Current != null)
                throw new Exception("Only one instance allowed.");

            Current = this;

            _client = client;
            _cacheStorage = cacheStorage;
            _pubSub = pubSub;

            _profileStorage = new Lazy<DiscStorage>(() => new DiscStorage(_cacheStorage, "profiledata", 64, 0, DiscStorageFlags.UnsortedDynamicIndex));
            _jsonStorage = new Lazy<DiscStorage>(() => new DiscStorage(_cacheStorage, "profilejson", 0, 0, DiscStorageFlags.UnsortedDynamicIndex));
            _imageStorage = new Lazy<DiscStorage>(() => new DiscStorage(_cacheStorage, "profileimage", 0, 0, DiscStorageFlags.UnsortedDynamicIndex));
        }

        public ProfileInfoResult GetCachedProfileInfo(long accountId)
        {
            _cachedProfileInfo.TryGetValue(accountId, out var result);
            return result;
        }

        public ProfileDataResult GetCachedProfileData(long accountId)
        {
            _cachedProfileData.TryGetValue(accountId, out var result);
            return result;
        }

        void CacheProfileInfo(ProfileInfoResult profileInfoResult)
        {
            var profile = profileInfoResult.Profile;
            if(profile != null)
            {
                if(_cachedProfileInfo.TryGetValue(profile.AccountId, out var storedProfile))
                {
                    if(profile.ProfileTransactionId > storedProfile.Profile.ProfileTransactionId || profile.ImageTransactionId > storedProfile.Profile.ImageTransactionId)
                    {
                        _cachedProfileInfo[profile.AccountId] = profileInfoResult;
                    }
                }
                else
                {
                    _cachedProfileInfo[profile.AccountId] = profileInfoResult;
                }
            }
        }

        void CacheProfileData(ProfileDataResult profileDataResult)
        {
            var profile = profileDataResult.ProfileInfo;
            if(profile != null)
            {
                if (_cachedProfileData.TryGetValue(profile.AccountId, out var storedProfile))
                {
                    if (profile.ProfileTransactionId > storedProfile.ProfileInfo.ProfileTransactionId || profile.ImageTransactionId > storedProfile.ProfileInfo.ImageTransactionId)
                    {
                        _cachedProfileData[profile.AccountId] = profileDataResult;
                    }
                }
                else
                {
                    _cachedProfileData[profile.AccountId] = profileDataResult;
                }
            }
        }


        public async Task<List<ProfileInfo>> SearchProfiles(string searchText)
        {
            var result = new List<ProfileInfo>();

            if (!string.IsNullOrWhiteSpace(searchText) && searchText.Length >= ProfileServiceInfo.MinSearchLength)
            {
                var profilesResult = (await _client.QueryDynamicServiceData<SearchResult>(ProfileServiceInfo.ChainId, $"search/{searchText}/result.data")).Data;
                var profiles = profilesResult?.Item;

                if (profiles != null)
                {
                    for (var i = 0; i < profiles.Count; i++)
                    {
                        var profileInfo = profiles.GetProfileInfo(i);
                        if (profileInfo != null)
                        {
                            await UpdateProfileInfo(profileInfo);
                            result.Add(profileInfo);
                        }
                    }
                }
            }

            return result;
        }
 
        public async Task<ProfileInfoResult> GetProfileInfo(long accountId, ProfileDownloadType queryType, bool publishEvent)
        {
            if (queryType == ProfileDownloadType.QueryStoredData)
            {
                if (_cachedProfileInfo.TryGetValue(accountId, out var info))
                    return info;
            }

            var result = await GetProfileInfo(accountId, queryType);
            CacheProfileInfo(result);

            var evt = new ProfileInfoResultEvent(accountId, result);

            if (publishEvent)
                await _pubSub.PublishAsync(evt);

            return result;
        }

        async Task UpdateProfileInfo(ProfileInfo profileInfo)
        {
            if(profileInfo != null)
            {
                var result = new ProfileInfoResult(profileInfo);
                CacheProfileInfo(new ProfileInfoResult(profileInfo));

                var storage = _profileStorage.Value;

                var accountId = profileInfo.AccountId;
                if (!storage.ContainsIndex(accountId))
                {
                    storage.AddEntry(accountId, result.ToByteArray());
                }
                else
                {
                    var update = true;

                    var profileData = await storage.GetBlockDataAsync(accountId);
                    if (profileData != null)
                    {
                        var storedProfile = new ProfileInfoResult(new Unpacker(profileData))?.Profile;
                        if(storedProfile != null)
                        {
                            update = profileInfo.ProfileTransactionId > storedProfile.ProfileTransactionId;
                        }
                    }

                    if(update)
                        storage.UpdateEntry(accountId, result.ToByteArray());
                }

                await storage.CommitAsync();
            }
        }

        async Task<ProfileInfoResult> GetProfileInfo(long accountId, ProfileDownloadType queryType)
        {
            var storage = _profileStorage.Value;
            ProfileInfoResult storedProfile = null;

            try
            {
                if(accountId <= CoreAccount.NetworkAccountId)
                {
                    return new ProfileInfoResult((ProfileInfo)null);
                }

                var profileData = await storage.GetBlockDataAsync(accountId);
                if (profileData != null)
                {
                    storedProfile = new ProfileInfoResult(new Unpacker(profileData));
                }

                if (storedProfile != null && queryType == ProfileDownloadType.DownloadIfNotAvailable)
                {
                    return storedProfile;
                }

                var profileDownload = (await _client.QueryDynamicServiceData<ProfileInfo>(ProfileServiceInfo.ChainId, $"profileinfobyid/{accountId}/result.data")).Data;
                if (profileDownload != null)
                {
                    if (profileDownload.ResultType == ResultTypes.Ok)
                    {
                        var result = new ProfileInfoResult(profileDownload.Item);

                        if (storage.ContainsIndex(accountId))
                            storage.UpdateEntry(accountId, result.ToByteArray());
                        else
                            storage.AddEntry(accountId, result.ToByteArray());

                        await storage.CommitAsync();

                        return result;
                    }

                    return new ProfileInfoResult((ProfileInfo)null);
                }

                // fallback required, using indices
            }
            catch (Exception ex)
            {
                Log.IgnoreException(ex);
            }

            if (storedProfile != null)
            {
                return storedProfile;
            }

            return ProfileInfoResult.NetworkError;
        }

        /*
        public Task<ProfileDataResult> GetLocaleProfileData(ProfileDownloadType queryType, bool publishEvent)
        {
            return GetProfileData(LocalAccountId, queryType, publishEvent);
        }
        */

        public async Task<ProfileDataResult> GetProfileData(long accountId, ProfileDownloadType queryType, bool publishEvent)
        {
            if (queryType == ProfileDownloadType.QueryStoredData)
            {
                if (_cachedProfileData.TryGetValue(accountId, out var data))
                    return data;
            }

            var result = await GetProfileData(accountId, queryType);
            CacheProfileData(result);

            var evt = new ProfileDataResultEvent(accountId, result);

            if (publishEvent)
                await _pubSub.PublishAsync(evt);

            return result;
        }

        async Task<ProfileDataResult> GetProfileData(long accountId, ProfileDownloadType queryType)
        {
            var profileInfo = await GetProfileInfo(accountId, queryType);
            try
            {
                if (profileInfo.Result == ProfileDownloadResult.Available)
                {
                    var profile = profileInfo.Profile;
                    if (profile != null)
                    {
                        var hasImage = profile.ImageTransactionId > Operation.InvalidTransactionId;
                        byte[] imageData = null;

                        string json = null;
                        List<ProfileItemJson> jsonItems = null;

                        var jsonStorage = _jsonStorage.Value;
                        var jsonData = await jsonStorage.GetBlockDataAsync(profile.ProfileTransactionId);
                        if (jsonData == null)
                        {
                            jsonData = (await _client.DownloadAttachement(profile.ProfileTransactionId, ProfileServiceInfo.ChainId, ProfileServiceInfo.ChainIndex, profile.ProfileAttachementKey, ProfileServiceInfo.ProfileJsonFileName)).Data;
                            if (jsonData != null)
                            {
                                if (!jsonStorage.ContainsIndex(profile.ProfileTransactionId))
                                {
                                    jsonStorage.AddEntry(profile.ProfileTransactionId, jsonData);
                                    await jsonStorage.CommitAsync();
                                }
                            }
                        }

                        if (jsonData != null)
                        {
                            json = Encoding.UTF8.GetString(jsonData);
                            if (json != null)
                            {
                                jsonItems = json.FromJson<List<ProfileItemJson>>();
                            }
                        }

                        if (hasImage)
                        {
                            var imageStorage = _imageStorage.Value;
                            imageData = await imageStorage.GetBlockDataAsync(profile.ImageTransactionId);
                            if (imageData == null)
                            {
                                imageData = (await _client.DownloadAttachement(profile.ImageTransactionId, ProfileServiceInfo.ChainId, ProfileServiceInfo.ChainIndex, profile.ImageAttachementKey, ProfileServiceInfo.ImageFileName)).Data;
                                if (imageData != null)
                                {
                                    if (!imageStorage.ContainsIndex(profile.ImageTransactionId))
                                    {
                                        imageStorage.AddEntry(profile.ImageTransactionId, imageData);
                                        await imageStorage.CommitAsync();
                                    }
                                }
                            }
                        }

                        var jsonResult = jsonItems != null ? ProfileDownloadResult.Available : ProfileDownloadResult.NetworkError;
                        var imageResult = hasImage ? (imageData != null ? ProfileDownloadResult.Available : ProfileDownloadResult.NetworkError) : ProfileDownloadResult.NotAvailable;

                        var result = new ProfileDataResult(profileInfo.Result, profileInfo.Profile, jsonResult, jsonItems, imageResult, imageData);

                        /*
                        if (accountId == LocalAccountId)
                            LocalProfileData = result;
                        */

                        return result;
                    }
                }
            }
            catch(Exception ex)
            {
                Log.IgnoreException(ex);
            }

            return new ProfileDataResult(profileInfo.Result, profileInfo.Profile);
        }
    }
}
