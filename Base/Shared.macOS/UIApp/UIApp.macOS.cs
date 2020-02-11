using System;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using AppKit;
using Foundation;
using Heleus.Base;
using Heleus.Service.Push;
using Xamarin.Forms;

namespace Heleus.Apps.Shared
{
    partial class UIApp
    {
        public static readonly bool HasRemoteNotificationSupport = true;

        void PlatformInit()
        {
            IsMacOS = true;
            LanguageString = NSLocale.CurrentLocale.LanguageCode.ToLower();
        }

        public void EnableRemoteNotifications()
        {
            NSApplication.SharedApplication.RegisterForRemoteNotificationTypes(NSRemoteNotificationType.Alert | NSRemoteNotificationType.Sound);
        }

        public static string PlatformName
        {
            get
            {
                return Network.Client.PlatformName.MACOS;
            }
        }

        public BrokerType PushBrokerType => BrokerType.ApnsMacOS;

        [DllImport("/usr/lib/libSystem.dylib")]
        static internal extern int sysctlbyname([MarshalAs(UnmanagedType.LPStr)] string property, IntPtr output, IntPtr oldLen, IntPtr newp, uint newlen);

        public static string GetSystemProperty(string property)
        {
            var pLen = Marshal.AllocHGlobal(sizeof(int));
            sysctlbyname(property, IntPtr.Zero, pLen, IntPtr.Zero, 0);
            var length = Marshal.ReadInt32(pLen);
            var pStr = Marshal.AllocHGlobal(length);
            sysctlbyname(property, pStr, pLen, IntPtr.Zero, 0);

            var result = Marshal.PtrToStringAnsi(pStr);
            Marshal.FreeHGlobal(pLen);
            Marshal.FreeHGlobal(pStr);
            return result;
        }

        public static string DeviceInfo
        {
            get
            {
                return GetSystemProperty("hw.model") + "/" + NSProcessInfo.ProcessInfo.OperatingSystemVersionString;
            }
        }

        public static string CodedVersion
        {
            get
            {
                return NSBundle.MainBundle.InfoDictionary["CFBundleShortVersionString"].ToString();
            }
        }

        public static void Share(string text)
        {
            Run(async () =>
            {
                await Task.Delay(50);

                try
                {
                    var window = NSApplication.SharedApplication.MainWindow;
                    if (window != null)
                    {
                        var last = window.Toolbar?.Items?.LastOrDefault();
                        if (last != null)
                        {
                            var picker = new NSSharingServicePicker(new NSObject[] { new NSString(text) });
                            picker.ShowRelativeToRect(last.View.Bounds, last.View, NSRectEdge.MaxXEdge);
                        }
                        else
                        {
                            var picker = new NSSharingServicePicker(new NSObject[] { new NSString(text) });
                            picker.ShowRelativeToRect(window.ContentView.Bounds, window.ContentView, NSRectEdge.MaxXEdge);
                        }
                    }
                }
                catch (Exception ex)
                {
                    Log.IgnoreException(ex);
                }
            });
        }

        public static void Toast(string text)
        {
            Run(async () =>
            {
                try
                {
                    var p = Current?.CurrentPage;
                    if (p != null)
                    {
                        var frame = new Frame
                        {
                            InputTransparent = true,
                            Padding = new Thickness(10),
                            BackgroundColor = Theme.SecondaryColor.Color,
                            Opacity = 0
                        };

                        AbsoluteLayout.SetLayoutFlags(frame, AbsoluteLayoutFlags.PositionProportional);
                        AbsoluteLayout.SetLayoutBounds(frame, new Rectangle(0.5, 0.9, AbsoluteLayout.AutoSize, AbsoluteLayout.AutoSize));
                        frame.Content = new ExtLabel
                        {
                            Text = text,
                            TextColor = Theme.TextColor.Color,
                            FontStyle = Theme.RowFont,
                            HorizontalTextAlignment = TextAlignment.Center,
                            InputTransparent = true
                        };

                        p.RootLayout.Children.Add(frame);
                        await frame.FadeTo(1);
                        await Task.Delay(1500);
                        await frame.FadeTo(0);
                        p.RootLayout.Children.Remove(frame);
                    }
                }
                catch (Exception ex)
                {
                    Log.IgnoreException(ex);
                }
            });
        }

