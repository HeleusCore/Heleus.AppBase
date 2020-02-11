using System;
using System.Globalization;
using System.IO;
using System.Threading.Tasks;
using Gtk;
using Heleus.Base;
using Heleus.Service.Push;
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

        public static string PlatformName => Network.Client.PlatformName.GTK;

        public BrokerType PushBrokerType => BrokerType.None;

        public static string CodedVersion { get; private set; } = string.Empty;

        public static string DeviceInfo => Environment.OSVersion.ToString();

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
            UIApp.Run(async () =>
			{
				try
				{
					var page = Current?.CurrentPage;
					if (page != null)
					{
						var frame = new Xamarin.Forms.Frame
                        {
                            InputTransparent = true,
							Padding = new Thickness(10),
							BackgroundColor = Theme.SecondaryColor.DefaultColor,
							Opacity = 0
						};

						AbsoluteLayout.SetLayoutFlags(frame, AbsoluteLayoutFlags.PositionProportional);
						AbsoluteLayout.SetLayoutBounds(frame, new Rectangle(0.5, 0.9, AbsoluteLayout.AutoSize, AbsoluteLayout.AutoSize));
						frame.Content = new ExtLabel
						{
							Text = text,
							TextColor = Theme.TextColor.DefaultColor,
							FontStyle = Theme.RowFont,
							HorizontalTextAlignment = TextAlignment.Center,
							InputTransparent = true
						};

						page.RootLayout.Children.Add(frame);
						await frame.FadeTo(1);
						await Task.Delay(1500);
						await frame.FadeTo(0);
						page.RootLayout.Children.Remove(frame);
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

        public static Task SaveFilePicker2(byte[] data, string filename)
        {
            var dialog = new FileChooserDialog(string.Empty, GtkApp.Window, FileChooserAction.Save, Tr.Get("Common.Cancel"), ResponseType.Cancel, Tr.Get("Common.Open"), ResponseType.Accept)
            {
                SelectMultiple = false
            };

            if(!string.IsNullOrEmpty(filename))
                dialog.SetFilename(filename);

            if (dialog.Run() == (int)ResponseType.Accept)
            {
                try
                {
                    var filePath = dialog.Filename;
                    File.WriteAllBytes(filePath, data);

                    dialog.Destroy();

                    return Task.CompletedTask;
                }
                catch (Exception ex)
                {
                    Log.IgnoreException(ex);
                }

            }

            dialog.Destroy();
            return Task.CompletedTask;
        }

        public static Task<OpenFile> OpenFilePicker2(params string[] extensions)
        {
            var dialog = new FileChooserDialog(string.Empty, GtkApp.Window, FileChooserAction.Open, Tr.Get("Common.Cancel"), ResponseType.Cancel, Tr.Get("Common.Open"), ResponseType.Accept)
            {
                SelectMultiple = false
            };

            if (dialog.Run() == (int)ResponseType.Accept)
            {
                try
                {
                    var filePath = dialog.Filename;
                    var stream = File.OpenRead(filePath);

                    dialog.Destroy();

                    return Task.FromResult(new OpenFile(Path.GetFileName(filePath), stream));
                }
                catch (Exception ex)
                {
                    Log.IgnoreException(ex);
                }

            }

            dialog.Destroy();
            return Task.FromResult(new OpenFile());
        }

        public static async Task OpenImagePicker(Func<ImageHandler, Task> successAsync)
		{
            var dialog = new FileChooserDialog(string.Empty, GtkApp.Window, FileChooserAction.Open, Tr.Get("Common.Cancel"), ResponseType.Cancel, Tr.Get("Common.Open"), ResponseType.Accept)
            {
                SelectMultiple = false
            };

            if (dialog.Run() == (int)ResponseType.Accept)
            {
                try
                {
                    var image = await ImageHandler.FromFile(dialog.Filename);
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

            dialog.Destroy();
        }

        static readonly Gdk.Atom clipboardAtom = Gdk.Atom.Intern("CLIPBOARD", false);

        public static void CopyToClipboard(string text)
        {
            try
            {
                var clipboard = Clipboard.Get(clipboardAtom);
                clipboard.Text = text;
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
                var clipboard = Clipboard.Get(clipboardAtom);
                return Task.FromResult(clipboard.WaitForText());
            }
            catch (Exception ex)
            {
                Log.IgnoreException(ex);
            }
            return Task.FromResult<string>(null);
        }
    }
}
