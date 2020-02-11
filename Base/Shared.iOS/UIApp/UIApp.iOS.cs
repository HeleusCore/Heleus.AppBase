using System;
using System.IO;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using CoreGraphics;
using Foundation;
using Heleus.Base;
using Heleus.Service.Push;
using MediaPlayer;
using Photos;
using Security;
using SkiaSharp;
using SkiaSharp.Views.iOS;
using UIKit;
using UserNotifications;
using Xamarin.Forms;

namespace Heleus.Apps.Shared
{
    partial class UIApp
    {
        void PlatformInit()
        {
            IsIOS = true;
            LanguageString = NSLocale.CurrentLocale.LanguageCode.ToLower();
        }

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

        public static string PlatformName
        {
            get
            {
                return Network.Client.PlatformName.IOS;
            }
        }

        public BrokerType PushBrokerType => BrokerType.ApnsIOS;

        public static string DeviceInfo
        {
            get
            {
                return GetSystemProperty("hw.machine") + "/" + UIDevice.CurrentDevice.SystemVersion;
            }
        }

        public static string CodedVersion
        {
            get
            {
                return NSBundle.MainBundle.InfoDictionary["CFBundleShortVersionString"].ToString();
            }
        }

        static UIViewController GetRootController()
        {
            var rootController = UIApplication.SharedApplication.KeyWindow.RootViewController;
            if (rootController is UINavigationController)
                rootController = (rootController as UINavigationController).VisibleViewController;
            else if (rootController.ModalViewController != null)
                rootController = rootController.ModalViewController;
            return rootController;
        }

        public static void Share(string text)
        {
            try
            {
                var activityController = new UIActivityViewController(new NSObject[] { NSObject.FromObject(text) }, null);
                var rootController = GetRootController();

                if (null != activityController.PopoverPresentationController)
                {
                    activityController.PopoverPresentationController.SourceView = rootController.View;
                    var frame = UIScreen.MainScreen.Bounds;
                    frame.Height /= 2;
                    activityController.PopoverPresentationController.SourceRect = frame;
                }

                rootController.PresentViewController(activityController, true, null);
            }
            catch (Exception ex)
            {
                Log.IgnoreException(ex);
            }
        }

        public static bool CanRate => !string.IsNullOrWhiteSpace(AppStoreIdentifier);

        public static void RateApp()
        {
            if (CanRate)
                OpenUrl(new Uri("http://itunes.apple.com/WebObjects/MZStore.woa/wa/viewContentsUserReviews?id=" + AppStoreIdentifier + "&pageNumber=0&sortOrdering=2&type=Purple+Software&mt=8"));
        }

        static IDisposable currentToast = null;

        public static void Toast(string message)
        {
            currentToast?.Dispose();

            var app = UIKit.UIApplication.SharedApplication;
            app.InvokeOnMainThread(() =>
            {
                var snackbar = new TTG.TTGSnackbar
                {
                    Message = message,
                    Duration = TimeSpan.FromSeconds(1.5),
                    AnimationType = TTG.TTGSnackbarAnimationType.FadeInFadeOut
                };

                snackbar.BackgroundColor = Xamarin.Forms.Platform.iOS.ColorExtensions.ToUIColor(Theme.PrimaryColor.Color);
                snackbar.MessageLabel.TextColor = Xamarin.Forms.Platform.iOS.ColorExtensions.ToUIColor(Color.White);

                snackbar.Show();

                currentToast = new TTG.DisposableAction(
                    () => app.InvokeOnMainThread(() => snackbar.Dismiss())
                );
            });
        }

        public static bool ShowLoadingIndicatorNative
        {
            set
            {
                UIKit.UIApplication.SharedApplication.NetworkActivityIndicatorVisible = value;
            }
        }

        public void EnableRemoteNotifications()
        {
            UNUserNotificationCenter.Current.RequestAuthorization(UNAuthorizationOptions.Alert | UNAuthorizationOptions.Sound, (bool approved, NSError error) =>
            {
                if (!approved)
                    RemoteNotifiactionTokenResult(null);
            });
        }

        /*
        public class FilePickerImplementation : NSObject, IUIDocumentPickerDelegate
        {
            public void DidPickDocument(UIDocumentPickerViewController controller, NSUrl url)
            {
            }
        }
        */

        public static Task SaveFilePicker2(byte[] data, string filename)
        {
            return Task.CompletedTask;
        }

        class IosOpenFile : OpenFile
        {
            readonly NSUrl _url;

            public IosOpenFile(NSUrl url, string name, Stream stream) : base(name, stream)
            {
                _url = url;
            }

            public static IosOpenFile New(NSUrl url)
            {
                url.StartAccessingSecurityScopedResource();
                var stream = File.OpenRead(url.Path);

                return new IosOpenFile(url, Path.GetFileName(url.Path), stream);
            }

