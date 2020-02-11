using Heleus.Apps.Shared.UWP.Renderers;
using Heleus.Base;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Threading.Tasks;
using Windows.ApplicationModel.Activation;
using Windows.ApplicationModel.Core;
using Windows.Foundation;
using Windows.Globalization;
using Windows.UI.ViewManagement;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

namespace Heleus.Apps.Shared.UWP
{
    sealed partial class WindowsApp
    {
        public static readonly bool HasAcrylicSupport = Windows.Foundation.Metadata.ApiInformation.IsTypePresent("Windows.UI.Xaml.Media.AcrylicBrush");
        public static readonly bool IsDesktopApp = Windows.Foundation.Metadata.ApiInformation.IsTypePresent("Windows.UI.ViewManagement.ApplicationView");
        public static readonly bool IsPhoneApp = Windows.Foundation.Metadata.ApiInformation.IsTypePresent("Windows.UI.ViewManagement.StatusBar");

        public WindowsApp()
        {
            UIApp.PubSub.Subscribe<ThemeChangedEvent>(this, ThemeChanged);
            Log.ShowSystemDiagnostics = true;
#if DEBUG
            Log.LogLevel = LogLevels.Trace;
#endif
            try
            {
                ApplicationView.PreferredLaunchWindowingMode = ApplicationViewWindowingMode.PreferredLaunchViewSize;
                ApplicationView.PreferredLaunchViewSize = new Size(Theme.WindowWidth, Theme.WindowHeight);
            }
            catch
            {
            }

            if (Theme.WindowTheme == WindowTheme.Dark)
                RequestedTheme = ApplicationTheme.Dark;
            else
                RequestedTheme = ApplicationTheme.Light;

            InitializeComponent();

            var language = "en";
            try
            {
                var region = new GeographicRegion();
                language = region.CodeTwoLetter.ToLower();
            }
            catch
            {
            }

            Tr.Initalize(language);
        }

        Task ThemeChanged(ThemeChangedEvent e)
        {
            try
            {
                var view = ApplicationView.GetForCurrentView();
                var titleBar = view.TitleBar;

                var color = Xamarin.Forms.Color.White;
                if (Theme.WindowTheme == WindowTheme.Light)
                    color = Xamarin.Forms.Color.Black;

                titleBar.ButtonBackgroundColor = Windows.UI.Colors.Transparent;
                titleBar.ButtonForegroundColor = color.ToWindowsColor();

                titleBar.ButtonHoverBackgroundColor = Theme.SecondaryColor.Color.ToWindowsColor();
                titleBar.ButtonHoverForegroundColor = color.ToWindowsColor();

                titleBar.ButtonInactiveBackgroundColor = Windows.UI.Colors.Transparent;
                titleBar.ButtonInactiveForegroundColor = Xamarin.Forms.Color.Gray.ToWindowsColor();


                titleBar.ButtonPressedBackgroundColor = Theme.PrimaryColor.Color.ToWindowsColor();
                titleBar.ButtonPressedForegroundColor = color.ToWindowsColor();

                CoreApplication.GetCurrentView().TitleBar.ExtendViewIntoTitleBar = true;
            }
            catch { }

            /*
            if (IsPhoneApp)
            {
                try
                {
                    var statusBar = Windows.UI.ViewManagement.StatusBar.GetForCurrentView();
                    statusBar.BackgroundColor = Theme.SecondaryColor.Color.ToWindowsColor();
                    if (Theme.WindowTheme == WindowTheme.Light)
                        statusBar.ForegroundColor = Xamarin.Forms.Color.Black.ToWindowsColor();
                    else
                        statusBar.ForegroundColor = Xamarin.Forms.Color.White.ToWindowsColor();
                    statusBar.BackgroundOpacity = 1;

                    statusBar.ProgressIndicator.Text = Tr.Get("Common.AppName");
                    statusBar.ProgressIndicator.ProgressValue = 0;// null;
                    await statusBar.ProgressIndicator.ShowAsync();
                }
                catch { }
            }
            */
            
            return Task.CompletedTask;
        }

        private void WindowSizeChanged(object sender, Windows.UI.Core.WindowSizeChangedEventArgs e)
        {
            var view = ApplicationView.GetForCurrentView();
            {
                Theme.WindowWidth = (int)e.Size.Width;
                Theme.WindowHeight = (int)e.Size.Height;
                Theme.Fullscreen = false;
                Theme.Save();
            }
        }

        protected override void OnLaunched(LaunchActivatedEventArgs e)
        {
            StartApp(e);
        }

