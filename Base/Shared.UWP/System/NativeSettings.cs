using System;
using System.Collections.Generic;
using System.Text;

namespace Heleus.Apps.Shared
{
    public abstract class NativeSettings : INativeSettings
    {
        Windows.Storage.ApplicationDataContainer settings = null;
        string name;

        public NativeSettings(string name)
        {
            settings = Windows.Storage.ApplicationData.Current.LocalSettings;
            this.name = name;
            RestoreSettings();
        }

        public void SetValue<T>(string name, T value, INativeSettingsConverter<T> converter)
        {
            converter.ConvertToSetting(this, name, value);
        }

        public T GetValue<T>(string name, T defaultValue, INativeSettingsConverter<T> converter)
        {
            return converter.ConvertFromSetting(this, name, defaultValue);
        }

        public void SetValue(string name, object value)
        {
            try
            {
                settings.Values[this.name + name] = value;
            }
            catch
            {

            }
        }

        public T GetValue<T>(string name, T defaultValue)
        {
            try
            {
                if (!settings.Values.ContainsKey(this.name + name))
                    return defaultValue;

                return (T)settings.Values[this.name + name];
            }
            catch
            {

            }

            return defaultValue;
        }

        protected abstract void RestoreSettings();
        protected abstract void StoreSettings();

        public void SaveSettings()
        {
            StoreSettings();
        }
    }
}
