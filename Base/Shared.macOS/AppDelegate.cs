using System;
using System.IO;
using System.Threading.Tasks;
using AppKit;
using CoreGraphics;
using Foundation;
using Heleus.Base;
using Xamarin.Forms;
using Xamarin.Forms.Platform.MacOS;

namespace Heleus.Apps.Shared.macOS
{
    [Register("AppDelegate")]
    public class AppDelegate : FormsApplicationDelegate
    {
        public static NSWindow Window { get; private set; }
        UIApp _app;
        string _schemeRequest = string.Empty;

        public override NSWindow MainWindow
        {
            get { return Window; }
        }

        class XWindow : NSWindow
        {
            public XWindow() : base(new CGRect(0, 0, 0, 0), /*NSWindowStyle.FullSizeContentView |*/ NSWindowStyle.Closable | NSWindowStyle.Miniaturizable | NSWindowStyle.Resizable | NSWindowStyle.Titled | NSWindowStyle.UnifiedTitleAndToolbar, NSBackingStore.Buffered, false)
            {
                Title = string.Empty;
                TitleVisibility = NSWindowTitleVisibility.Hidden;
                Restorable = true;
                //Toolbar = new NSToolbar("init");
                MovableByWindowBackground = false;
                SharingType = NSWindowSharingType.ReadOnly;
                MinSize = new CGSize(500, 500);
                //Appearance = NSAppearance.GetAppearance(NSAppearance.NameVibrantDark);
            }

            public override void MouseDown(NSEvent theEvent)
            {
                base.MouseDown(theEvent);
                //MakeFirstResponder(null);
            }
        }

        public AppDelegate()
        {
            Window = new XWindow();
        }

        void WindowResized(NSNotification notification)
        {
            var content = Window.ContentRectFor(Window.Frame);
            var width = (int)Window.Frame.Width;
            var height = (int)Window.Frame.Height;

            if (width != Theme.WindowWidth || height != Theme.WindowHeight)
            {
                Theme.WindowWidth = width;
                Theme.WindowHeight = height;

                Theme.Save();
            }
        }

        void WindowMoved(NSNotification notification)
        {
            var x = (int)Window.Frame.X;
            var y = (int)Window.Frame.Y;

            if (x != Theme.WindowX || y != Theme.WindowY)
            {
                Theme.WindowX = x;
                Theme.WindowY = y;

                Theme.Save();
            }
        }

        static void StoreNotification(string source, NSNotification notification)
        {
            var text = string.Empty;
            if (File.Exists(StorageInfo.CacheStorage.RootPath + "notifications.txt"))
            {
                text = File.ReadAllText(StorageInfo.CacheStorage.RootPath + "notifications.txt");
            }

            var old = text;
            text = source + "\n";
            if (notification.UserInfo != null)
            {
                text += notification.UserInfo + "\n";
            }

            File.WriteAllText(StorageInfo.CacheStorage.RootPath + "notifications.txt", text + old);
        }

        public override void WillFinishLaunching(NSNotification notification)
        {
            //StoreNotification("WillFinishLaunching", notification);
            NSUserNotificationCenter.DefaultUserNotificationCenter.DidActivateNotification += DidActivateNotification;
            NSUserNotificationCenter.DefaultUserNotificationCenter.DidDeliverNotification += DidDeliverNotification;
            NSAppleEventManager.SharedAppleEventManager.SetEventHandler(this, new ObjCRuntime.Selector("handleURLEvent:withReplyEvent:"), AEEventClass.Internet, AEEventID.GetUrl);
        }

        public override void DidFinishLaunching(NSNotification notification)
        {
            Log.LogLevel = LogLevels.Trace;

            var x = Theme.WindowX;
            var y = Theme.WindowY;
            var w = Theme.WindowWidth;
            var h = Theme.WindowHeight;
            Window.SetFrame(new CGRect(x, y, w, h - 38), false);

            //StoreNotification("DidFinishLaunching", notification);
            UIApp.PubSub.Subscribe<ThemeChangedEvent>(this, ThemeChanged);

            ThemeChanged(null);

            Forms.Init();
            _app = new UIApp();
            UIApp.Current.SetupMainMenu();
            LoadApplication(_app);

            try
            {
                var userInfo = notification.UserInfo;
                if (userInfo != null)
                {
                    var userNotification = (NSUserNotification)userInfo.ObjectForKey((NSString)FromObject("NSApplicationLaunchUserNotificationKey"));
                    if (userNotification != null)
                    {
                        DidActivateNotification(this, new UNCDidActivateNotificationEventArgs(userNotification));
                    }
                }
            }
            catch { }

            base.DidFinishLaunching(notification);
            ProcessSchemeRequest();

            NSNotificationCenter.DefaultCenter.AddObserver(new NSString("NSWindowDidResizeNotification"), WindowResized, null);
            NSNotificationCenter.DefaultCenter.AddObserver(new NSString("NSWindowDidMoveNotification"), WindowMoved, null);

            UIApp.Current.SetMainMenu();
        }

