using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Heleus.Base;
using Heleus.Network.Client;

namespace Heleus.Apps.Shared
{
	public partial class UIApp
	{
        static string ErrorReportFilePath => Path.Combine(StorageInfo.CacheStorage.RootPath, "errorreports.txt");

        readonly static object _errorLock = new object();
        readonly static Dictionary<int, ClientErrorReport> _errorReports = new Dictionary<int, ClientErrorReport>();
        static bool _errorsLoaded;
        static bool _saving;

        void InitErrorReporting()
		{
            PubSub.Subscribe<LogEvent>(this, LogEvent);
#if MACOS
			ObjCRuntime.Runtime.MarshalManagedException += (sender, args) => 
			{
                Log.HandleException(args.Exception);
			};
#endif

#if GTK && !DEBUG
			GLib.ExceptionManager.UnhandledException += (GLib.UnhandledExceptionArgs args) => 
			{
                Log.HandleException(args.ExceptionObject as Exception);
			};
#endif
            LoadErrorReports();
		}

        public static async Task UploadErrorReports(ServiceNode serviceNode)
        {
            var serviceAccount = serviceNode?.FirstUnlockedServiceAccount;
            if (serviceNode == null || serviceAccount == null)
                return;

            var reports = await GetErrorReports();
            if (reports != null)
            {
                if (await serviceNode.Client.UploadErrorReports(reports, serviceAccount))
                {
                    await ClearErrorReports();
                }
            }
        }

        static Task LogEvent(LogEvent logEvent)
        {
            if (logEvent.LogLevel >= LogLevels.Error)
            {
                try
                {
                    var save = false;
                    var report = new ClientErrorReport(logEvent.OriginalMessage, CodedVersion, LanguageString, PlatformName, DeviceInfo);
                    lock (_errorLock)
                    {
                        if (_errorReports.TryGetValue(report.Hash, out var stored))
                            stored.Increment();
                        else
                            _errorReports[report.Hash] = report;

                        save = _errorsLoaded;
                    }

                    if (save)
                        SaveErrorReports();

                }
                catch { }
            }

            return Task.CompletedTask;
        }

        static Task<byte[]> GetErrorReports()
        {
            return Task.Run(() =>
            {
                try
                {
                    lock(_errorLock)
                    {
                        if (_errorReports.Count <= 0)
                            return null;
                    }

                    using (var packer = new Packer())
                    {
                        lock (_errorLock)
                            packer.Pack(_errorReports);
                        return packer.ToByteArray();
                    }
                } catch { }
                return null;
            });
        }

        static Task ClearErrorReports()
        {
            return Task.Run(() =>
            {
                try
                {
                    _errorReports.Clear();
                    var filePath = ErrorReportFilePath;
                    if (File.Exists(filePath))
                        File.Delete(filePath);
                } catch { }
            });
        }

        static void SaveErrorReports()
        {
#if CLI
            return;
#endif

            lock(_errorLock)
            {
                if (_saving)
                    return;
                _saving = true;
            }

            try
            {
                using (var packer = new Packer())
                {
                    lock (_errorLock)
                    {
                        packer.Pack(_errorReports);
                    }
                    File.WriteAllBytes(ErrorReportFilePath, packer.ToByteArray());
                }
            }
            catch { }

            lock (_errorLock)
                _saving = false;
        }

        static void LoadErrorReports()
        {
            lock(_errorLock)
            {
                if (_errorsLoaded)
                    return;
            }

            Task.Run(() =>
            {
                try
                {
                    var save = false;
                    if (File.Exists(ErrorReportFilePath))
                    {
                        var data = File.ReadAllBytes(ErrorReportFilePath);
                        using (var unpacker = new Unpacker(data))
                        {
                            var storedReports = unpacker.UnpackList((u) => new ClientErrorReport(u));
                            lock (_errorLock)
                            {
                                if (_errorsLoaded)
                                    return;

                                save = _errorReports.Count > 0;
                                foreach (var report in storedReports)
                                {
                                    if (_errorReports.TryGetValue(report.Hash, out var stored))
                                        stored.Increment();
                                    else
                                        _errorReports[report.Hash] = report;
                                    _errorsLoaded = true;
                                }
                            }
                        }
                    }

                    if (save)
                        SaveErrorReports();
                }
                catch { }
                finally
                {
                    lock(_errorLock)
                        _errorsLoaded = true;
                }
            });
        }
    }
}
