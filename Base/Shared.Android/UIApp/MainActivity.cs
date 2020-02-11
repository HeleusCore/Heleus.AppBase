using System;
using System.Linq;
using System.Threading.Tasks;
using Android.App;
using Android.Content;
using Android.Gms.Common;
using Android.OS;
using Android.Views;
using SkiaSharp.Views.Android;
using Xamarin.Forms.Platform.Android;

namespace Heleus.Apps.Shared.Android
{
	public partial class MainActivity : FormsAppCompatActivity, global::Android.Gms.Tasks.IOnSuccessListener
    {
		public static MainActivity Current
		{
			get;
			private set;
		}

		public static bool Active
		{
			get;
			private set;
		}

		public const int NotificationSoundPickerResultId = 1336;
		public const int ImagePickerResultId = 1337;
        public const int FilePickerResultId = 1338;
        public const int SaveFilePickerResultId = 1339;

        ViewGroup _backgroundLayout;
        SKCanvasView _backgroundCanvas;

		ViewGroup RootViewGroup
		{
			get
			{
				return (ViewGroup)FindViewById(global::Android.Resource.Id.Content).RootView;
			}
		}

		void CanvasView_PaintSurface(object sender, SKPaintSurfaceEventArgs e)
		{
			var info = e.Info;
			var surface = e.Surface;

			UIApp.UpdateBackgroundCanvas(surface.Canvas, info.Width, info.Height);
		}

        Task ThemeChanged(ThemeChangedEvent e)
        {
            try
            {
                _backgroundCanvas?.Invalidate();
            }
            catch { }

            return Task.CompletedTask;
        }

        protected override void OnCreate(Bundle savedInstanceState)
        {
            Current = this;
            NativeSettings.Init(this);
            AndroidStorage.Init(this);

            global::Heleus.Base.Log.LogLevel = global::Heleus.Base.LogLevels.Trace;

            TabLayoutResource = global::Heleus.Apps.Shared.Android.Resource.Layout.Tabbar;
            ToolbarResource = global::Heleus.Apps.Shared.Android.Resource.Layout.Toolbar;


            UIApp.PubSub.Clear();

            UIApp.PubSub.Subscribe<ThemeChangedEvent>(this, ThemeChanged);

            Window.AddFlags(WindowManagerFlags.TranslucentStatus);
			//Window.AddFlags(WindowManagerFlags.TranslucentNavigation);

			base.SetTheme(global::Heleus.Apps.Shared.Android.Resource.Style.MyTheme);

            base.OnCreate(savedInstanceState);

            var root = RootViewGroup;

			//var baseLayout = ViewChildren.GetChild<global::Android.Widget.RelativeLayout>(root).FirstOrDefault();
			//baseLayout?.SetFitsSystemWindows(true);

			_backgroundLayout = ViewChildren.GetChild<global::Android.Support.V7.Widget.ContentFrameLayout>(root).FirstOrDefault();
			if(_backgroundLayout != null)
			{
				_backgroundCanvas = new SKCanvasView(this);
				_backgroundCanvas.PaintSurface += CanvasView_PaintSurface;
				_backgroundLayout.AddView(_backgroundCanvas, 0);
			}

            Xamarin.Forms.Forms.SetFlags("FastRenderers_Experimental");
			Xamarin.Forms.Forms.Init(this, savedInstanceState);

            LoadApplication(new UIApp());

            //ViewChildren.IterateViewGroupChildren(RootViewGroup);

            //CheckNewIntent(Intent);
            CheckNotificationIntend(Intent);

            InitFirebase();
        }

		public override void OnContentChanged()
		{
			base.OnContentChanged();
		}

        protected override void OnResume()
        {
            base.OnResume();
            Active = true;
        }

        protected override void OnPause()
        {
            Active = false;
            base.OnPause();

            GC.Collect();
        }

        protected override void OnDestroy()
        {
            Active = false;

            try
            {
				if(_backgroundCanvas != null)
				{
					if(_backgroundLayout != null)
					{
						_backgroundLayout.RemoveView(_backgroundCanvas);
					}

					_backgroundCanvas.PaintSurface -= CanvasView_PaintSurface;
					try {
						_backgroundCanvas.Dispose();
					}
					catch {
						
					}
				}
				_backgroundCanvas = null;

                base.OnDestroy();
            }
            catch (Exception ex)
            {
                global::Heleus.Base.Log.IgnoreException(ex);
            }
        }

