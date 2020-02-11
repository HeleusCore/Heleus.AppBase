using System;
using System.Threading.Tasks;
using Foundation;
using Heleus.Base;
using UIKit;
using Xamarin.Forms.Platform.iOS;

namespace Heleus.Apps.Shared.iOS
{
	[Register("AppDelegate")]
	public partial class AppDelegate : FormsApplicationDelegate
	{
        internal static Task ThemeChanged(ThemeChangedEvent e)
        {
            UISwitch.Appearance.OnTintColor = TintColor.ToUIColor();
            UITabBar.Appearance.TintColor = TintColor.ToUIColor();

            return Task.CompletedTask;
        }

        public override bool FinishedLaunching(UIApplication uiApplication, NSDictionary launchOptions)
        {
            Log.LogLevel = LogLevels.Trace;

            UIApp.PubSub.Subscribe<ThemeChangedEvent>(this, ThemeChanged);
            ThemeChanged(null);

            Xamarin.Forms.Forms.Init();

			LoadApplication(new UIApp());

			if (launchOptions != null)
			{
				if (launchOptions.ObjectForKey(UIApplication.LaunchOptionsRemoteNotificationKey) is NSDictionary remoteNotification)
				{
					HandleRemoteNotification(uiApplication, remoteNotification, null, null, true);
				}
			}

			return base.FinishedLaunching(uiApplication, launchOptions);
		}

        void HandleRemoteNotification(UIApplication application, NSDictionary userInfo, string action = null, Action completionHandler = null, bool fromUser = false)
		{
            try
            {
                var userInteraction = fromUser || !(application.ApplicationState == UIApplicationState.Active);

                var scheme = userInfo.ObjectForKey(FromObject("scheme")) as NSString;
                var silent = (userInfo.ObjectForKey(FromObject("silent")) as NSString) == "1";


                UIApp.Run(() => UIApp.PubSub.PublishAsync(new PushNotificationEvent(new Uri(scheme), silent ? PushNotificationEventType.Silent : (userInteraction ? PushNotificationEventType.UserInteraction : PushNotificationEventType.NoneUserInteraction))));

            }
            catch (Exception ex){
                Log.IgnoreException(ex);
            }

			completionHandler?.Invoke();
		}

		public override bool OpenUrl(UIApplication application, NSUrl url, string sourceApplication, NSObject annotation)
		{
            try
            {
                return UIApp.PublishSchemeRequest(url.AbsoluteString);
            } catch(Exception ex)
            {
                Log.IgnoreException(ex);
            }
            return false;
		}

		public override bool ContinueUserActivity(UIApplication application, NSUserActivity userActivity, UIApplicationRestorationHandler completionHandler)
		{
			if (userActivity.ActivityType == NSUserActivityType.BrowsingWeb)
			{
                try
                {
                    return UIApp.PublishSchemeRequest(userActivity.WebPageUrl.AbsoluteString);
                }catch (Exception ex)
                {
                    Log.IgnoreException(ex);
                }
			}

			return true;
		}

		public override void DidRegisterUserNotificationSettings(UIApplication application, UIUserNotificationSettings notificationSettings)
		{
			application.RegisterForRemoteNotifications();
		}

		public override void ReceivedRemoteNotification(UIApplication application, NSDictionary userInfo)
		{
			HandleRemoteNotification(application, userInfo);
		}

		public override void HandleAction(UIApplication application, string actionIdentifier, NSDictionary remoteNotificationInfo, Action completionHandler)
		{
			HandleRemoteNotification(application, remoteNotificationInfo, actionIdentifier, completionHandler);
		}

		public override void RegisteredForRemoteNotifications(UIApplication application, NSData deviceToken)
		{
			byte[] token = deviceToken.ToArray();
			UIApp.Current.RemoteNotifiactionTokenResult(Convert.ToBase64String(token));
		}

		public override void FailedToRegisterForRemoteNotifications(UIApplication application, NSError error)
		{
			UIApp.Current.RemoteNotifiactionTokenResult(null);
			Log.Write(error);
		}
	}
}