        public static bool CanRate => AppStoreIdentifier != null;

        public static void RateApp()
        {
            if (CanRate)
                //http://stackoverflow.com/questions/4805991/mac-app-store-link-to-app-review-page
                UIApp.OpenUrl(new Uri("macappstore://itunes.apple.com/app/id" + AppStoreIdentifier + "?mt=12"));
        }

        public static bool ShowLoadingIndicatorNative
        {
            set
            {
                //NativeToolbarTracker.ProgessActive = value;
            }
        }

        public static Task<OpenFile> OpenFilePicker2(params string[] extentions)
        {
            var panel = new NSOpenPanel
            {
                CanChooseFiles = true,
                CanChooseDirectories = false,
                AllowsMultipleSelection = false,
                //AllowedFileTypes = new string[] { "png", "PNG", "jpg", "JPG", "jpeg", "JPEG" }
            };

            if (extentions != null && extentions.Length > 0)
                panel.AllowedFileTypes = extentions;

            if (panel.RunModal() == 1)
            {
                try
                {
                    var filePath = panel.Urls[0].Path;
                    var stream = File.OpenRead(filePath);

                    return Task.FromResult(new OpenFile(Path.GetFileName(filePath), stream));
                }
                catch (Exception ex)
                {
                    Log.IgnoreException(ex);
                }
            }
            return Task.FromResult(new OpenFile());
        }

        public static Task SaveFilePicker2(byte[] data, string name)
        {
            if (string.IsNullOrEmpty(name))
                name = string.Empty;

            var panel = new NSSavePanel
            {
                NameFieldStringValue = name
            };

            if(panel.RunModal() == 1)
            {
                using(var file = File.OpenWrite(panel.Url.Path))
                {
                    file.Write(data, 0, data.Length);
                }
            }

            return Task.CompletedTask;
        }

        public static async Task OpenImagePicker(Func<ImageHandler, Task> successAsync)
        {
            var panel = new NSOpenPanel
            {
                CanChooseFiles = true,
                CanChooseDirectories = false,
                AllowsMultipleSelection = false,
                AllowedFileTypes = new string[] { "png", "PNG", "jpg", "JPG", "jpeg", "JPEG" }
            };

            if (panel.RunModal() == 1)
            {
                try
                {
                    var image = await ImageHandler.FromFile(panel.Urls[0].Path);
                    if (image != null && successAsync != null)
                    {
                        await successAsync.Invoke(image);
                    }
                }
                catch (Exception ex)
                {
                    Log.IgnoreException(ex);
                }
            }
        }

        public static async Task SaveImagePicker(byte[] imageData, string comment)
        {
            var panel = new NSSavePanel
            {
                NameFieldStringValue = "image.jpg"
            };

            if (panel.RunModal() == 1)
            {
                using (var image = await ImageHandler.FromData(imageData))
                {
                    await image.Save(panel.Url.Path);
                }
            }
        }

        static readonly string[] pboardTypes = { NSPasteboard.NSStringType };

        public static void CopyToClipboard(string text)
        {
            try
            {
                var pasteboard = NSPasteboard.GeneralPasteboard;
                pasteboard.DeclareTypes(pboardTypes, null);
                pasteboard.SetStringForType(text, NSPasteboard.NSStringType);
            }
            catch (Exception ex)
            {
                Log.IgnoreException(ex);
            }
        }

        public static Task<string> CopyFromClipboard()
        {
            try
            {
                var pasteboard = NSPasteboard.GeneralPasteboard;
                return Task.FromResult(pasteboard.GetStringForType(NSPasteboard.NSStringType));
            }
            catch (Exception ex)
            {
                Log.IgnoreException(ex);
            }
            return Task.FromResult<string>(null);
        }
    }
}