        protected override void OnActivityResult(int requestCode, Result resultCode, Intent data)
        {
			if (requestCode == ImagePickerResultId)
			{
				if ((resultCode == Result.Ok) && (data != null))
				{
                    UIApp.Run(() => UIApp.HandleImagePicker(data.Data));
				}
			}
            else if(requestCode == FilePickerResultId)
            {
                UIApp.HandleFilePicker(data);
            }
            else if (requestCode == SaveFilePickerResultId)
            {
                if(resultCode == Result.Ok && data != null)
                {
                    var uri = data.Data;
                    if (uri != null)
                        UIApp.HandleSaveFilePicker(uri);
                }
            }
            else if (requestCode == NotificationSoundPickerResultId)
            {
                if (resultCode == Result.Ok && data != null)
                {
                    try
                    {
						var uri = data.GetParcelableExtra(global::Android.Media.RingtoneManager.ExtraRingtonePickedUri).ToString();
                        UIApp.Run(() => UIApp.PubSub.PublishAsync(new AndroidNotificationSoundPickedEvent(uri)));
                    }
                    catch { }
                }
            }
            else
            {
                base.OnActivityResult(requestCode, resultCode, data);
                HandleActivityResult(requestCode, resultCode, data);
            }
        }

        protected override void OnNewIntent(Intent intent)
        {
            CheckNotificationIntend(intent);
            base.OnNewIntent(intent);
        }

        public void EnableRemoteNotifications()
        {
            if (IsPlayServicesAvailable())
                Firebase.Iid.FirebaseInstanceId.Instance.GetInstanceId().AddOnSuccessListener(this);
        }

        public void OnSuccess(Java.Lang.Object result)
        {
            var token = result.Class.GetMethod("getToken").Invoke(result).ToString();
            UIApp.Current?.RemoteNotifiactionTokenResult(token);
        }

        bool IsPlayServicesAvailable()
        {
            try
            {
                int resultCode = GoogleApiAvailability.Instance.IsGooglePlayServicesAvailable(this);
                if (resultCode != ConnectionResult.Success)
                {
                    if (GoogleApiAvailability.Instance.IsUserResolvableError(resultCode))
                        global::Heleus.Base.Log.Write(GoogleApiAvailability.Instance.GetErrorString(resultCode));
                    else
                    {
                        global::Heleus.Base.Log.Write("Sorry, this device is not supported");
                    }

                    GoogleApiAvailability.Instance.MakeGooglePlayServicesAvailable(Current);
                    return false;
                }
                else
                {
                    global::Heleus.Base.Log.Write("Google Play Services is available.");
                    return true;
                }
            }
            catch (Exception ex)
            {
                global::Heleus.Base.Log.IgnoreException(ex);
            }
            return false;
        }

        void CheckNotificationIntend(Intent intent)
        {
            try
            {
                if (intent.Action == LocalFirebaseMessagingService.NotificationOpenSingleAction)
                {
                    try
                    {
                        UIApp.Run(async () =>
                        {
                            try
							{
                                var scheme = intent.GetStringExtra("scheme");
                                if(scheme != null)
                                {
                                    await Task.Delay(250);
                                    var uri = new Uri(scheme);
                                    await UIApp.PubSub.PublishAsync(new PushNotificationEvent(uri, PushNotificationEventType.UserInteraction));
                                }
                            }
							catch (Exception ex)
							{
                                Base.Log.IgnoreException(ex);
							}
						});
					}
                    catch (Exception ex)
                    {
                        global::Heleus.Base.Log.IgnoreException(ex);
                    }
                }
                else if (intent.Action == Intent.ActionView)
                {
                    try
                    {
                        var data = intent?.Data;
                        if (data != null)
                        {
                            UIApp.Run(async () =>
                            {
                                await Task.Delay(250);
                                UIApp.PublishSchemeRequest(data.ToString());
                            });
                        }
                    }
                    catch (Exception ex)
                    {
                        global::Heleus.Base.Log.IgnoreException(ex);
                    }
                }
            }
            catch (Exception ex)
            {
                global::Heleus.Base.Log.IgnoreException(ex);
            }
        }
    }
}