        protected override void OnActivated(IActivatedEventArgs e)
        {
            StartApp(e);
        }

        void StartApp(IActivatedEventArgs e)
        {
            if (!(Window.Current.Content is Frame rootFrame))
            {
                //if (System.Diagnostics.Debugger.IsAttached)
                //  DebugSettings.EnableFrameRateCounter = true;

                _ = ThemeChanged(null);
                /*
                if (Windows.Foundation.Metadata.ApiInformation.IsTypePresent("Windows.UI.ViewManagement.StatusBar"))
                {
                    try
                    {
                        var statusBar = Windows.UI.ViewManagement.StatusBar.GetForCurrentView();
                        statusBar.BackgroundOpacity = 1;

                        statusBar.ProgressIndicator.Text = Tr.Get("Common.AppName");
#pragma warning disable 4014
                        statusBar.ProgressIndicator.ShowAsync();
#pragma warning restore 4014
                        statusBar.ProgressIndicator.ProgressValue = 0;// null;

                        //var applicationView = Windows.UI.ViewManagement.ApplicationView.GetForCurrentView();
                        //applicationView.SetDesiredBoundsMode(Windows.UI.ViewManagement.ApplicationViewBoundsMode.UseCoreWindow);
                    }
                    catch
                    {
                    }
                }
                else */
                if (Windows.Foundation.Metadata.ApiInformation.IsTypePresent("Windows.UI.ViewManagement.ApplicationView"))
                {
                    try
                    {
                        var view = ApplicationView.GetForCurrentView();
                        view.SetPreferredMinSize(new Size(500, 500));

                        Window.Current.SizeChanged += WindowSizeChanged;
                        view.TryResizeView(new Size(Theme.WindowWidth, Theme.WindowHeight));
                    }
                    catch { }
                }

                rootFrame = new Frame();

                List<Assembly> assembliesToInclude = new List<Assembly>();
                //assembliesToInclude.Add(typeof(MR.Gestures.BoxView).GetTypeInfo().Assembly);
                //assembliesToInclude.Add(typeof(MR.Gestures.UWP.Renderers.BoxViewRenderer).GetTypeInfo().Assembly);

                Xamarin.Forms.Forms.Init(e, assembliesToInclude);
                UnhandledException += OnUnhandledException;

                System.Threading.Tasks.TaskScheduler.UnobservedTaskException += UnobservedTaskException; ;

                Window.Current.Content = rootFrame;
                Window.Current.Activated += Activated;
            }

            if (rootFrame.Content == null)
            {
                rootFrame.Navigate(typeof(MainPage), string.Empty);
            }

            if (e.Kind == ActivationKind.Protocol)
            {
                try
                {
                    var eventArgs = e as ProtocolActivatedEventArgs;
                    UIApp.PublishSchemeRequest(eventArgs.Uri.ToString());
                }
                catch
                {
                }
            }
            else if (e.Kind == ActivationKind.ToastNotification)
            {
                var eventArgs = e as ToastNotificationActivatedEventArgs;
                UIApp.Run(async () =>
                {
                    await UIApp.PubSub.PublishAsync(new PushNotificationEvent(new Uri(eventArgs.Argument), PushNotificationEventType.UserInteraction));
                });
            }

            Window.Current.Activate();
        }

        public static bool InForeground {
            get;
            private set;
        } = false;

        private void Activated(object sender, Windows.UI.Core.WindowActivatedEventArgs e)
        {
            if (e.WindowActivationState == Windows.UI.Core.CoreWindowActivationState.Deactivated)
            {
                InForeground = false;
                //App.Current?.Deactivated();
            }
            else
            {
                InForeground = true;
                //App.Current?.Activated();
            }
        }

        private void UnobservedTaskException(object sender, System.Threading.Tasks.UnobservedTaskExceptionEventArgs e)
        {
            try
            {
                string message = e.Exception.ToString();
                System.Diagnostics.Debug.WriteLine(message);
                Log.HandleException(e.Exception);
            }
            catch (Exception ex)
            {
                Log.IgnoreException(ex);
            }
        }

        private void OnUnhandledException(object sender, Windows.UI.Xaml.UnhandledExceptionEventArgs e)
        {
            try
            {
                string message = e.Exception.ToString();
                System.Diagnostics.Debug.WriteLine(message);
                Log.HandleException(e.Exception);
            }
            catch (Exception ex)
            {
                Log.IgnoreException(ex);
            }
        }

    }
}