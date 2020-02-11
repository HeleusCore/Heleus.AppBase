using Heleus.Apps.Shared;
using Heleus.Apps.Shared.UWP.Renderers;
using Heleus.Base;
using SkiaSharp.Views.UWP;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Windows.ApplicationModel.DataTransfer;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media;

namespace Heleus.Apps.Shared.UWP
{
    public sealed partial class MainPage
    {
        public MainPage()
        {
            UIApp.PubSub.Subscribe<ThemeChangedEvent>(this, ThemeChanged);
            ThemeChanged(null);

            InitializeComponent();

            LoadApplication(new UIApp());

            var dataTransferManager = DataTransferManager.GetForCurrentView();
            dataTransferManager.DataRequested += ShareTextHandler;

            Loaded += MainPage_Loaded;
        }

        private void MainPage_Loaded(object sender, Windows.UI.Xaml.RoutedEventArgs e)
        {
            ThemeChanged(null);

        }

        SKSwapChainPanel skiaBackground;

        Task ThemeChanged(ThemeChangedEvent e)
        {
            if (Theme.WindowTheme == WindowTheme.Dark)
                RequestedTheme = Windows.UI.Xaml.ElementTheme.Dark;
            else
                RequestedTheme = Windows.UI.Xaml.ElementTheme.Light;

            try
            {
                if (UIApp.Current == null)
                    return Task.CompletedTask;

                var renderer = Xamarin.Forms.Platform.UWP.Platform.GetRenderer(UIApp.Current.MainPage);
                var container = renderer.ContainerElement;
                var canvas = container.Parent as Canvas;

                var pane = container.FindControlByName<Grid>("PaneRoot");
                var content = container.FindControlByName<Grid>("ContentRoot");
                var commandArea = container.FindControlByName<Border>("TopCommandBarArea");
                var commandBar = container.FindFirstControl<CommandBar>();
                var title = container.FindControlByName<Border>("TitleArea");
                var toggle = container.FindControlByName<Button>("PaneTogglePane");
                var presenter = container.FindControlByName<ContentPresenter>("MasterPresenter");

                if (pane == null || content == null)
                    return Task.CompletedTask;

                Brush brush = null;
                if (!WindowsApp.HasAcrylicSupport)
                    brush = Theme.SecondaryColor.Color.ToBrush();

                commandArea.Background = brush;
                commandBar.Background = brush;

                (title.Child as Grid).Background = brush;
                (toggle.Parent as StackPanel).Background = brush;
                toggle.Background = brush;

                if(brush == null)
                    (presenter.Content as Panel).Background = brush;

                //if(!WindowsApp.IsPhoneApp)
                    content.Margin = (pane.Children[0] as Border).Margin = new Windows.UI.Xaml.Thickness(0, 32, 0, 0);

                canvas.Background = brush;

                if (WindowsApp.HasAcrylicSupport)
                {
                    skiaBackground = new  SKSwapChainPanel();
                    skiaBackground.PaintSurface += PaintSurfaceFast;
                    canvas.SizeChanged += Canvas_SizeChanged;

                    skiaBackground.Width = canvas.ActualWidth;
                    skiaBackground.Height = canvas.ActualHeight;

                    canvas.Children.Insert(0, skiaBackground);

                    var myBrush = new AcrylicBrush
                    {
                        BackgroundSource = AcrylicBackgroundSource.Backdrop
                    };

                    var c = Windows.UI.Color.FromArgb(255, 0, 0, 0);
                    if (Theme.WindowTheme == WindowTheme.Light)
                        c = Windows.UI.Color.FromArgb(255, 255, 255, 255);

                    myBrush.TintColor = c;
                    myBrush.FallbackColor = c;
                    myBrush.TintOpacity = 0.5;

                    var grid = container.FindFirstControl<Grid>();

                    grid.Background = myBrush;
                    pane.Background = myBrush;
                }
            }
            catch (Exception ex)
            {
                Log.IgnoreException(ex);
            }

            return Task.CompletedTask;
        }

        private void PaintSurfaceFast(object sender, SKPaintGLSurfaceEventArgs e)
        {
            UIApp.UpdateBackgroundCanvas(e.Surface.Canvas, e.BackendRenderTarget.Width, e.BackendRenderTarget.Height);
        }

        private void Canvas_SizeChanged(object sender, Windows.UI.Xaml.SizeChangedEventArgs e)
        {
            var canvas = sender as Canvas;
            if(skiaBackground.Width != canvas.ActualWidth || skiaBackground.Height != canvas.ActualHeight)
            {
                skiaBackground.Width = canvas.ActualWidth;
                skiaBackground.Height = canvas.ActualHeight;
                skiaBackground.Invalidate();
            }
        }

        private void PaintSurface(object sender, SKPaintSurfaceEventArgs e)
        {
            UIApp.UpdateBackgroundCanvas(e.Surface.Canvas, e.Info.Width, e.Info.Height);
        }

        private void ShareTextHandler(DataTransferManager sender, DataRequestedEventArgs e)
        {
            var text = UIApp.ShareText;
            if (!string.IsNullOrEmpty(text))
            {
                var request = e.Request;
                request.Data.Properties.Title = Tr.Get("Common.AppName");
                request.Data.SetText(text);
            }
        }
    }
}
