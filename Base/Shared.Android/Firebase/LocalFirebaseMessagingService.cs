using System;
using Android.Content;
using Android.App;
using Firebase.Messaging;
using Android.Util;

namespace Heleus.Apps.Shared.Android
{
    // [Service (Name= "packagename.MyFirebaseMessagingService")]
    [Service]
    [IntentFilter(new[] { "com.google.firebase.MESSAGING_EVENT" })]
    public class LocalFirebaseMessagingService : FirebaseMessagingService
    {
        const string TAG = "LocalFirebaseMessagingService";

        public const string NotificationOpenSingleAction = "NotificationOpenSingleAction";

        public override void OnMessageReceived(RemoteMessage p0)
        {
            Console.WriteLine("OnMessageReceived");
            FirebaseNotification.HandlePushMessage(this, p0);
        }

        public override void OnNewToken(string p0)
        {
            base.OnNewToken(p0);
            UIApp.Current?.RemoteNotifiactionTokenResult(p0);
        }
    }
}
