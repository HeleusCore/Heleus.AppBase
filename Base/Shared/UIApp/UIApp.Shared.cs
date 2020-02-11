using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Heleus.Base;
using Heleus.Service.Push;
using Xamarin.Forms;

#if ANDROID
using Xamarin.Forms.Platform.Android;
#elif MACOS
using Xamarin.Forms.Platform.MacOS;
#elif __IOS__
using Xamarin.Forms.Platform.iOS;
#elif UWP
using Xamarin.Forms.Platform.UWP;
#elif GTK
using Xamarin.Forms.Platform.GTK;
#elif WPF
using Xamarin.Forms.Platform.WPF;
#elif CLI
class Platform { }
#endif

namespace Heleus.Apps.Shared
{
    public static class PubSubExtension
    {
        public static void Publish<T>(this PubSub pubSub, T data)
        {
            UIApp.Run(() => pubSub.PublishAsync(data));
        }
    }

	partial class UIApp
    {
        public bool PushTokenSynchronized => !string.IsNullOrEmpty(_uploadedPushToken);
        public static string RemoteNotificationsError = string.Empty;
        public bool SyncPushBusy { get; private set; }

        public bool AutoRemoveNotifications = true;
        public bool PushNotificationsEnabled = false;
        public bool SendErrorReports = true;

        string _currentPushToken;
        string _uploadedPushToken;

        long _lastChannelUpdate;
        HashSet<Chain.Index> _subscribedChannels = new HashSet<Chain.Index>();
        TaskCompletionSource<PushTokenInfo> _pushTokenInfoResponse;

        public static PubSub PubSub = new PubSub();

        public static new UIApp Current
        {
            get;
            private set;
        }

        public static bool IsMacOS
        {
            get;
            private set;
        }
#if MACOS
		= true;
#endif

        public static bool IsIOS
        {
            get;
            private set;
        }
#if __IOS__
        = true;
#endif

        public static bool IsUWP
        {
            get;
            private set;
        }
#if UWP
        = true;
#endif
        public const int WindowsPartialCollapseSize = 48;

        public static bool IsAndroid
        {
            get;
            private set;
        }
#if ANDROID
		= true;
#endif

        public static bool IsGTK
        {
            get;
            private set;
        }
#if GTK
		= true;
#endif

        public static bool IsWPF
        {
            get;
            private set;
        }
#if WPF
        = true;
#endif

        public static bool IsCLI
        {
            get;
            private set;
        }
#if CLI
        = true;
#endif

        public static bool IsDesktop => IsMacOS || IsGTK || IsWPF;

        public static string LanguageString
        {
            get;
            private set;
        }

        public static readonly BindableProperty RenderProperty = (BindableProperty)typeof(Platform).GetField("RendererProperty", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Static)?.GetValue(null);

        public static bool CanShare => IsMacOS || IsIOS || IsAndroid || IsUWP;

        public UIApp()
		{
			Current = this;
            Log.PubSub = PubSub;
            Log.LogLevel = LogLevels.Trace;
            _currentSettings = new Settings();

			PlatformInit();
            InitErrorReporting();

			if (!IsUWP) // Windows inits in WindowsApp constructor
				Tr.Initalize(LanguageString);
                
            PubSub.Subscribe<ThemeChangedEvent>(this, ThemeChanged);
            PubSub.Subscribe<SchemeRequestEvent>(this, SchemeRequest);
            PubSub.Subscribe<PushNotificationEvent>(this, PushNotification);

            SchemeAction.RegisterSchemeAction<AddServiceNodeSchemeAction>();
            SchemeAction.RegisterSchemeAction<PushTokenSchemeAction>();

            ModalPopped += (object sender, ModalPoppedEventArgs e) =>
			{
				var p = (e.Modal as ExtContentPage);
				if (p != null)
					p.OnPopped();
			};

			// otfinfo -i fontawesome-light-300.ttf, brew install lcdf-typetools
			FontIcon.RegisterIconFont(new FontIcon.FontInfo('\uf000', '\uffff', IconSet.Light)
			{
                FileName = "fa-light-300.ttf",
                FullName = "Font Awesome 5 Pro Light",
                PostScriptName = "FontAwesome5Pro-Light",
				Windows = "Fonts/fa-light-300.ttf#Font Awesome 5 Pro"
            });

			FontIcon.RegisterIconFont(new FontIcon.FontInfo('\uf000', '\uffff', IconSet.Regular)
			{
                FileName = "fa-regular-400.ttf",
                PostScriptName = "FontAwesome5Pro-Regular",
                FullName = "Font Awesome 5 Pro Regular",
                Windows = "Fonts/fa-regular-400.ttf#Font Awesome 5 Pro"
            });

			FontIcon.RegisterIconFont(new FontIcon.FontInfo('\uf000', '\uffff', IconSet.Solid)
			{
                FileName = "fa-solid-900.ttf",
                PostScriptName = "FontAwesome5Pro-Solid",
                FullName = "Font Awesome 5 Pro Solid",
                Windows = "Fonts/fa-solid-900.ttf#Font Awesome 5 Pro"
            });

			FontIcon.RegisterIconFont(new FontIcon.FontInfo('\uf000', '\uffff', IconSet.Brands)
			{
                FileName = "fa-brands-400.ttf",
                PostScriptName = "FontAwesome5Brands-Regular",
                FullName = "Font Awesome 5 Brands Regular",
                Windows = "Fonts/fa-brands-400.ttf#Font Awesome 5 Brands"
            });

			Init();
            Run(LoadAsync);
		}

