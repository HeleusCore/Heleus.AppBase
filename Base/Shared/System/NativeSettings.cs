using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using Heleus.Base;

namespace Heleus.Apps.Shared
{
#if (WPF || GTK || MACOS || __IOS__ || CLI)
    public abstract class NativeSettings : INativeSettings
	{
		readonly string _settingName;
		readonly string _filePath;

		readonly Dictionary<string, object> _items = new Dictionary<string, object>();

		protected NativeSettings(string name)
		{
			_settingName = name;
			_filePath = Path.Combine(StorageInfo.DocumentStorage.RootPath, _settingName);

			if(File.Exists(_filePath))
			{
				try
				{
					using (var fileStream = File.OpenRead(_filePath))
					{
						_items = Deserialize<Dictionary<string, object>>(fileStream);
					}
				}
				catch(Exception ex)
				{
                    Log.HandleException(ex);
				}
			}

#pragma warning disable RECS0021 // Warns about calls to virtual member functions occuring in the constructor
			RestoreSettings();
#pragma warning restore RECS0021 // Warns about calls to virtual member functions occuring in the constructor
		}

		public T GetValue<T>(string valueKey, T defaultValue, INativeSettingsConverter<T> converter)
		{
			return converter.ConvertFromSetting(this, valueKey, defaultValue);
		}

		public T GetValue<T>(string valueKey, T defaultValue)
		{
			try
			{
				if (_items.TryGetValue(_settingName + valueKey, out var value))
					return (T)value;
			}
			catch(Exception ex)
			{
				Log.HandleException(ex);
			}
			return defaultValue;
		}

		public void SaveSettings()
		{
			try
			{
				StoreSettings();
				using (var stream = File.OpenWrite(_filePath))
				{
					Serialize(_items, stream);
				}
			}
			catch(Exception ex)
			{
				Log.HandleException(ex);
			}
		}

		public void SetValue<T>(string valueKey, T value, INativeSettingsConverter<T> converter)
		{
			converter.ConvertToSetting(this, valueKey, value);
		}

		public void SetValue(string valueKey, object value)
		{
			_items[_settingName + valueKey] = value;
		}

		protected abstract void RestoreSettings();
		protected abstract void StoreSettings();

		static void Serialize<T>(T item, Stream stream)
		{
			var formatter = new BinaryFormatter();
			formatter.Serialize(stream, item);
		}

		static T Deserialize<T>(Stream stream) where T : new()
		{
			var formatter = new BinaryFormatter();
			return (T)formatter.Deserialize(stream);
		}
	}
#endif
}
