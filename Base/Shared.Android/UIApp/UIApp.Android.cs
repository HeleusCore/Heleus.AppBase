using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Android.Content;
using Android.Graphics;
using Heleus.Apps.Shared.Android;
using Heleus.Base;
using Heleus.Service.Push;
using SkiaSharp.Views.Android;
using Xamarin.Forms.Platform.Android;

namespace Heleus.Apps.Shared
{
    partial class UIApp 
    {
        void PlatformInit()
        {
            IsAndroid = true;

            LanguageString = Java.Util.Locale.Default.Language?.ToLower();
        }

        public static bool ShowLoadingIndicatorNative = false;

        public static string PlatformName
        {
            get
            {
                return Network.Client.PlatformName.ANDROID;
            }
        }

        public static BrokerType PushBrokerType => BrokerType.Firebase;

        static string info = "";
        public static string DeviceInfo
        {
            get
            {
                if (string.IsNullOrEmpty(info) || info == "-")
                {

                    try
                    {
                        string OS = global::Android.OS.Build.VERSION.Release;
                        string Device = global::Android.OS.Build.Model;
                        string Vendor = global::Android.OS.Build.Manufacturer;

                        info = Vendor + "/" + Device + "/" + OS;
                    }
                    catch (Exception ex)
                    {
                        Log.IgnoreException(ex);
                        info = "-";
                    }
                }
                return info;
            }
        }

        public static string CodedVersion
        {
            get
            {
                return MainActivity.Current.PackageManager.GetPackageInfo(MainActivity.Current.PackageName, 0).VersionName;
            }
        }

        public static void Share(string text)
        {
            var share = new Intent(Intent.ActionSend);
            share.SetType("text/plain");
            share.PutExtra(Intent.ExtraText, text);

            MainActivity.Current?.StartActivity(Intent.CreateChooser(share, Tr.Get("Share.Share")));
        }

        public const bool CanRate = true;

        public static void RateApp()
        {
            var context = MainActivity.Current;

            var uri = global::Android.Net.Uri.Parse("market://details?id=" + context.PackageName);
            var goToMarket = new Intent(Intent.ActionView, uri);
            // To count with Play market backstack, After pressing back button, 
            // to taken back to our application, we need to add following flags to intent. 
            goToMarket.AddFlags(ActivityFlags.NoHistory | ActivityFlags.NewDocument | ActivityFlags.MultipleTask);
            try
            {
                context.StartActivity(goToMarket);
            }
            catch (Exception ex)
            {
                Log.IgnoreException(ex);
                context.StartActivity(new Intent(Intent.ActionView, global::Android.Net.Uri.Parse("http://play.google.com/store/apps/details?id=" + context.PackageName)));
            }
        }

        public static void Toast(string message)
        {
            var toast = global::Android.Widget.Toast.MakeText(MainActivity.Current, message, global::Android.Widget.ToastLength.Long);
            try
            {
                //toast.View.SetBackgroundColor(Color.Black.MultiplyAlpha(0.5).ToAndroid());
                toast.View.SetBackgroundColor(Theme.SecondaryColor.Color.ToAndroid());
                var textView = (toast.View.FindViewById(global::Android.Resource.Id.Message) as global::Android.Widget.TextView);
                textView?.SetTextColor(Xamarin.Forms.Color.White.ToAndroid());
            }

            catch (Exception ex)
            {
                Log.IgnoreException(ex);
            }

            Xamarin.Forms.Device.StartTimer(TimeSpan.FromMilliseconds(350), () =>
            {
                toast.Show();
                return false;
            });
        }

        public void EnableRemoteNotifications()
        {
            MainActivity.Current?.EnableRemoteNotifications();
        }

        static Func<ImageHandler, Task> imagePickerSuccess;
        static TaskCompletionSource<OpenFile> filePickerResult;

        static byte[] saveFilePickerData;

        public static Task SaveFilePicker2(byte[] data, string name)
        {
            var intent = new Intent(Intent.ActionCreateDocument);
            intent.AddCategory(Intent.CategoryOpenable);

            if(!string.IsNullOrEmpty(name))
            {
                var ext = name.Split('.').Last();
                intent.SetType($"*/{ext}");
            }
            else
            {
                intent.SetType("*/*");
            }

            intent.PutExtra(Intent.ExtraTitle, name);

            saveFilePickerData = data;
            MainActivity.Current?.StartActivityForResult(Intent.CreateChooser(intent, ""), MainActivity.SaveFilePickerResultId);

            return Task.CompletedTask;
        }