        async Task LoadAsync()
        {
            await ServiceNodeManager.Current.LoadServiceNodes();

#if CLI
            return;
#endif

            await Task.Delay(TimeSpan.FromSeconds(2));

            if (UIAppUsesPushNotifications)
            {
                await SyncPushToken(false);
                await SyncPushChannelSubscriptions();
            }

            if (SendErrorReports)
                await UploadErrorReports(ServiceNodeManager.Current.FirstDefaultServiceNode);
        }

        internal void RestoreUIAppSettings(ChunkReader reader)
        {
            reader.Read(nameof(AutoRemoveNotifications), ref AutoRemoveNotifications);
            reader.Read(nameof(PushNotificationsEnabled), ref PushNotificationsEnabled);
            reader.Read(nameof(SendErrorReports), ref SendErrorReports);
            reader.Read("PushToken", ref _uploadedPushToken);
            reader.Read("Subscriptions", (unpacker) =>
            {
                var channels = unpacker.UnpackList<Chain.Index>((u) => new Chain.Index(u));
                foreach (var channel in channels)
                    _subscribedChannels.Add(channel);
            });
            reader.Read("PushLastChannelUpdate", ref _lastChannelUpdate);
        }

        internal void StoreUIAppSettings(ChunkWriter writer)
        {
            writer.Write(nameof(AutoRemoveNotifications), AutoRemoveNotifications);
            writer.Write(nameof(PushNotificationsEnabled), PushNotificationsEnabled);
            writer.Write(nameof(SendErrorReports), SendErrorReports);
            writer.Write("PushToken", _uploadedPushToken);
            writer.Write("Subscriptions", (packer) =>
            {
                var channels = new List<Chain.Index>(_subscribedChannels);
                packer.Pack(channels);
            });
            writer.Write("PushLastChannelUpdate", _lastChannelUpdate);
        }

        Task ThemeChanged(ThemeChangedEvent e)
        {
            (MainPage as IThemeable)?.ThemeChanged();

            return Task.CompletedTask;
        }

        bool _finishedLoading;
        readonly List<SchemeAction> _startupActions = new List<SchemeAction>();

        public async Task SetFinishedLoading()
        {
            _finishedLoading = true;
            foreach (var schemeAction in _startupActions)
                await schemeAction.Run();
            _startupActions.Clear();
        }

        async Task SchemeRequest(SchemeRequestEvent schemeRequest)
        {
            var schemeAction = SchemeAction.ParseSchemeAction(schemeRequest.Uri);
            if (schemeAction != null)
            {
                if (!_finishedLoading)
                    _startupActions.Add(schemeAction);
                else
                    await schemeAction.Run();
            }
        }

        public static void Run(Action action)
        {
#if CLI
            action?.Invoke();
            return;
#endif

            Device.BeginInvokeOnMainThread(() =>
            {
                try
                {
                    action?.Invoke();
                }
                catch (Exception ex)
                {
                    Log.IgnoreException(ex);
                }
            });
        }

        /*
        public static void Run(Task task)
        {
            if (task != null)
                Run(() => task);
        }

        public static void Run<T>(Task<T> task)
        {
            if (task != null)
                Run(() => task);
        }
        */

        public static void Run(Func<Task> task)
        {
#if CLI
            if (task != null)
                task.Invoke().Wait();
            return;
#endif

#pragma warning disable RECS0165 // Asynchronous methods should return a Task instead of void
            Device.BeginInvokeOnMainThread(async () =>
#pragma warning restore RECS0165 // Asynchronous methods should return a Task instead of void
            {
                try
                {
                    if(task != null)
                        await task.Invoke();
                }
                catch (Exception ex)
                {
                    Log.IgnoreException(ex);
                }
            });
        }

        public static void Run<T>(Func<Task<T>> task)
        {
#if CLI
            if (task != null)
                task.Invoke().Wait();
            return;
#endif

#pragma warning disable RECS0165 // Asynchronous methods should return a Task instead of void
            Device.BeginInvokeOnMainThread(async () =>
#pragma warning restore RECS0165 // Asynchronous methods should return a Task instead of void
            {
                try
                {
                    if (task != null)
                        await task.Invoke();
                }
                catch (Exception ex)
                {
                    Log.IgnoreException(ex);
                }
            });
        }

        public static void OpenUrl(Uri uri)
		{
#pragma warning disable CS0618 // Type or member is obsolete
            Device.OpenUri(uri);
#pragma warning restore CS0618 // Type or member is obsolete
        }

        protected override void OnStart()
		{
#if DEBUG
            if (TestPage.ShowTestPage)
			{
                Run(async () =>
				{
					await PushModal(new TestPage(), true, true);
				});
			}
#endif
            RemoveNotifications();
            Start();
		}

        protected override void OnResume()
        {
            Resume();
            RemoveNotifications();
            PubSub.Publish(new ResumeEvent());
        }

        protected override void OnSleep()
        {
            Sleep();
            PubSub.Publish(new PauseEvent());
        }

		internal static bool PublishSchemeRequest(string url)
		{
			try
			{
                string escaped;
                while ((escaped = Uri.UnescapeDataString(url)) != url)
                    url = escaped;

				var uri = new Uri(url);
                Run(async () =>
				{
					await PubSub.PublishAsync(new SchemeRequestEvent(uri));
				});
			}
			catch (Exception ex)
			{
                Log.IgnoreException(ex);
			}

			return true;
		}
	}
}
