using System;
using Heleus.Base;

namespace Heleus.Apps.Shared
{
	public abstract class NativeSettings : INativeSettings
	{
		static global::Android.Content.Context _context;

		public static void Init(global::Android.Content.Context context)
        {
            _context = context;
        }

		readonly string settingName;

		global::Android.Content.ISharedPreferences preferences;
		global::Android.Content.ISharedPreferencesEditor editor;

		public NativeSettings(string name)
		{
			this.settingName = name;
#if DEBUG
			if (_context == null)
				throw new Exception("NativeSettings.Context is null");
#endif
			try
			{
				using (preferences = _context.GetSharedPreferences(name, global::Android.Content.FileCreationMode.Private))
				{
#pragma warning disable RECS0021 // Warns about calls to virtual member functions occuring in the constructor
                    RestoreSettings();
#pragma warning restore RECS0021 // Warns about calls to virtual member functions occuring in the constructor
                }
			}
            catch (Exception ex)
			{
                Log.IgnoreException((ex));
			}

			preferences = null;
		}

		public T GetValue<T>(string name, T defaultValue)
		{
			try
			{
				if (preferences.Contains(name))
				{
					var type = typeof(T);
					if (type == typeof(long))
					{
						return (T)((object)preferences.GetLong(name, (long)((object)defaultValue)));
					}
					else if (type == typeof(int))
					{
						return (T)((object)preferences.GetLong(name, (int)((object)defaultValue)));
					}
					else if (type == typeof(double))
					{
						return (T)((object)(double)(preferences.GetFloat(name, Convert.ToSingle((double)((object)defaultValue)))));
					}
					else if (type == typeof(float))
					{
						return (T)((object)preferences.GetFloat(name, (float)((object)defaultValue)));
					}
					else if (type == typeof(bool))
					{
						return (T)((object)preferences.GetBoolean(name, (bool)((object)defaultValue)));
					}
                    else if (type == typeof(string))
                    {
                        return (T)((object)preferences.GetString(name, (string)((object)defaultValue)));
					}
					else
					{
						throw new Exception("NativeSettings.GetValue: invalid type");
					}
				}
			}
			catch (Exception ex)
			{
                Log.IgnoreException(ex);
			}
			return defaultValue;
		}

		public void SetValue(string name, object value)
		{
			try
			{
				if (editor != null)
				{
					if (value is long || value is int)
					{
						editor.PutLong(name, (long)value);
					}
					else if (value is float || value is double)
					{
						editor.PutFloat(name, Convert.ToSingle((double)value));
					}
					else if (value is bool)
					{
						editor.PutBoolean(name, (bool)value);
					}
                    else if (value is string)
                    {
                        editor.PutString(name, (string)value);
                    }
					else
					{
						throw new Exception("NativeSettings.SetValue: invalid type");
					}
				}
				else
				{
					throw new Exception("NativeSettings.SetValue editor is null");
				}
			}
			catch (Exception ex)
			{
                Log.IgnoreException(ex);
			}
		}

		public T GetValue<T>(string name, T defaultValue, INativeSettingsConverter<T> converter)
		{
			return converter.ConvertFromSetting(this, name, defaultValue);
		}

		public void SetValue<T>(string name, T value, INativeSettingsConverter<T> converter)
		{
			converter.ConvertToSetting(this, name, value);
		}

		protected abstract void RestoreSettings();
		protected abstract void StoreSettings();

		public void SaveSettings()
		{
			try
			{
				using (preferences = _context.GetSharedPreferences(settingName, global::Android.Content.FileCreationMode.Private))
				{

					editor = preferences.Edit();
					StoreSettings();
					editor.Commit();
				}
			}
            catch(Exception ex)
			{
                Log.IgnoreException(ex);
			}

			preferences = null;
			editor = null;
		}
	}
}
