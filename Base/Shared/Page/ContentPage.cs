using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Xamarin.Forms;

namespace Heleus.Apps.Shared
{
    public class LoadingFrame : Frame
    {
        readonly ExtActivityIndicator _indicator = new ExtActivityIndicator();

        public LoadingFrame()
        {
            Padding = Margin = new Thickness(5);
            BackgroundColor = Theme.PrimaryColor.Color;

            if (UIApp.IsMacOS)
                _indicator.WidthRequest = _indicator.HeightRequest = 24;

            if (!UIApp.IsMacOS)
                _indicator.Color = Theme.TextColor.Color;

            _indicator.IsRunning = true;
            Content = _indicator;

            AbsoluteLayout.SetLayoutFlags(this, AbsoluteLayoutFlags.XProportional);
            AbsoluteLayout.SetLayoutBounds(this, new Rectangle(1, UIApp.IsIOS ? 90 : 0, AbsoluteLayout.AutoSize, AbsoluteLayout.AutoSize));
        }
    }

    public class InfoFrame : Frame
    {
        public readonly ExtLabel Label;
        readonly AbsoluteLayout _layout;

        public InfoFrame(AbsoluteLayout layout, string text)
        {
            Padding = Margin = new Thickness(5);
            BackgroundColor = Theme.PrimaryColor.Color;

            _layout = layout;
            Label = new ExtLabel
            {
                Text = text,
                FontStyle = Theme.RowTitleFont,
                ColorStyle = Theme.TextColor
            };

            Content = Label;

            AbsoluteLayout.SetLayoutFlags(this, AbsoluteLayoutFlags.PositionProportional);
            AbsoluteLayout.SetLayoutBounds(this, new Rectangle(0.5, 0.5, AbsoluteLayout.AutoSize, AbsoluteLayout.AutoSize));

            layout.Children.Add(this);
        }

        public bool Attached => _layout.Children.Contains(this);

        public void Attach()
        {
            if (!Attached)
                _layout.Children.Add(this);
        }

        public void Detach()
        {
            _layout.Children.Remove(this);
        }
    }

    public abstract class ExtContentPage : ContentPage, IThemeable
    {
        public new string Title { get; private set; }

        public readonly string PageName;
        public readonly AbsoluteLayout RootLayout;

        protected VisualElement FocusElement;

        readonly LoadingFrame _loadingFrame = new LoadingFrame();

        bool _firstAppearing = true;
        readonly List<Type> _subscriptions = new List<Type>();

        public void SetTitle(string title)
        {
            if (!UIApp.IsMacOS || this is MenuPage)
                base.Title = title;
        }

        protected ExtContentPage(string name)
        {
            if (string.IsNullOrWhiteSpace(name))
                name = GetType().Name;

            PageName = name;
            SetTitle(T(name + ".Title"));

            RootLayout = new AbsoluteLayout
            {
                InputTransparent = UIApp.IsMacOS
            };

            //CompressedLayout.SetIsHeadless(RootLayout, true);

            Content = RootLayout;

#pragma warning disable RECS0021 // Warns about calls to virtual member functions occuring in the constructor
            UpdateColors();
#pragma warning restore RECS0021 // Warns about calls to virtual member functions occuring in the constructor

            if (UIApp.IsGTK)
                NavigationPage.SetHasNavigationBar(this, false);


            UIApp.NewContentPage(this);
        }

        protected ExtToolbarItem GetToolbarItem(string identifier)
        {
            foreach (var item in ToolbarItems)
            {
                if (item is ExtToolbarItem exItem && exItem.Identifier == identifier)
                {
                    return exItem;
                }
            }
            return null;
        }

        public void Subscribe<T>(Func<T, Task> function)
        {
            _subscriptions.Add(typeof(T));
            UIApp.PubSub.Subscribe(this, function);
        }

        protected void SetRootContent(View view)
        {
            if (!RootLayout.Children.Contains(view))
            {
                AbsoluteLayout.SetLayoutFlags(view, AbsoluteLayoutFlags.All);
                AbsoluteLayout.SetLayoutBounds(view, new Rectangle(0.5, 0.5, 1, 1));
                RootLayout.Children.Add(view);
            }
        }

        public virtual void OnOpen()
        {

        }

        public virtual void ThemeChanged()
        {
            UpdateColors();
            RootLayout.PropagateThemeChange();
        }

        protected virtual void UpdateColors()
        {
            if (UIApp.IsAndroid || UIApp.IsUWP)
                BackgroundColor = Color.Transparent;
            else
                BackgroundColor = Theme.PrimaryColor.Color;
        }

        protected override void OnAppearing()
        {
            base.OnAppearing();

            if (_firstAppearing)
            {
                _firstAppearing = false;

                Init();

                UIApp.Run(async () =>
                {
                    await InitAsync();

                    if (FocusElement != null)
                    {
                        await Task.Delay(500);
                        var e = FocusElement;
                        FocusElement = null;
                        if (e != null)
                            e.Focus();
                    }
                });
            }
        }

