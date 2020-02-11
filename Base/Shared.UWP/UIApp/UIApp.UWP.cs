using Heleus.Apps.Shared.UWP;
using Heleus.Apps.Shared.UWP.Renderers;
using System;
using System.Collections.Generic;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Text;
using System.Threading.Tasks;
using Windows.ApplicationModel;
using Windows.ApplicationModel.Background;
using Windows.ApplicationModel.Resources;
using Windows.Globalization;
using Windows.Networking.PushNotifications;
using Windows.Security.ExchangeActiveSyncProvisioning;
using Windows.Storage.Pickers;
using Windows.System.Profile;
using System.IO;
using Heleus.Base;
using Windows.ApplicationModel.DataTransfer;
using Heleus.Service.Push;

namespace Heleus.Apps.Shared
{
    partial class UIApp
    {
        public static bool ShowLoadingIndicatorNative;

        void PlatformInit()
        {
            IsUWP = true;

            LanguageString = "en";
            try
            {
                var region = new GeographicRegion();
                LanguageString = region.CodeTwoLetter.ToLower();
            }
            catch { }
        }

        public static string PlatformName
        {
            get
            {
                return Network.Client.PlatformName.UWP;
            }
        }

        static string deviceInfo = string.Empty;
        public static string DeviceInfo
        {
            get
            {
                if (string.IsNullOrEmpty(deviceInfo))
                {
                    try
                    {
                        var deviceInfo = new EasClientDeviceInformation();
                        UIApp.deviceInfo = deviceInfo.OperatingSystem + " - " + deviceInfo.SystemManufacturer + " " + deviceInfo.SystemProductName;
                    }
                    catch { }
                    if (string.IsNullOrEmpty(deviceInfo))
                    {
                        deviceInfo = "unkown";
                    }
                }
                return deviceInfo;
            }
        }

        public static string CodedVersion
        {
            get
            {
                var packageId = Package.Current.Id;

                return packageId.Version.Major + "." + packageId.Version.Minor + "." + packageId.Version.Build;
            }
        }

        public BrokerType PushBrokerType => BrokerType.ApnsMacOS;

        public const string TaskName = "BackgroundNotificationTask.Task";
        public const string TaskEntryPoint = "BackgroundNotificationTask.Task";

        bool UnregisterBackgroundTask()
        {
            foreach (var iter in BackgroundTaskRegistration.AllTasks)
            {
                IBackgroundTaskRegistration task = iter.Value;
                if (task.Name == TaskName)
                {
                    task.Unregister(true);
                    return true;
                }
            }
            return false;
        }

        BackgroundTaskRegistration RegisterBackgroundTask(string taskEntryPoint, string taskName, IBackgroundTrigger trigger, IBackgroundCondition condition)
        {
            foreach (var cur in BackgroundTaskRegistration.AllTasks)
            {
                if (cur.Value.Name == taskName)
                {
                    return (BackgroundTaskRegistration)(cur.Value);
                }
            }

            var builder = new BackgroundTaskBuilder
            {
                Name = taskName,
                TaskEntryPoint = taskEntryPoint
            };
            builder.SetTrigger(trigger);

            if (condition != null)
            {
                builder.AddCondition(condition);
            }

            BackgroundTaskRegistration task = builder.Register();
            return task;
        }

        PushNotificationChannel channel = null;
        public void EnableRemoteNotifications()
        {
            Xamarin.Forms.Device.BeginInvokeOnMainThread(async () =>
            {
                try
                {
                    var access = await BackgroundExecutionManager.RequestAccessAsync();
                    if (access != BackgroundAccessStatus.DeniedByUser && access != BackgroundAccessStatus.DeniedBySystemPolicy && access != BackgroundAccessStatus.Unspecified)
                    {
                        try
                        {
                            //UnregisterBackgroundTask();
                            RegisterBackgroundTask(TaskEntryPoint, TaskName, new PushNotificationTrigger(), null);
                        }
                        catch
                        {
                        }

                        channel = await PushNotificationChannelManager.CreatePushNotificationChannelForApplicationAsync();
                        channel.PushNotificationReceived += PushNotificationReceived;
                        RemoteNotifiactionTokenResult(channel.Uri);
                    }
                    else
                    {
                        RemoteNotifiactionTokenResult(null);
                    }
                }
                catch
                {
                }
            });
        }