        [Export("handleURLEvent:withReplyEvent:")]
        void HandleURLEvent(NSAppleEventDescriptor evt, NSAppleEventDescriptor replyEvt)
        {
            try
            {
                _schemeRequest = evt.ParamDescriptorForKeyword((uint)FourCC.ToFourCC("----")).StringValue;
                ProcessSchemeRequest();
            }
            catch (Exception ex)
            {
                Log.IgnoreException(ex);
            }
        }

        void ProcessSchemeRequest()
        {
            if (_app != null && !string.IsNullOrEmpty(_schemeRequest))
            {
                UIApp.PublishSchemeRequest(_schemeRequest);
                _schemeRequest = string.Empty;
            }
        }

        public override NSApplicationTerminateReply ApplicationShouldTerminate(NSApplication sender)
        {
            return NSApplicationTerminateReply.Now;
        }

        public override void WillTerminate(NSNotification notification)
        {

        }

        public override bool ApplicationShouldTerminateAfterLastWindowClosed(NSApplication sender)
        {
#if DEBUG
			return true;
#else
            return true;
#endif
        }

        void ShowWindow()
        {
            if (!Window.IsVisible)
            {
                Window.Display();
                Window.MakeKeyAndOrderFront(NSApplication.SharedApplication);
            }
        }

        public override bool ApplicationShouldHandleReopen(NSApplication sender, bool hasVisibleWindows)
        {
            if (hasVisibleWindows)
            {
                Window.OrderFront(sender);
                return true;
            }

            ShowWindow();
            return true;
        }

        Task ThemeChanged(ThemeChangedEvent e)
        {
            var theme = Theme.WindowTheme;

            if (theme == WindowTheme.Dark)
                Window.Appearance = NSAppearance.GetAppearance(NSAppearance.NameVibrantDark);
            else
                Window.Appearance = NSAppearance.GetAppearance(NSAppearance.NameVibrantLight);

            return Task.CompletedTask;
        }

        public override void RegisteredForRemoteNotifications(NSApplication application, NSData deviceToken)
        {
            var token = deviceToken.ToArray();
            _app?.RemoteNotifiactionTokenResult(Convert.ToBase64String(token));

            //var hex = BitConverter.ToString(token).Replace("-", string.Empty);
            UIApp.RemoteNotificationsError = string.Empty;
        }

        public override void FailedToRegisterForRemoteNotifications(NSApplication application, NSError error)
        {
            _app?.RemoteNotifiactionTokenResult(null);
            UIApp.RemoteNotificationsError = error.LocalizedDescription;
            Log.Write(error.ToString());
        }

        public override void ReceivedRemoteNotification(NSApplication application, NSDictionary userInfo)
        {
            HandleRemoteNofification(userInfo, false);
        }

        void DidDeliverNotification(object sender, UNCDidDeliverNotificationEventArgs e)
        {
            if (Window.IsKeyWindow)
            {
                var notification = e?.Notification;
                if (notification != null)
                {
                    NSUserNotificationCenter.DefaultUserNotificationCenter.RemoveDeliveredNotification(notification);
                }
            }
        }

        void DidActivateNotification(object sender, UNCDidActivateNotificationEventArgs e)
        {
            var notification = e?.Notification;
            if (notification != null)
            {
                ShowWindow();

                NSUserNotificationCenter.DefaultUserNotificationCenter.RemoveDeliveredNotification(notification);
                NSDictionary userInfo = e?.Notification?.UserInfo;

                HandleRemoteNofification(userInfo, true);
            }
        }

        void HandleRemoteNofification(NSDictionary userInfo, bool userInteraction)
        {
            try
            {
                var scheme = userInfo.ObjectForKey(FromObject("scheme")) as NSString;
                var silent = (userInfo.ObjectForKey(FromObject("silent")) as NSString) == "1";

                if (!string.IsNullOrEmpty(scheme))
                    UIApp.Run(() => UIApp.PubSub.PublishAsync(new PushNotificationEvent(new Uri(scheme), silent ? PushNotificationEventType.Silent : (userInteraction ? PushNotificationEventType.UserInteraction : PushNotificationEventType.NoneUserInteraction))));
            }
            catch (Exception ex)
            {
                Log.IgnoreException(ex);
            }
        }
    }
}
