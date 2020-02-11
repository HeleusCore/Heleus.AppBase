using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Text;
using Heleus.Base;

namespace Heleus.Apps.Shared
{
	public static class Tr
	{
		static Translation @base;
		static Translation translation;

		public static void Initalize(string language)
		{
			@base = new Translation("en");
			if (string.IsNullOrWhiteSpace(language) || language == "en")
				translation = @base;
			else
				translation = new Translation(language);

			if (!translation.Valid)
				translation = @base;
		}

		public static bool Has(string key)
		{
			if (translation.Has(key))
				return true;

			return @base.Has(key);
		}

		public static string Get(string key, params object[] parameterList)
		{
			var result = translation.Get(key, out var succes, parameterList);
			if (succes)
				return result;

			return @base.Get(key, parameterList);
		}

		public static string Get(string key, out bool success, params object[] parameterList)
		{
			var result = translation.Get(key, out success, parameterList);
			if(success)
				return result;

			return @base.Get(key, out success, parameterList);
		}

		public static bool Get(string key, out string text, params object[] parameterList)
		{
			var result = translation.Get(key, out var success, parameterList);
			if (success)
			{
				text = result;
				return true;
			}

			result = @base.Get(key, out success, parameterList);
			if(success)
			{
				text = result;
				return true;
			}

			text = null;
			return false;
		}
	}

	public class Translation
	{
		public bool Valid => _items.Count > 0;

		readonly Dictionary<string, string> _items = new Dictionary<string, string>();

		public Translation(string language)
		{
			var lines = FromResource(language);
            var replaces = new List<KeyValuePair<string, string>>();

			foreach(var line in lines)
			{
				var split = line.IndexOf(':');
				if(split > 0)
				{
					var key = line.Substring(0, split);
					var value = line.Substring(split + 2).Replace("\\n", "\n");
					if (value.Length > 0)
					{
						if(value[0] == '.')
                            replaces.Add(new KeyValuePair<string, string>(key, value.Substring(1)));

						_items[key] = value;
					}
				}
			}

            foreach(var replace in replaces)
            {
                if (_items.TryGetValue(replace.Value, out var newValue))
                    _items[replace.Key] = newValue;
            }
        }

		public bool Has(string key)
		{
			return _items.ContainsKey(key);
		}

		public string Get(string key, params object[] parameterList)
		{
			if(_items.TryGetValue(key, out var value))
			{
				if (parameterList != null)
					return string.Format(value, parameterList);
				return value;
			}
			return key;
		}

		public string Get(string key, out bool success, params object[] parameterList)
		{
			if (_items.TryGetValue(key, out var value))
			{
				success = true;
				if (parameterList != null && parameterList.Length > 0)
					return string.Format(value, parameterList);
				return value;
			}

			success = false;
			return key;
		}

		static List<string> FromResource(string language)
		{
			var result = new List<string>();

			try
			{
				var assembly = typeof(Translation).GetTypeInfo().Assembly;

                var res = $"{language}.txt";
                var rescommon = $"{language}.common.txt";

                foreach (string resource in assembly.GetManifestResourceNames())
				{
					if (resource.EndsWith(res, StringComparison.Ordinal) || resource.EndsWith(rescommon, StringComparison.Ordinal))
					{
						using (var stream = assembly.GetManifestResourceStream(resource))
						{
							using (var reader = new StreamReader(stream, Encoding.UTF8))
							{
								while (true)
								{
									var line = reader.ReadLine();
									if (line == null)
										break;
									if (line.Length == 0)
										continue;
									if (line[0] == '#')
										continue;

									result.Add(line);
								}
							}
						}

					}
				}
			}
			catch (Exception ex)
			{
                Log.IgnoreException(ex);
			}

            return result;
		}
	}
}
