using System;
using System.Collections;
using System.IO;
using System.Reflection;
using Gtk;
using Heleus.Base;
using Xamarin.Forms;
using Xamarin.Forms.Platform.GTK;
using Xamarin.Forms.Platform.GTK.Helpers;

namespace Heleus.Apps.Shared
{
    class GtkApp
    {
        public static bool IsFlatpak { get; private set; }
        public static DirectoryInfo ResourceDirectory { get; private set; }
        public static FormsWindow Window { get; private set; }

        static void ShowMessage(string title, string message)
        {
            Dialog dialog = null;
            try
            {
                var label = new Gtk.Label(message);
                dialog = new Dialog(title, null, DialogFlags.DestroyWithParent | DialogFlags.Modal, ResponseType.Ok);
                dialog.VBox.Add(label);
                dialog.ShowAll();

                dialog.Run();
            }
            finally
            {
                if (dialog != null)
                    dialog.Destroy();
            }
        }

        public static void Main(string[] args)
        {
            Log.LogLevel = LogLevels.Trace;

            IsFlatpak = (Environment.GetEnvironmentVariable("ISFLATPAK") == "1");

            Console.WriteLine("Is Flatpak: " + IsFlatpak);
            if (IsFlatpak)
                ResourceDirectory = new DirectoryInfo("/app/bin/Resources");
            else
                ResourceDirectory = new DirectoryInfo("Resources");

            Gtk.Application.Init();

            if (PlatformHelper.GetGTKPlatform() == GTKPlatform.Linux)
            {
                var fonts = new string[] { "fa-brands-400.ttf", "fa-light-300.ttf", "fa-regular-400.ttf", "fa-solid-900.ttf" };
                foreach (var font in fonts)
                {
                    var fi = new FileInfo(Path.Combine(ResourceDirectory.FullName, "Fonts", font));
                    if (!FontConfig.AddFontFromFile(fi))
                    {
                        Log.Warn($"Could not add font {fi.FullName}.");
                    }
                }
            }

            Forms.Init();

            var app = new UIApp();
            Window = new FormsWindow();

            Window.LoadApplication(app);
            Window.SetApplicationTitle(Tr.Get("App.FullName"));

            var icon = EmbeddedResource.GetEmbeddedResource<GtkApp>("application.png");
            if (icon != null)
                Window.Icon = new Gdk.Pixbuf(icon);

            Window.Show();

            var scheme = Tr.Get("App.Scheme");
            if (!string.IsNullOrEmpty(scheme))
            {
                foreach (var arg in args)
                {
                    if (arg.StartsWith(Tr.Get("App.Scheme"), StringComparison.Ordinal))
                    {
                        UIApp.PublishSchemeRequest(arg);
                    }
                }
            }

            Gtk.Application.Run();
        }
    }
}
