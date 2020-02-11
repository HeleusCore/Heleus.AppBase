using System;
using System.IO;

namespace Heleus.Apps.Shared
{
    public class AppleStorageInfo : StorageInfo
    {
        public AppleStorageInfo(StorageInfoTypes type) : base(type)
        {
            var root = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments) + Path.DirectorySeparatorChar;

#if __MOBILE__
            var cache = Path.GetFullPath(Path.Combine(root, "..", "Library", "Caches")) + Path.DirectorySeparatorChar;
            var documents = Path.GetFullPath(Path.Combine(root, "..", "Library", "Application Support")) + Path.DirectorySeparatorChar;
#elif MACOS
			var documents = Path.Combine(root, "Library", "Application Support", Foundation.NSBundle.MainBundle.BundleIdentifier) + Path.DirectorySeparatorChar;
			var cache = Path.GetFullPath(Path.Combine(root, "Library", "Caches", Foundation.NSBundle.MainBundle.BundleIdentifier)) + Path.DirectorySeparatorChar;
			
#endif
            if (type == StorageInfoTypes.Document)
            {
#if DEBUG
                documents += "debug" + Path.DirectorySeparatorChar;
#endif
                Directory.CreateDirectory(documents);
                RootPath = documents;
            }
            else if (type == StorageInfoTypes.Cache)
            {
#if DEBUG
                cache += "debug" + Path.DirectorySeparatorChar;
#endif
                Directory.CreateDirectory(cache);
                RootPath = cache;
            }
        }
    }
}

