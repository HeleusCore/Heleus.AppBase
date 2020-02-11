using System;
using System.Threading.Tasks;
using Heleus.Service.Push;

namespace Heleus.Apps.Shared
{
    public partial class UIApp
    {
        void PlatformInit()
        {
            IsCLI = true;
            LanguageString = "en";
        }

        public BrokerType PushBrokerType = BrokerType.None;

        public void EnableRemoteNotifications()
        {
        }

        public static string PlatformName
        {
            get
            {
                return Network.Client.PlatformName.CLI;
            }
        }

        public static string DeviceInfo
        {
            get
            {
                return "cli";
            }
        }

        public static string CodedVersion
        {
            get
            {
                return Tr.Get("App.Version");
            }
        }

        public static void Share(string text)
        {
        }

        public static void Toast(string text)
        {
        }

        public const bool CanRate = false;

        public static void RateApp()
        {
        }

        public static bool ShowLoadingIndicatorNative;

        public static Task SaveImageToPhotoLibrary(byte[] imageData, string comment)
        {
            return Task.CompletedTask;
        }

        public static Task<OpenFile> OpenFilePicker2(params string[] extensions)
        {
            return Task.FromResult(new OpenFile());
        }

        public static Task SaveFilePicker2(byte[] data, string filename)
        {
            return Task.CompletedTask;
        }

        public static Task OpenImagePicker(Func<ImageHandler, Task> successAsync)
        {
            return Task.CompletedTask;
        }

        public static void CopyToClipboard(string text)
        {
        }

        public static Task<string> CopyFromClipboard()
        {
            return Task.FromResult<string>(null);
        }
    }
}