        private void PushNotificationReceived(PushNotificationChannel sender, PushNotificationReceivedEventArgs args)
        {
            try
            {
                //args.Cancel = WindowsApp.InForeground; // do not propagate it to the background task, if the windows is active

                if (args.NotificationType == PushNotificationType.Raw)
                {
                    var rawNotification = args.RawNotification;
                    var xml = new Windows.Data.Xml.Dom.XmlDocument();
                    xml.LoadXml(rawNotification.Content);

                    var scheme = string.Empty;

                    foreach (var attribute in xml.FirstChild.Attributes)
                    {
                        if (attribute.NodeName == "scheme")
                            scheme = attribute.NodeValue.ToString();
                    }

                    if (!string.IsNullOrEmpty(scheme))
                    {
                        Run(async () =>
                        {
                            await PubSub.PublishAsync(new PushNotificationEvent(new Uri(scheme), PushNotificationEventType.NoneUserInteraction));
                        });
                    }
                }
            }
            catch (Exception ex)
            {
                Log.IgnoreException(ex);
            }
        }

        public static void Toast(string text)
        {
            Xamarin.Forms.Device.BeginInvokeOnMainThread(async () =>
            {
                try
                {
                    await Task.Delay(500);
                    new ToastPrompt
                    {
                        Message = text
                    }.Show();
                }
                catch { }
            });
        }

        public static bool CanRate => AppStoreIdentifier != null;

        public static void RateApp()
        {
            if (!CanRate)
                return;

            Xamarin.Forms.Device.BeginInvokeOnMainThread(async () => {
                try
                {
                    var uri = new Uri("ms-windows-store://review/?ProductId=" + AppStoreIdentifier);
                    await Windows.System.Launcher.LaunchUriAsync(uri);
                }
                catch { }
            });
        }

        public static string ShareText
        {
            get;
            private set;
        }

        public static void Share(string text)
        {
            // https://msdn.microsoft.com/en-us/library/windows/apps/xaml/hh871368.aspx
            ShareText = text;
            Windows.ApplicationModel.DataTransfer.DataTransferManager.ShowShareUI();
        }

        public static async Task OpenImagePicker(Action<ImageHandler> action)
        {
            var filePicker = new FileOpenPicker();
            filePicker.FileTypeFilter.Add(".jpg");
            filePicker.FileTypeFilter.Add(".jpeg");
            filePicker.FileTypeFilter.Add(".png");

            filePicker.ViewMode = PickerViewMode.Thumbnail;
            filePicker.SuggestedStartLocation = PickerLocationId.PicturesLibrary;

            try
            {
                var imageFile = await filePicker.PickSingleFileAsync();
                if (imageFile == null)
                    return;

                using (var fileStream = await imageFile.OpenAsync(Windows.Storage.FileAccessMode.Read))
                {
                    var ih = await ImageHandler.FromStream(fileStream.AsStream());
                    if (ih != null)
                        action?.Invoke(ih);
                }
            }
            catch (Exception ex)
            {
                Log.IgnoreException(ex);
            }
        }

        public static async Task<OpenFile> OpenFilePicker2(params string[] extensions)
        {
            var filePicker = new FileOpenPicker();
            filePicker.FileTypeFilter.Add("*");

            filePicker.ViewMode = PickerViewMode.List;

            try
            {
                var file = await filePicker.PickSingleFileAsync();
                if (file == null)
                {
                    return new OpenFile();
                }

                var fileStream = await file.OpenAsync(Windows.Storage.FileAccessMode.Read);

                return new OpenFile(file.Name, fileStream.AsStream());
            }
            catch (Exception ex)
            {
                Log.IgnoreException(ex);
            }

            return new OpenFile();
        }

        public static async Task SaveFilePicker2(byte[] data, string name)
        {
            var filePicker = new FileSavePicker();
            filePicker.SuggestedFileName = name;

            try
            {
                var file = await filePicker.PickSaveFileAsync();
                if (file == null)
                {
                    return;
                }

                using (var fileStream = await file.OpenAsync(Windows.Storage.FileAccessMode.ReadWrite))
                {
                    fileStream.Size = 0;
                    await fileStream.WriteAsync(data.AsBuffer());
                }
            }
            catch (Exception ex)
            {
                Log.IgnoreException(ex);
            }
        }

        public static void CopyToClipboard(string text)
        {
            var package = new DataPackage();
            package.SetText(text);
            Clipboard.SetContent(package);
        }

        public static async Task<string> CopyFromClipboard()
        {
            var content = Clipboard.GetContent();
            if (content.Contains(StandardDataFormats.Text))
                return await content.GetTextAsync().AsTask();

            return null;
        }
    }
}