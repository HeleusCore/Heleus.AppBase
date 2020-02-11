using System.Threading.Tasks;
using System.Collections.Generic;
using System.Linq;

namespace Heleus.Apps.Shared
{
    public class PushNotificationPage : StackPage
    {
        readonly bool _busy;
        readonly List<SeparatorRow> _rows = new List<SeparatorRow>();

        static readonly PushTokenSyncState[] _syncStates =
        {
            PushTokenSyncState.QueryPushToken,
            PushTokenSyncState.QueryServiceNode,

            PushTokenSyncState.UploadingPushToken,
            PushTokenSyncState.AwaitingResponse,

            PushTokenSyncState.UploadingPushTokenAck,
            PushTokenSyncState.AwaitingResponseAck,

            PushTokenSyncState.Done
        };

        public PushNotificationPage() : base("PushNotificationPage")
        {
            _busy = UIApp.Current.SyncPushBusy;

            AddTitleRow("Title");

            if (_busy)
            {
                AddHeaderRow("BusyHeader");
                AddInfoRow("Busy");
                AddFooterRow();
            }
            else
            {
                Subscribe<PushTokenSyncStateEvent>(SyncState);

                IsBusy = true;
            }
        }

        async Task Process(bool forceSync)
        {
            _rows.Clear();
            AddHeaderRow("States");
            AddFooterRow();

            var result = await UIApp.Current.SyncPushToken(forceSync);
            UIApp.Run(UIApp.Current.SyncPushChannelSubscriptions);

            IsBusy = false;

            AddHeaderRow("Result");

            var text = Tr.Get($"PushTokenSyncResult.{result}");
            if (result == PushTokenSyncResult.Ok || result == PushTokenSyncResult.AlreadySynced)
            {
                await MessageTextAsync(text);
            }
            else
            {
                AddTextRow(text);

                await ErrorTextAsync(text);

                if (result == PushTokenSyncResult.NoPushTokenAvailable)
                {
                    if (!string.IsNullOrEmpty(UIApp.RemoteNotificationsError))
                        await ErrorTextAsync(UIApp.RemoteNotificationsError);
#if __IOS__
                    if(await ConfirmAsync("ConfirmIosPushSettings"))
                    {
                        UIKit.UIApplication.SharedApplication.OpenUrl(new Foundation.NSUrl(UIKit.UIApplication.OpenSettingsUrlString));
                    }
#endif

#if ANDROID
                    var resultCode = global::Android.Gms.Common.GoogleApiAvailability.Instance.IsGooglePlayServicesAvailable(Heleus.Apps.Shared.Android.MainActivity.Current);
                    if (resultCode != global::Android.Gms.Common.ConnectionResult.Success)
                    {
                        if (global::Android.Gms.Common.GoogleApiAvailability.Instance.IsUserResolvableError(resultCode))
                        {
                            global::Android.Gms.Common.GoogleApiAvailability.Instance.ShowErrorNotification(Heleus.Apps.Shared.Android.MainActivity.Current, resultCode);
                        }
                    }
#endif
                }
            }

            AddSubmitButtonRow("Retry", Retry);

            AddFooterRow();
        }

        public override async Task InitAsync()
        {
            if (_busy)
                return;

            await Process(false);
        }

        async Task Retry(ButtonRow arg)
        {
            IsBusy = true;

            RemoveHeaderSection("States");
            RemoveHeaderSection("Result");

            await Process(true);
        }

        Task SyncState(PushTokenSyncStateEvent arg)
        {
            if (_busy)
                return Task.CompletedTask;

            var header = GetRow<HeaderRow>("States");

            var stateIndex = 0;
            for (var i = 0; i <= _syncStates.Length; i++)
            {
                if (_syncStates[i] == arg.SyncState)
                {
                    stateIndex = i;
                    break;
                }
            }

            AddIndex = header;
            for (var i = 0; i <= stateIndex; i++)
            {
                if (_rows.Count > 0)
                    AddIndex = _rows.Last();

                if (_rows.Count <= i)
                {
                    var state = _syncStates[i];

                    var row = AddTextRow(Tr.Get($"PushTokenSyncState.{state}"));
                    row.SetDetailViewIcon(Icons.RowMore);
                    _rows.Add(row);
                }

                /*
                if(i > 0)
                {
                    var prevRow = _rows[i - 1];
                }
                */
            }

            AddIndex = null;

            return Task.CompletedTask;
        }
    }
}

