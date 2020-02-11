using System;
using System.IO;
using System.Reflection;

namespace Heleus.Apps.Shared
{
    public class CliStorageInfo : StorageInfo
    {
        public static bool IsWindows
        {
            get
            {
                var windir = Environment.GetEnvironmentVariable("windir");
                return !string.IsNullOrEmpty(windir) && windir.Contains(@"\") && Directory.Exists(windir);
            }
        }

        public static bool IsMacOS
        {
            get
            {
                return File.Exists(@"/System/Library/CoreServices/SystemVersion.plist");
            }
        }

        public static bool IsLinux
        {
            get
            {
                if (File.Exists(@"/proc/sys/kernel/ostype"))
                {
                    var osType = File.ReadAllText(@"/proc/sys/kernel/ostype");
                    return osType.StartsWith("Linux", StringComparison.OrdinalIgnoreCase);
                }
                return false;
            }
        }

        public CliStorageInfo(StorageInfoTypes type) : base(type)
        {
            var name = $"{Assembly.GetExecutingAssembly().GetName().Name.Replace(" ", string.Empty)}.cli";

            if (IsLinux)
            {
                var user = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);

                if (type == StorageInfoTypes.Document)
                {
                    var documents = Path.Combine(user, ".local/", "share", name) + Path.DirectorySeparatorChar;
#if DEBUG
                    documents = Path.Combine(documents, "debug") + Path.DirectorySeparatorChar;
#endif
                    Directory.CreateDirectory(documents);
                    RootPath = documents;
                }
                else if (type == StorageInfoTypes.Cache)
                {
                    var cache = Path.Combine(user, ".cache", name) + Path.DirectorySeparatorChar;
#if DEBUG
                    cache = Path.Combine(cache, "debug") + Path.DirectorySeparatorChar;
#endif
                    Directory.CreateDirectory(cache);
                    RootPath = cache;
                }
            }
            else if (IsMacOS)
            {
                var root = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments) + Path.DirectorySeparatorChar;

                if (type == StorageInfoTypes.Document)
                {
                    var documents = Path.Combine(root, "Library", "Application Support", name) + Path.DirectorySeparatorChar;
#if DEBUG
                    documents = Path.Combine(documents, "debug") + Path.DirectorySeparatorChar;
#endif
                    Directory.CreateDirectory(documents);
                    RootPath = documents;
                }
                else if (type == StorageInfoTypes.Cache)
                {
                    var cache = Path.GetFullPath(Path.Combine(root, "Library", "Caches", name)) + Path.DirectorySeparatorChar;
#if DEBUG
                    cache = Path.Combine(cache, "debug") + Path.DirectorySeparatorChar;
#endif
                    Directory.CreateDirectory(cache);
                    RootPath = cache;
                }
            }
            else if (IsWindows)
            {
                var local = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);

                if (type == StorageInfoTypes.Document)
                {
                    var documents = Path.Combine(local, name, "documents") + Path.DirectorySeparatorChar;
#if DEBUG
                    documents = Path.Combine(documents, "debug") + Path.DirectorySeparatorChar;
#endif
                    Directory.CreateDirectory(documents);
                    RootPath = documents;
                }
                else if (type == StorageInfoTypes.Cache)
                {
                    var cache = Path.Combine(local, name, "cache") + Path.DirectorySeparatorChar;
#if DEBUG
                    cache = Path.Combine(cache, "debug") + Path.DirectorySeparatorChar;
#endif
                    Directory.CreateDirectory(cache);
                    RootPath = cache;
                }
            }
        }
    }
}
