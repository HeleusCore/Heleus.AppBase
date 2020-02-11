namespace Heleus.Apps.Shared
{
	public interface INativeSettingsConverter<T>
	{
		void ConvertToSetting(INativeSettings settings, string valueKey, T value);
		T ConvertFromSetting(INativeSettings settings, string valueKey, T defaultValue);
	}

	public interface INativeSettings
	{
		void SetValue<T>(string valueKey, T value, INativeSettingsConverter<T> converter);
		T GetValue<T>(string valueKey, T defaultValue, INativeSettingsConverter<T> converter);

		void SetValue(string valueKey, object value);
		T GetValue<T>(string valueKey, T defaultValue);

		void SaveSettings();
	}

	public class ColorSettingsConverter : INativeSettingsConverter<Xamarin.Forms.Color>
	{
		public static readonly ColorSettingsConverter Instance = new ColorSettingsConverter();

		public void ConvertToSetting(INativeSettings settings, string valueKey, Xamarin.Forms.Color color)
		{
			settings.SetValue(valueKey + "_r", color.R);
			settings.SetValue(valueKey + "_g", color.G);
			settings.SetValue(valueKey + "_b", color.B);
			settings.SetValue(valueKey + "_a", color.A);
		}

		public Xamarin.Forms.Color ConvertFromSetting(INativeSettings settings, string valueKey, Xamarin.Forms.Color defaultColor)
		{
			var r = settings.GetValue(valueKey + "_r", defaultColor.R);
			var g = settings.GetValue(valueKey + "_g", defaultColor.G);
			var b = settings.GetValue(valueKey + "_b", defaultColor.B);
			var a = settings.GetValue(valueKey + "_a", defaultColor.A);

			return new Xamarin.Forms.Color(r, g, b, a);
		}
	}
}
