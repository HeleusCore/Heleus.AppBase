using System;
using System.Threading.Tasks;
using System.IO;
using System.Collections.Generic;
using Heleus.Base;

#if UWP
using Windows.Storage;
using Windows.Storage.Search;
#endif

namespace Heleus.Apps.Shared
{
    public enum StorageInfoTypes
    {
        Document,
        Cache
    }

    public abstract class StorageInfo
	{
		static StorageInfo _documentStorage;
		static StorageInfo _cacheStorage;

        public static StorageInfo DocumentStorage
		{
			get
			{
				if (_documentStorage == null)
				{
#if (__IOS__ || MACOS)
					_documentStorage = new AppleStorageInfo(StorageInfoTypes.Document);
#elif ANDROID
					_documentStorage = new AndroidStorage(StorageInfoTypes.Document);
#elif UWP
					_documentStorage = new WinRtStorage(StorageInfoTypes.Document);
#elif GTK
					_documentStorage = new GTKStorageInfo(StorageInfoTypes.Document);
#elif WPF
                    _documentStorage = new WPFStorage(StorageInfoTypes.Document);
#elif CLI
                    _documentStorage = new CliStorageInfo(StorageInfoTypes.Document);
#endif
                    Log.Trace($"Document Path: {_documentStorage.RootPath}");
                }

				return _documentStorage;
			}
		}

		public static StorageInfo CacheStorage
		{
			get
			{
				if (_cacheStorage == null)
				{
#if (__IOS__ || MACOS)
					_cacheStorage = new AppleStorageInfo(StorageInfoTypes.Cache);
#elif ANDROID
					_cacheStorage = new AndroidStorage(StorageInfoTypes.Cache);
#elif UWP
					_cacheStorage = new WinRtStorage(StorageInfoTypes.Cache);
#elif GTK
					_cacheStorage = new GTKStorageInfo(StorageInfoTypes.Cache);
#elif WPF
                    _cacheStorage = new WPFStorage(StorageInfoTypes.Cache);
#elif CLI
                    _cacheStorage = new CliStorageInfo(StorageInfoTypes.Cache);
#endif
                    Log.Trace($"Cache Path: {_cacheStorage.RootPath}");
                }

                return _cacheStorage;
			}
		}

		public string RootPath
		{
			get;
			protected set;
		}

		public readonly StorageInfoTypes StorageType;

		protected StorageInfo(StorageInfoTypes storageType)
		{
			StorageType = storageType;
		}

        public Task<bool> DeleteAsync(string filePath)
        {
            return Task.Run(() =>
            {
                try
                {
                    File.Delete(Path.Combine(RootPath, filePath));
                    return true;
                }
                catch (Exception ex)
                {
                    Log.IgnoreException(ex);
                }
                return false;
            });
        }

        public Task<bool> ExistsAsync(string filePath)
        {
            return Task.Run(() =>
            {
                return File.Exists(Path.Combine(RootPath, filePath));
            });
        }

        public Task<byte[]> ReadAllBytesAsync(string filePath)
        {
            return Task.Run(() =>
            {
                try
                {
                    return File.ReadAllBytes(Path.Combine(RootPath, filePath));
                }
                catch (Exception ex)
                {
                    Log.IgnoreException(ex);
                }

                return null;
            });
        }

        public Task<bool> WriteAllBytesAsync(string filePath, byte[] data)
        {
            return Task.Run(() =>
            {
                try
                {
                    var path = Path.GetDirectoryName(filePath);
                    Directory.CreateDirectory(path);
                }
                catch (Exception ex)
                {
                    Log.IgnoreException(ex);
                }

                try
                {
                    File.WriteAllBytes(Path.Combine(RootPath, filePath), data);
                    return true;
                }
                catch (Exception ex)
                {
                    Log.IgnoreException(ex);
                }

                return false;
            });
        }

        public Task<List<string>> GetFileList(string path)
        {
            return Task.Run(() =>
            {
                var result = new List<string>();
                try
                {
                    var d = new DirectoryInfo(Path.Combine(RootPath, path));
                    var files = d.GetFiles();
                    foreach (var file in files)
                    {
                        result.Add(file.Name);
                    }
                }
                catch (Exception ex)
                {
                    Log.IgnoreException(ex);
                }

                return result;
            });
        }
    }

#if WPF
    public class WPFStorage : StorageInfo
    {
        public WPFStorage(StorageInfoTypes type) : base(type)
        {
            var name = System.Reflection.Assembly.GetExecutingAssembly().GetName().Name;
            var local = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);

            if (type == StorageInfoTypes.Document)
            {
                var documents = Path.Combine(local, name, "documents");
#if DEBUG
                documents += "debug" + Path.DirectorySeparatorChar;
#endif
                Directory.CreateDirectory(documents);
                RootPath = documents;
            }
            else if (type == StorageInfoTypes.Cache)
            {
                var cache = Path.Combine(local, name, "cache");
#if DEBUG
                cache += "debug" + Path.DirectorySeparatorChar;
#endif
                Directory.CreateDirectory(cache);
                RootPath = cache;
            }
        }
    }
#endif


#if UWP
    public class WinRtStorage : StorageInfo
    {
        public StorageFolder Folder
        {
            get;
            private set;
        }

        public WinRtStorage(StorageInfoTypes type) : base(type)
        {
            if(type == StorageInfoTypes.Document)
            {
                Folder = ApplicationData.Current.LocalFolder;
                RootPath = Folder.Path + Path.DirectorySeparatorChar;
            }
            else if (type == StorageInfoTypes.Cache)
            {
                Folder = ApplicationData.Current.LocalCacheFolder;
                RootPath = Folder.Path + Path.DirectorySeparatorChar;
            }
        }
    }
#endif

#if ANDROID
    public class AndroidStorage : StorageInfo
    {
        static global::Android.Content.Context ctx;

        public static void Init(global::Android.Content.Context context) 
        {
            ctx = context;
        }

        public AndroidStorage(StorageInfoTypes type) : base(type)
        {
            if (type == StorageInfoTypes.Document) {
                RootPath = ctx.FilesDir.AbsolutePath + Path.DirectorySeparatorChar;
            } else if (type == StorageInfoTypes.Cache) {
                RootPath = ctx.CacheDir.AbsolutePath + Path.DirectorySeparatorChar;
            }
        }
    }
#endif
}