        internal static void HandleSaveFilePicker(global::Android.Net.Uri uri)
        {
            try
            {
                if (saveFilePickerData != null)
                {
                    var output = MainActivity.Current.ContentResolver.OpenOutputStream(uri);

                    output.Write(saveFilePickerData);
                    output.Flush();
                    output.Close();
                }
            }
            catch(Exception ex)
            {
                Log.IgnoreException(ex);
            }
        }

        public static Task OpenImagePicker(Func<ImageHandler, Task> action)
        {
            imagePickerSuccess = action;

            var intent = new Intent(Intent.ActionGetContent);
            intent.SetType("image/*");
            intent.SetAction(Intent.ActionGetContent);

            MainActivity.Current?.StartActivityForResult(Intent.CreateChooser(intent, ""), MainActivity.ImagePickerResultId);

            return Task.CompletedTask;
        }

        public static Task<OpenFile> OpenFilePicker2(params string[] extentions)
        {
            try
            {
                filePickerResult?.SetResult(new OpenFile());
            }
            catch { }

            var intent = new Intent(Intent.ActionGetContent);
            if(extentions != null && extentions.Length == 1)
                intent.SetType($"*/{extentions[0]}");
            else
                intent.SetType("*/*");

            intent.SetAction(Intent.ActionGetContent);

			filePickerResult = new TaskCompletionSource<OpenFile>(TaskCreationOptions.RunContinuationsAsynchronously);

            MainActivity.Current?.StartActivityForResult(Intent.CreateChooser(intent, ""), MainActivity.FilePickerResultId);

            return filePickerResult.Task;
        }

        internal static void HandleFilePicker(Intent intent)
        {
            if (intent != null)
            {
                try
                {
                    var context = MainActivity.Current;
                    var uri = intent.Data;

                    var stream = context.ContentResolver.OpenInputStream(intent.Data);

                    // meh, but required as the InputStream isn't seekable
                    // should work for now
                    var memoryStream = new MemoryStream();
                    stream.CopyTo(memoryStream);
                    memoryStream.Position = 0;

                    filePickerResult?.SetResult(new OpenFile(uri.LastPathSegment, memoryStream));
                    filePickerResult = null;

                    return;
                }
                catch (Exception ex)
                {
                    Log.IgnoreException(ex);
                }
            }

            filePickerResult?.SetResult(new OpenFile());
            filePickerResult = null;
        }

        internal static async Task HandleImagePicker(global::Android.Net.Uri uri)
        {
            try
            {
                var context = MainActivity.Current;
                using (var sourceStream = context.ContentResolver.OpenInputStream(uri))
                {
                    var success = imagePickerSuccess;
                    if (success != null)
                    {
                        var androidBitmap = await BitmapFactory.DecodeStreamAsync(sourceStream);
                        if (androidBitmap != null)
                        {
                            var bitmap = androidBitmap.ToSKBitmap();
                            if (bitmap != null)
                            {
                                var ih = new ImageHandler(bitmap);

                                if (!androidBitmap.HasAlpha && !ih.IsOpaque) // meh
                                {
                                    var newIh = new ImageHandler(ih.Width, ih.Height, true);
                                    await newIh.Blend(ih, 0, 0);

                                    ih.Dispose();
                                    ih = newIh;
                                }

                                await success.Invoke(ih);
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Log.IgnoreException(ex);
            }
        }

        public static void CopyToClipboard(string text)
        {
            try
            {
                var clipboardManager = (ClipboardManager)MainActivity.Current.GetSystemService(Context.ClipboardService);
                clipboardManager.Text = text;
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
                var clipboardManager = (ClipboardManager)MainActivity.Current.GetSystemService(Context.ClipboardService);
                return Task.FromResult(clipboardManager.Text);
            }
            catch (Exception ex)
            {
                Log.IgnoreException(ex);
            }
            return Task.FromResult<string>(null);
        }
    }
}