            public override void Dispose()
            {
                base.Dispose();
                _url.StopAccessingSecurityScopedResource();
            }
        }

        public static Task<OpenFile> OpenFilePicker2(params string[] extentions)
        {
			var completed = new TaskCompletionSource<OpenFile>(TaskCreationOptions.RunContinuationsAsynchronously);

            var docPicker = new UIDocumentPickerViewController(new string[] { "public.data" }, UIDocumentPickerMode.Open);
            //docPicker.Delegate = new FilePickerImplementation();

            docPicker.WasCancelled += (sender, wasCancelledArgs) =>
            {
                try
                {
                    completed.SetResult(new OpenFile());
                    docPicker.DismissViewController(true, null);
                }
                catch { }
            };

            docPicker.DidPickDocument += (sender, e) =>
            {
                try
                {
                    completed.SetResult(IosOpenFile.New(e.Url));
                    docPicker.DismissViewController(true, null);
                }
                catch (Exception ex)
                {
                    Log.HandleException(ex);
                }
            };

            docPicker.DidPickDocumentAtUrls += (sender, e) =>
            {
                try
                {
                    completed.SetResult(IosOpenFile.New(e.Urls[0]));
                    docPicker.DismissViewController(true, null);
                }
                catch(Exception ex)
                {
                    Log.HandleException(ex);
                }
            };

            var rootController = GetRootController();
            rootController.PresentViewController(docPicker, true, null);

            return completed.Task;
        }

        public static async Task OpenImagePicker(Func<ImageHandler, Task> success)
        {
            var status = PHPhotoLibrary.AuthorizationStatus;
            if (status == PHAuthorizationStatus.Denied || status == PHAuthorizationStatus.Restricted)
            {
                if (await Current.CurrentPage.ConfirmAsync("AuthorizePhoto"))
                {
                    UIKit.UIApplication.SharedApplication.OpenUrl(new Foundation.NSUrl(UIKit.UIApplication.OpenSettingsUrlString));
                }
            }

            var imagePicker = new UIImagePickerController
            {
                ModalPresentationStyle = UIModalPresentationStyle.CurrentContext,
                SourceType = UIImagePickerControllerSourceType.PhotoLibrary,
                MediaTypes = UIImagePickerController.AvailableMediaTypes(UIImagePickerControllerSourceType.PhotoLibrary)
            };

            imagePicker.FinishedPickingMedia += async (object sender, UIImagePickerMediaPickedEventArgs e) =>
            {

                try
                {
                    if (e.Info[UIImagePickerController.OriginalImage] is UIImage image)
                    {
                        var cgImage = image.CGImage;
                        var isOpaque = cgImage.AlphaInfo == CGImageAlphaInfo.None || cgImage.AlphaInfo == CGImageAlphaInfo.NoneSkipFirst || cgImage.AlphaInfo == CGImageAlphaInfo.NoneSkipLast;

                        // https://stackoverflow.com/questions/8915630/ios-uiimageview-how-to-handle-uiimage-image-orientation
                        if (image.Orientation != UIImageOrientation.Up)
                        {
                            UIGraphics.BeginImageContext(image.Size);
                            image.Draw(CGPoint.Empty);
                            image = UIGraphics.GetImageFromCurrentImageContext();
                            UIGraphics.EndImageContext();
                        }

                        var ih = new ImageHandler(image.ToSKBitmap());
                        if (isOpaque && !ih.IsOpaque) // don't know why skiasharp messes this up, but if we don't do it, we can't compress the files and they end up very big
                        {
                            var newIh = new ImageHandler(ih.Width, ih.Height, true);
                            await newIh.Blend(ih, 0, 0);

                            ih.Dispose();
                            ih = newIh;
                        }

                        if (success != null)
                            _ = success.Invoke(ih);
                    }
                }
                catch (Exception ex)
                {
                    Log.IgnoreException(ex);
                }

                imagePicker?.DismissViewController(true, null);
                imagePicker = null;
            };

            imagePicker.Canceled += (object sender, EventArgs e) =>
            {

                imagePicker?.DismissViewController(true, null);
                imagePicker = null;
            };

            // http://stackoverflow.com/questions/13350938/attempt-to-present-on-whose-view-is-not-in-the-window-hierarchy
            var rootController = GetRootController();
            rootController.PresentViewController(imagePicker, true, null);
        }

        public static void CopyToClipboard(string text)
        {
            try
            {
                var clipboard = UIPasteboard.General;
                clipboard.String = text;
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
                var clipboard = UIPasteboard.General;
                return Task.FromResult(clipboard.String);
            }
            catch (Exception ex)
            {
                Log.IgnoreException(ex);
            }
            return Task.FromResult<string>(null);
        }
    }
}