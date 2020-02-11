using System;
using Android.Content;
using Xamarin.Forms;

namespace Heleus.Apps.Shared.Android
{   
    class FirebaseNotificationSettings
    {
        const string PrefName = "firebasesettings";

        public bool Vibrate = false;
        public bool PlaySound = true;
        public bool UseLed = true;

        public Color LedColor = Color.FromRgb(0, 255, 0);
        public string SoundUri = string.Empty;

        static readonly object _lock = new object();

		public void Load(Context context)
        {
            lock (_lock)
            {
                try
                {
					using (var prefs = context.GetSharedPreferences(PrefName, FileCreationMode.Private))
                    {

                        Vibrate = prefs.GetBoolean("Vibrate", false);
                        PlaySound = prefs.GetBoolean("PlaySound", true);
                        UseLed = prefs.GetBoolean("Led", true);

						var ledR = prefs.GetFloat("LedR", (float)Theme.SecondaryColor.Color.R);
						var ledG = prefs.GetFloat("LedG", (float)Theme.SecondaryColor.Color.G);
						var ledB = prefs.GetFloat("LedB", (float)Theme.SecondaryColor.Color.B);

                        LedColor = new Color(ledR, ledG, ledB);

                        SoundUri = prefs.GetString("SoundUri", string.Empty);
                    }
                }
                catch (Exception ex)
                {
                    global::Heleus.Base.Log.IgnoreException(ex);
                }
            }
        }

		public void Save(Context context)
        {
            lock (_lock)
            {
                try
                {
					using (var prefs = context.GetSharedPreferences(PrefName, FileCreationMode.Private))
                    {
                        var editor = prefs.Edit();

                        editor.PutBoolean("Vibrate", Vibrate);
                        editor.PutBoolean("PlaySound", PlaySound);
                        editor.PutBoolean("Led", UseLed);

                        editor.PutFloat("LedR", (float)LedColor.R);
                        editor.PutFloat("LedG", (float)LedColor.G);
                        editor.PutFloat("LedB", (float)LedColor.B);

                        editor.PutString("SoundUri", SoundUri);

                        editor.Commit();
                    }
                }
                catch (Exception ex)
                {
                    global::Heleus.Base.Log.IgnoreException(ex);
                }
            }
        }
    }
}