        public virtual Task InitAsync()
        {
            return Task.CompletedTask;
        }

        public virtual void Init()
        {

        }

        public virtual void OnPopped()
        {
            UIApp.ShowLoadingIndicator = false;
            foreach (var sub in _subscriptions)
                UIApp.PubSub.Unsubscribe(this, sub);
        }

        protected bool Loading
        {
            set
            {
                if (value)
                {
                    RootLayout.Children.Add(_loadingFrame);
                    UIApp.ShowLoadingIndicator = true;

                }
                else
                {
                    RootLayout.Children.Remove(_loadingFrame);
                    UIApp.ShowLoadingIndicator = false;
                }
            }
        }

        public string T(string path, params object[] parameters)
        {
            var text = Tr.Get(PageName + "." + path, out bool success, parameters);
            if (!success)
                text = Tr.Get(path, parameters);

            return text;
        }

        public T PreviousPage<T>() where T : ExtContentPage
        {
            if (Navigation.NavigationStack.Count < 2)
                return null;

            Page previous = Navigation.NavigationStack[Navigation.NavigationStack.Count - 2];
            return previous as T;
        }

        public ExtContentPage PreviousPage()
        {
            return PreviousPage<ExtContentPage>();
        }

        public async Task PopAsync(int count = 1)
        {
            var pages = new List<ExtContentPage>
            {
                this
            };

            var page = this;
            for (var i = 2; i <= count; i++)
            {
                page = page.PreviousPage();
                if (page == null)
                    break;

                pages.Add(page);
            }

            foreach (var p in pages)
            {
                await p.Navigation.PopAsync();
            }
        }

        public InfoFrame InfoFrame(string name)
        {
            bool success;
            string text;

            if (name.StartsWith(".", StringComparison.Ordinal))
                text = Tr.Get(name.Substring(1), out success);
            else
                text = Tr.Get(PageName + "." + name, out success);

            if (!success)
                text = Tr.Get("Messages." + name);

            return new InfoFrame(RootLayout, text);
        }

        public InfoFrame InfoFrameText(string text)
        {
            return new InfoFrame(RootLayout, text);
        }

        public void Toast(string name)
        {
            bool success = false;

            string text;
            if (name.StartsWith(".", StringComparison.Ordinal))
                text = Tr.Get(name.Substring(1), out success);
            else
                text = Tr.Get(PageName + "." + name, out success);

            if (!success)
                text = Tr.Get("Messages." + name);

            UIApp.Toast(text);
        }

        public void ToastText(string text)
        {
            UIApp.Toast(text);
        }

        public void Error(string name)
        {
            Message(name, Tr.Get("Common.Error"));
        }

        public void Message(string name, string title = null)
        {
            if (title == null)
                title = Tr.Get("App.Name");
            MessageAsync(name, title);
        }

        public Task ErrorAsync(string name)
        {
            return MessageAsync(name, Tr.Get("Common.Error"));
        }

        public Task MessageAsync(string name, string title = null)
        {
            if (title == null)
                title = Tr.Get("App.Name");

            bool success = false;

            string text;
            if (name.StartsWith(".", StringComparison.Ordinal))
                text = Tr.Get(name.Substring(1), out success);
            else
                text = Tr.Get(PageName + "." + name, out success);

            if (!success)
                text = Tr.Get("Messages." + name);

            return DisplayAlert(title, text, T("Common.DialogOk"));
        }

        public void ErrorText(string text)
        {
            Message(text, Tr.Get("Common.Error"));
        }

        public void MessageText(string text, string title = null)
        {
            if (title == null)
                title = Tr.Get("App.Name");

            DisplayAlert(title, text, T("Common.DialogOk"));
        }

        public Task ErrorTextAsync(string text)
        {
            return MessageTextAsync(text, Tr.Get("Common.Error"));
        }

        public async Task MessageTextAsync(string text, string title = null)
        {
            if (title == null)
                title = Tr.Get("App.Name");

            await DisplayAlert(title, text, T("Common.DialogOk"));
        }

        public async Task<bool> ConfirmAsync(string name, string title = null)
        {
            if (title == null)
                title = Tr.Get("App.Name");

            var success = false;
            string text;
            if (name.StartsWith(".", StringComparison.Ordinal))
                text = Tr.Get(name.Substring(1), out success);
            else
                text = Tr.Get(PageName + "." + name, out success);

            if (!success)
                text = Tr.Get("Messages." + name);

            return await DisplayAlert(title, text, T("Common.DialogYes"), T("Common.DialogNo"));
        }

        public async Task<bool> ConfirmTextAsync(string text, string title = null)
        {
            if (title == null)
                title = Tr.Get("App.Name");

            return await DisplayAlert(title, text, T("Common.DialogYes"), T("Common.DialogNo"));
        }
    }
}
