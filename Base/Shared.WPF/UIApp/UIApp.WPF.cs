using Heleus.Base;
using Heleus.Service.Push;
using Microsoft.Win32;
using System;
using System.Globalization;
using System.IO;
using System.Threading.Tasks;
using System.Windows;
using Xamarin.Forms;

namespace Heleus.Apps.Shared
{
	public partial class UIApp
	{
        public void PlatformInit()
		{
            LanguageString = CultureInfo.CurrentUICulture.TwoLetterISOLanguageName.ToLower();

            var version = System.Reflection.Assembly.GetExecutingAssembly().GetName().Version;
            CodedVersion = version.Major + "." + version.Minor + "." + version.Build;
        }

        public static string PlatformName => Network.Client.PlatformName.WPF;

        public static string CodedVersion { get; private set; } = string.Empty;

        public static string DeviceInfo => Environment.OSVersion.ToString();

        public BrokerType PushBrokerType => BrokerType.None;

        public void EnableRemoteNotifications()
		{
		}

		public static void Share(string shareText)
		{
		}

        public const bool CanRate = false;

		public static void RateApp()
		{
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
                            Padding = new Xamarin.Forms.Thickness(10),
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
                            HorizontalTextAlignment = Xamarin.Forms.TextAlignment.Center,
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


        public static bool ShowLoadingIndicatorNative
		{
			set
			{
			}
		}

		public static async Task SaveImageToPhotoLibrary(byte[] data, string comment)
		{
            await Task.Delay(0);
		}

		public static async Task OpenImagePicker(Func<ImageHandler, Task> action)
		{
            var openFileDialog = new OpenFileDialog
            {
                Filter = "Images|*.png;*.jpg;*.jpeg|All (*.*)|*.*"
            };
            if (openFileDialog.ShowDialog() == true)
            {
                try
                {
                    if (action != null)
                    {
                        var ih = await ImageHandler.FromFile(openFileDialog.FileName);
                        if (ih != null)
                            await action.Invoke(ih);
                    }
                }
                catch(Exception ex)
                {
                    Log.IgnoreException(ex);
                }
            }
        }

        public static Task<OpenFile> OpenFilePicker2(params string[] extensions)
        {
            var openFileDialog = new OpenFileDialog
            {
                Filter = "All|*.*"
            };

            if (openFileDialog.ShowDialog() == true)
            {
                try
                {
                    var fileName = openFileDialog.FileName;
                    var stream = File.OpenRead(fileName);

                    return Task.FromResult(new OpenFile(Path.GetFileName(fileName), stream));
                }
                catch (Exception ex)
                {
                    Log.IgnoreException(ex);
                }
            }

            return Task.FromResult(new OpenFile());
        }

        public static Task SaveFilePicker2(byte[] data, string fileName)
        {
            var saveFileDialog = new SaveFileDialog
            {
                FileName = fileName
            };

            if (saveFileDialog.ShowDialog() == true)
            {
                try
                {
                    File.WriteAllBytes(saveFileDialog.FileName, data);
                }
                catch (Exception ex)
                {
                    Log.IgnoreException(ex);
                }
            }

            return Task.CompletedTask;
        }

        public static void CopyToClipboard(string text)
        {
            Clipboard.SetText(text);
        }

        public static Task<string> CopyFromClipboard()
        {
            return Task.FromResult(Clipboard.GetText());
        }
    }
}
