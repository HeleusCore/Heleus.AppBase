using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Heleus.Base;
using Heleus.Network.Client;
using Heleus.Service.Push;

namespace Heleus.Apps.Shared
{
    public enum PushTokenSyncResult
    {
        Busy,
        UnknownError,

        PushDisabled,

        NoPushTokenAvailable,
        NoAppropriateServiceNodeFound,

        UploadFailure,
        NoResponse,
        WrongResponse,

        UploadAckFailure,
        NoAckResponse,
        WrongAckResponse,

        AlreadySynced,
        Ok
    }

    public enum PushTokenSyncState
    {
        QueryPushToken = 0,
        QueryServiceNode,

        UploadingPushToken,
        AwaitingResponse,

        UploadingPushTokenAck,
        AwaitingResponseAck,

        Done
    }

    public class PushTokenSyncStateEvent
    {
        public readonly PushTokenSyncState SyncState;

        public PushTokenSyncStateEvent(PushTokenSyncState state)
        {
            SyncState = state;
        }
    }

    public partial class UIApp
    {
        async Task PushNotification(PushNotificationEvent pushNotification)
        {
            var schemeAction = SchemeAction.ParseSchemeAction(pushNotification.Scheme);
            if (schemeAction != null)
            {
                if (schemeAction is PushTokenSchemeAction pushTokenScheme)
                {
                    if (schemeAction.IsValid)
                        _pushTokenInfoResponse?.SetResult(new PushTokenInfo(PushBrokerType, pushTokenScheme.Token, pushTokenScheme.AccountId, pushTokenScheme.ChallengeCode));
                    else
                        _pushTokenInfoResponse?.SetResult(null);
                }

                if (pushNotification.EventType == PushNotificationEventType.Silent || pushNotification.EventType == PushNotificationEventType.UserInteraction)
                    await schemeAction.Run();
            }
        }

        public void RemoteNotifiactionTokenResult(string token)
        {
            if (token != null)
            {
                Log.Write(string.Format("\n\nPush Token\n{0}\n\n", token));
            }
            else
            {
                Log.Write("No Push Token available");
            }

            _currentPushToken = token;

            if(!string.IsNullOrEmpty(token) && token != _uploadedPushToken)
                Run(() => SyncPushToken(false));
        }

        (TaskCompletionSource<PushTokenInfo>, CancellationTokenSource) GetPushTokenCompletionSource()
        {
            var pushTokenInfoResponse = new TaskCompletionSource<PushTokenInfo>(TaskCreationOptions.RunContinuationsAsynchronously);
            var cancelation = new CancellationTokenSource(TimeSpan.FromSeconds(10));

            cancelation.Token.Register(() =>
            {
                try
                {
                    pushTokenInfoResponse.SetResult(null);
                }
                catch (Exception ex)
                {
                    Log.IgnoreException(ex);
                }
            });

            return (pushTokenInfoResponse, cancelation);
        }

        public bool IsPushChannelSubscribed(Chain.Index channel) => _subscribedChannels.Contains(channel);

        public async Task SyncPushChannelSubscriptions()
        {
            var serviceNode = ServiceNodeManager.Current.FirstDefaultServiceNode;
            var serviceAccount = serviceNode?.FirstUnlockedServiceAccount;
            if (serviceNode == null || serviceAccount == null)
            {
                return;
            }

            if (!PushNotificationsEnabled)
                return;

            var result = (await serviceNode.Client.QueryDynamicServiceData<PushSubscriptionResponse>(serviceNode.ChainId, $"pushservice/lastupdate/{serviceNode.AccountId}/result.data")).Data?.Item;
            if(result != null)
            {
                var storedTimestamp = result.StoredTimestamp;
                if(storedTimestamp > 0 && storedTimestamp != _lastChannelUpdate)
                {
                    if(await SyncSubscriptions())
                    {
                        Run(SaveSettings);
                    }
                }
            }
        }

        readonly Chain.Index _subscriptionIndex = Chain.Index.New().Add((short)0).Build();

