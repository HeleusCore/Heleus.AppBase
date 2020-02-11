using System;
using Xamarin.Forms;
using System.Threading.Tasks;

namespace Heleus.Apps.Shared
{
    class SettingsPageBase : StackPage
    {
#if ANDROID
        readonly Android.FirebaseNotificationSettings _firebaseSettings = new Android.FirebaseNotificationSettings();

        async Task LedColor(ButtonRow button)
        {
            await Navigation.PushAsync(new ColorPickerPage(_firebaseSettings.LedColor, false, null, (newColor) =>
            {
                _firebaseSettings.LedColor = newColor;
            }));
        }

        Task NotficiationSoundPicked(AndroidNotificationSoundPickedEvent evt)
        {
            if (!string.IsNullOrEmpty(evt.Uri))
            {
                _firebaseSettings.SoundUri = evt.Uri;
            }

            return Task.CompletedTask;
        }
#endif

        async Task ThemeColors(ButtonRow button)
        {
            await Navigation.PushAsync(new ThemePage());
        }

        Task NotificationSound(ButtonRow button)
        {
            //if (App.IsIOS)
            //await NotificationSoundiOSPage.Open();

#if ANDROID
            var intent = new global::Android.Content.Intent(global::Android.Media.RingtoneManager.ActionRingtonePicker);
            intent.PutExtra(global::Android.Media.RingtoneManager.ExtraRingtoneType, (int)(global::Android.Media.RingtoneType.Notification | global::Android.Media.RingtoneType.Alarm));
            intent.PutExtra(global::Android.Media.RingtoneManager.ExtraRingtoneShowSilent, false);
            intent.PutExtra(global::Android.Media.RingtoneManager.ExtraRingtoneShowDefault, true);
            intent.PutExtra(global::Android.Media.RingtoneManager.ExtraRingtoneDefaultUri, global::Android.Provider.Settings.System.DefaultNotificationUri);

            var uri = _firebaseSettings.SoundUri;
            if (!string.IsNullOrEmpty(uri))
            {
                intent.PutExtra(global::Android.Media.RingtoneManager.ExtraRingtoneExistingUri, global::Android.Net.Uri.Parse(uri));
            }

            Android.MainActivity.Current.StartActivityForResult(intent, Android.MainActivity.NotificationSoundPickerResultId);
#endif

            return Task.CompletedTask;
        }


        protected bool IsToggled(string name, bool def)
        {
            var row = GetRow<SwitchRow>(name);
            if (row == null)
                return def;

            return row.Switch.IsToggled;
        }

        protected void AddThemeSection()
        {
            AddHeaderRow("ThemeSection");
            var swtch = AddSwitchRow("ThemeEnable");
            swtch.Switch.IsToggled = Theme.ThemeMode == ThemeMode.Custom;
            swtch.Switch.Toggled = (sw) =>
            {
                UIApp.Run(() => Theme.SwitchTheme(sw.IsToggled ? ThemeMode.Custom : ThemeMode.Default));
            };
            AddButtonRow("ThemeColors", ThemeColors, true);
            AddFooterRow();
        }

        protected void AddAppInfoSection(AppInfoType? ignore = null)
        {
            AddHeaderRow("MoreHeleusApps");

            foreach (AppInfoType appInfoType in Enum.GetValues(typeof(AppInfoType)))
            {
                if (ignore.HasValue && ignore.Value == appInfoType)
                    continue;

                var appInfo = AppInfo.GetAppInfo(appInfoType);
                if(appInfo != null)
                {
                    var button = AddLinkRow(null, appInfo.Link.AbsoluteUri);
                    button.SetMultilineText(appInfo.Name, appInfo.Description);
                    button.SetDetailView(new ExtImage { Source = appInfo.ImageSource });
                }
            }

            AddFooterRow();
        }

        protected virtual void AddPushNotificationSectionExtras()
        {

        }

        protected void AddPushNotificationSection()
        {
            AddHeaderRow("PushNotificationSection");

            AddPushNotificationSectionExtras();

            if (!UIApp.Current.PushNotificationsEnabled)
                AddButtonRow("PNEnable", EnablePushNotifications);
            else
                AddButtonRow("PNCheck", CheckPushNotifications);

#if ANDROID
            Subscribe<AndroidNotificationSoundPickedEvent>(NotficiationSoundPicked);
            _firebaseSettings.Load(Android.MainActivity.Current);

            var sw = AddSwitchRow("PNPlaySound");
            sw.Switch.IsToggled = _firebaseSettings.PlaySound;

            AddButtonRow("PNSound", NotificationSound);

            sw = AddSwitchRow("PNVibrate");
            sw.Switch.IsToggled = _firebaseSettings.Vibrate;

            sw = AddSwitchRow("PNUseLED");
            sw.Switch.IsToggled = _firebaseSettings.UseLed;

            AddButtonRow("PNColor", LedColor);

#endif

            var swtch = AddSwitchRow("PNAutoRemove");
            swtch.Switch.IsToggled = UIApp.Current.AutoRemoveNotifications;

#if WP
            var settings = Windows.Storage.ApplicationData.Current.LocalSettings.Values;
            bool value = false;
            object data = null;

            if (settings.TryGetValue("AutoRemoveNotifications", out data))
            {
                value = (bool)data;
            }
            swtch.Switch.IsToggled = value;

            value = false;
            if(settings.TryGetValue("DisableToast", out data))
            {
                value = (bool)data;
            }

            swtch = AddSwitchRow("DisableToast");
            swtch.Switch.IsToggled = value;

#endif

#if WP
            swtch.Switch.IsToggled = value;

            value = false;
            if(settings.TryGetValue("ToastNoLogo", out data))
            {
                value = (bool)data;
            }

            swtch = AddSwitchRow("ToastNoLogo");
            swtch.Switch.IsToggled = value;
#endif

            AddFooterRow();
        }

        async Task CheckPushNotifications(ButtonRow arg)
        {
            await Navigation.PushAsync(new PushNotificationPage());
        }

        async Task EnablePushNotifications(ButtonRow arg)
        {
            if (!UIApp.Current.PushNotificationsEnabled)
            {
                if (!await ConfirmAsync("ConfirmEnableNotifications"))
                {
                    return;
                }

                await Task.Delay(1000);

                UIApp.Current.PushNotificationsEnabled = true;
                UIApp.Current.SaveSettings();
            }

            await Navigation.PushAsync(new PushNotificationPage());
        }

        public SettingsPageBase() : base("SettingsPage")
        {
        }

        public override void OnPopped()
        {
            base.OnPopped();
#if ANDROID
            try
            {

                _firebaseSettings.PlaySound = IsToggled("PNPlaySound", _firebaseSettings.PlaySound);
                _firebaseSettings.Vibrate = IsToggled("PNVibrate", _firebaseSettings.Vibrate);
                _firebaseSettings.UseLed = IsToggled("PNUseLED", _firebaseSettings.UseLed);

                _firebaseSettings.Save(Android.MainActivity.Current);
            }
            catch
            {
            }
#endif

            UIApp.Current.AutoRemoveNotifications = IsToggled("PNAutoRemove", UIApp.Current.AutoRemoveNotifications);

#if WP
            var settings = Windows.Storage.ApplicationData.Current.LocalSettings.Values;
            settings["AutoRemoveNotifications"] = IsToggled ("PNAutoRemove", false);
            settings["DisableToast"] = IsToggled ("DisableToast", false);
            settings["ToastNoLogo"] = IsToggled ("ToastNoLogo", false);
#endif

            UIApp.Current.SaveSettings();
        }
    }
}

