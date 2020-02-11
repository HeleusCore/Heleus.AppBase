using System;
using System.IO;

namespace Heleus.Apps.Shared
{
    public class GTKStorageInfo : StorageInfo
	{
		public GTKStorageInfo(StorageInfoTypes type) : base(type)
		{
			if(GtkApp.IsFlatpak)
			{
				if(type == StorageInfoTypes.Document)
					RootPath = Environment.GetEnvironmentVariable("XDG_CONFIG_HOME");
				else if (type == StorageInfoTypes.Cache)
					RootPath = Environment.GetEnvironmentVariable("XDG_CACHE_HOME");

				return;
			}

			var name = System.Reflection.Assembly.GetExecutingAssembly().GetName().Name.Replace(" ", string.Empty);
			var user = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);

            if (type == StorageInfoTypes.Document)
			{
				var documents = Path.Combine(user, ".local/", "share", name);
#if DEBUG
                documents += "debug" + Path.DirectorySeparatorChar;
#endif
                Directory.CreateDirectory(documents);
				RootPath = documents;
			}
			else if (type == StorageInfoTypes.Cache)
			{
                var cache = Path.Combine(user, ".cache", name);
#if DEBUG
                cache += "debug" + Path.DirectorySeparatorChar;
#endif
                Directory.CreateDirectory(cache);
				RootPath = cache;
			}
		}
	}
}