        async Task<bool> SyncSubscriptions()
        {
            var serviceNode = ServiceNodeManager.Current.FirstDefaultServiceNode;
            var serviceAccount = serviceNode?.FirstUnlockedServiceAccount;
            if (serviceNode == null || serviceAccount == null)
            {
                return false;
            }

            var response = (await serviceNode.Client.SendPushSubscription(new PushSubscription(PushSubscriptionAction.Query, serviceAccount.AccountId, _subscriptionIndex), serviceAccount))?.Response;
            if(response != null && response.SubscriptionResult == PushSubscriptionResult.Ok)
            {
                var subscriptions = new HashSet<Chain.Index>();
                foreach (var channel in response.Channels)
                    subscriptions.Add(channel);
                _subscribedChannels = subscriptions;
                _lastChannelUpdate = response.StoredTimestamp;

                return true;
            }

            return false;
        }

        public async Task<bool> ChangePushChannelSubscription(ExtContentPage page, Chain.Index channel)
        {
            var action = _subscribedChannels.Contains(channel) ? PushSubscriptionAction.Delete : PushSubscriptionAction.Insert;

            var noSyncConfirm = false;

            if (!PushNotificationsEnabled)
            {
                if (!await page.ConfirmAsync("ConfirmEnableNotifications"))
                {
                    return false;
                }

                PushNotificationsEnabled = true;
                SaveSettings();

                noSyncConfirm = true;
            }

            if (!PushTokenSynchronized)
            {
                if (noSyncConfirm || await page.ConfirmAsync("ConfirnSynchronizePushTokens"))
                {
                    await page.Navigation.PushAsync(new PushNotificationPage());
                }

                return false;
            }

            var serviceNode = ServiceNodeManager.Current.FirstDefaultServiceNode;
            var serviceAccount = serviceNode?.FirstUnlockedServiceAccount;
            if (serviceNode == null || serviceAccount == null)
            {
                await page.ErrorAsync("PushMissingAccount");
                return false;
            }

            var result = await serviceNode.Client.SendPushSubscription(new PushSubscription(action, serviceAccount.AccountId, channel), serviceAccount);
            if (result.ResponseResult == PushSubscriptionResult.Ok)
            {
                var response = result.Response;

                if(_lastChannelUpdate < response.StoredTimestamp)
                {
                    await SyncSubscriptions();
                }
                else
                {
                    _lastChannelUpdate = response.CurrentTimestamp;
                }

                if (action == PushSubscriptionAction.Insert)
                    _subscribedChannels.Add(channel);
                else
                    _subscribedChannels.Remove(channel);

                SaveSettings();

                return true;
            }

            if (result.ResultType == HeleusClientResultTypes.ServiceNodeAccountMissing)
                await page.ErrorAsync("PushMissingAccount");
            else
                await page.ErrorTextAsync(Tr.Get($"HeleusClientResultTypes.{result.ResultType}"));

            return false;
        }

