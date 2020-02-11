using System;
using System.Collections.Generic;
using Android.Graphics;

namespace Heleus.Apps.Shared.Android.Renderers
{
	static class TypefaceCache
	{
		static readonly Dictionary<string, Typeface> typeFaces = new Dictionary<string, Typeface>();
		public static Typeface CacheTypeFace(string name)
		{
			lock (typeFaces)
			{
				var style = TypefaceStyle.Normal;

                if (typeFaces.TryGetValue(name, out Typeface tf))
                    return tf;

                try
                {
					tf = Typeface.CreateFromAsset(MainActivity.Current.Assets, name);
					if (style != TypefaceStyle.Normal)
					{
						tf = Typeface.Create(tf, style);
					}

					typeFaces[name] = tf;
				}
				catch (Exception ex)
				{
                    global::Heleus.Base.Log.IgnoreException(ex);

					try
					{
						tf = Typeface.Create(name, style);
						typeFaces[name] = tf;
					}
					catch (Exception ex2)
					{
                        global::Heleus.Base.Log.IgnoreException(ex2);
					}
				}

				if (tf == null)
				{
					tf = Typeface.Create(Typeface.Default, style);
				}


				return tf;
			}
		}
	}
}