        public async Task<PushTokenSyncResult> SyncPushToken(bool forceSync)
        {
            if (SyncPushBusy)
                return PushTokenSyncResult.Busy;

            SyncPushBusy = true;

            try
            {
                PushTokenSyncResult result;
                CancellationTokenSource cancelation = null;

                await PubSub.PublishAsync(new PushTokenSyncStateEvent(PushTokenSyncState.QueryPushToken));

                if (!PushNotificationsEnabled)
                {
                    result = PushTokenSyncResult.PushDisabled;
                    goto cleanup;
                }

                EnableRemoteNotifications();
                await Task.Delay(1000);

                if (string.IsNullOrEmpty(_currentPushToken))
                {
                    result = PushTokenSyncResult.NoPushTokenAvailable;
                    goto cleanup;
                }

                if (_currentPushToken == _uploadedPushToken && !forceSync)
                {
                    result = PushTokenSyncResult.AlreadySynced;
                    goto cleanup;
                }

                await PubSub.PublishAsync(new PushTokenSyncStateEvent(PushTokenSyncState.QueryServiceNode));

                var serviceNode = ServiceNodeManager.Current.FirstDefaultServiceNode;
                var serviceAccount = serviceNode?.FirstUnlockedServiceAccount;
                if (serviceNode == null || serviceAccount == null)
                {
                    result = PushTokenSyncResult.NoAppropriateServiceNodeFound;
                    goto cleanup;
                }

                (_pushTokenInfoResponse, cancelation) = GetPushTokenCompletionSource();

                await PubSub.PublishAsync(new PushTokenSyncStateEvent(PushTokenSyncState.UploadingPushToken));

                var localPushTokenInfo = new PushTokenInfo(PushBrokerType, _currentPushToken, serviceNode.AccountId, 0);

                if (!await serviceNode.Client.UploadPushTokenInfo(localPushTokenInfo, serviceAccount))
                {
                    result = PushTokenSyncResult.UploadFailure;
                    goto cleanup;
                }

                await PubSub.PublishAsync(new PushTokenSyncStateEvent(PushTokenSyncState.AwaitingResponse));

                var remotePushTokenInfo = await _pushTokenInfoResponse.Task;
                if (remotePushTokenInfo == null)
                {
                    result = PushTokenSyncResult.NoResponse;
                    goto cleanup;
                }

                if (!remotePushTokenInfo.IsValid || remotePushTokenInfo.Token != localPushTokenInfo.Token)
                {
                    result = PushTokenSyncResult.WrongResponse;
                    goto cleanup;
                }

                try
                {
                    cancelation.Dispose();
                }
                catch { }

                (_pushTokenInfoResponse, cancelation) = GetPushTokenCompletionSource();

                await PubSub.PublishAsync(new PushTokenSyncStateEvent(PushTokenSyncState.UploadingPushTokenAck));

                if (!await serviceNode.Client.UploadPushTokenInfo(remotePushTokenInfo, serviceAccount))
                {
                    result = PushTokenSyncResult.UploadAckFailure;
                    goto cleanup;
                }

                await PubSub.PublishAsync(new PushTokenSyncStateEvent(PushTokenSyncState.AwaitingResponseAck));

                var finalPushTokenInfo = await _pushTokenInfoResponse.Task;

                if (finalPushTokenInfo == null)
                {
                    result = PushTokenSyncResult.NoAckResponse;
                    goto cleanup;
                }

                if (!finalPushTokenInfo.IsValid || finalPushTokenInfo.Token != localPushTokenInfo.Token || finalPushTokenInfo.ChallengeCode != 0)
                {
                    result = PushTokenSyncResult.WrongAckResponse;
                    goto cleanup;
                }

                _uploadedPushToken = finalPushTokenInfo.Token;
                SaveSettings();

                result = PushTokenSyncResult.Ok;

                await PubSub.PublishAsync(new PushTokenSyncStateEvent(PushTokenSyncState.Done));

            cleanup:
                try
                {
                    cancelation?.Dispose();
                }
                catch { }

                SyncPushBusy = false;

                return result;
            }
            catch (Exception ex)
            {
                Log.HandleException(ex);
            }

            SyncPushBusy = false;
            return PushTokenSyncResult.UnknownError;
        }

        void RemoveNotifications()
        {
#if __IOS__
            try
            {
                if (AutoRemoveNotifications)
                {
                    UIKit.UIApplication.SharedApplication.ApplicationIconBadgeNumber = 1;
                    UIKit.UIApplication.SharedApplication.ApplicationIconBadgeNumber = 0;
                    UIKit.UIApplication.SharedApplication.CancelAllLocalNotifications();
                }
            }
            catch { }
#endif

#if ANDROID
            try
            {
                if (AutoRemoveNotifications)
                {
                    var nm = Android.MainActivity.Current.GetSystemService(global::Android.Content.Context.NotificationService) as global::Android.App.NotificationManager;
                    nm.CancelAll();
                }
            }
            catch { }

#endif

#if UWP
            try
            {
                if(AutoRemoveNotifications)
				{
                    Windows.UI.Notifications.ToastNotificationManager.History.Clear();
				}

                var settings = Windows.Storage.ApplicationData.Current.LocalSettings.Values;
                settings["badgeCount"] = 0;
            }
            catch { }

            var badgeXml = new Windows.Data.Xml.Dom.XmlDocument();
            badgeXml.LoadXml(@"<?xml version=""1.0"" encoding=""UTF-8""?><badge value=""none""></badge>");

            Windows.UI.Notifications.BadgeUpdateManager.CreateBadgeUpdaterForApplication().Update(new Windows.UI.Notifications.BadgeNotification(badgeXml));
#endif

#if MACOS
            try
            {
				if (AutoRemoveNotifications)
				{
					Foundation.NSUserNotificationCenter.DefaultUserNotificationCenter.RemoveAllDeliveredNotifications();
				}
			}
			catch (Exception ex)
			{
                Log.IgnoreException(ex);
			}
#endif
        }
    }
}
