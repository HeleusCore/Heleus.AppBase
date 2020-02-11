using System;
using AppKit;

namespace Heleus.Apps.Shared
{
    partial class UIApp
    {
        public NSMenu MainMenu
        {
            get;
            private set;
        }

        public NSMenu AppMenu
        {
            get;
            private set;
        }

        public NSMenu EditMenu
        {
            get;
            private set;
        }

        public NSMenu WindowMenu
        {
            get;
            private set;
        }

        public NSMenu HelpMenu
        {
            get;
            private set;
        }

        public void SetupMainMenu()
        {
            MainMenu = new NSMenu();
            var appMenuItem = new NSMenuItem(Tr.Get("App.Name"));
            MainMenu.AddItem(appMenuItem);

            AppMenu = new NSMenu();
            appMenuItem.Submenu = AppMenu;

            AppMenu.AddItem(new NSMenuItem(Tr.Get("AboutPage.Title"), "", (sender, e) =>
            {
                var cp = UIApp.Current.CurrentPage;
                if (cp.GetType() != typeof(AboutPage))
                    cp.Navigation.PushAsync(new AboutPage());
            }));

            AppMenu.AddItem(NSMenuItem.SeparatorItem);

            AppMenu.AddItem(new NSMenuItem(Tr.Get("SettingsPage.Title") + "…", ",", (sender, e) =>
            {
                var cp = UIApp.Current.CurrentPage;
                if (cp.GetType() != typeof(SettingsPage))
                    cp.Navigation.PushAsync(new SettingsPage());
            }));

            AppMenu.AddItem(NSMenuItem.SeparatorItem);

            NSApplication.SharedApplication.ServicesMenu = new NSMenu("Services");
            AppMenu.AddItem(new NSMenuItem(Tr.Get("Menu.Services")) { Submenu = NSApplication.SharedApplication.ServicesMenu });

            AppMenu.AddItem(NSMenuItem.SeparatorItem);

            AppMenu.AddItem(new NSMenuItem(Tr.Get("Menu.Hide", Tr.Get("Common.AppName")), new ObjCRuntime.Selector("hide:"), "h"));
            AppMenu.AddItem(new NSMenuItem(Tr.Get("Menu.HideOthers"), new ObjCRuntime.Selector("hideOtherApplications:"), "h") { KeyEquivalentModifierMask = NSEventModifierMask.AlternateKeyMask });
            AppMenu.AddItem(new NSMenuItem(Tr.Get("Menu.ShowAll"), new ObjCRuntime.Selector("unhideAllApplications:"), ""));

            AppMenu.AddItem(NSMenuItem.SeparatorItem);
            AppMenu.AddItem(new NSMenuItem(Tr.Get("Menu.Quit", Tr.Get("Common.AppName")), new ObjCRuntime.Selector("terminate:"), "q"));


            var editMenuItem = new NSMenuItem("Edit");
            MainMenu.AddItem(editMenuItem);

            EditMenu = new NSMenu(Tr.Get("Menu.Edit"));
            editMenuItem.Submenu = EditMenu;

            EditMenu.AddItem(new NSMenuItem(Tr.Get("Menu.Undo"), new ObjCRuntime.Selector("undo:"), "z"));
            EditMenu.AddItem(new NSMenuItem(Tr.Get("Menu.Redo"), new ObjCRuntime.Selector("redo:"), "Z"));

            EditMenu.AddItem(NSMenuItem.SeparatorItem);

            EditMenu.AddItem(new NSMenuItem(Tr.Get("Menu.Cut"), new ObjCRuntime.Selector("cut:"), "x"));
            EditMenu.AddItem(new NSMenuItem(Tr.Get("Menu.Copy"), new ObjCRuntime.Selector("copy:"), "c"));
            EditMenu.AddItem(new NSMenuItem(Tr.Get("Menu.Paste"), new ObjCRuntime.Selector("paste:"), "v"));
            EditMenu.AddItem(new NSMenuItem(Tr.Get("Menu.Delete"), new ObjCRuntime.Selector("delete:"), ""));
            EditMenu.AddItem(new NSMenuItem(Tr.Get("Menu.SelectAll"), new ObjCRuntime.Selector("selectAll:"), "a"));


            var windowMenuItem = new NSMenuItem("Window");
            MainMenu.AddItem(windowMenuItem);

            WindowMenu = new NSMenu(Tr.Get("Menu.Window"));
            windowMenuItem.Submenu = WindowMenu;

            WindowMenu.AddItem(new NSMenuItem(Tr.Get("Menu.Minimize"), new ObjCRuntime.Selector("performMiniaturize:"), "m"));

            WindowMenu.AddItem(new NSMenuItem(Tr.Get("Menu.Zoom"), new ObjCRuntime.Selector("performZoom:"), ""));

            WindowMenu.AddItem(NSMenuItem.SeparatorItem);
            WindowMenu.AddItem(new NSMenuItem(Tr.Get("Menu.BringAllToFront"), new ObjCRuntime.Selector("arrangeInFront:"), ""));


            var helpMenuItem = new NSMenuItem("Help");
            MainMenu.AddItem(helpMenuItem);

            HelpMenu = new NSMenu(Tr.Get("Menu.Help"));
            helpMenuItem.Submenu = HelpMenu;


            HelpMenu.AddItem(new NSMenuItem(Tr.Get("Menu.Website", Tr.Get("App.FullName")), "", (sender, e) =>
            {
                UIApp.OpenUrl(new Uri(Tr.Get("App.Link")));
            }));

            HelpMenu.AddItem(NSMenuItem.SeparatorItem);

            if (Tr.Get("Link.ReportIssue", out string issuelink))
            {
                HelpMenu.AddItem(new NSMenuItem(Tr.Get("Common.ReportIssue"), "", (sender, e) =>
                {
                    UIApp.OpenUrl(new Uri(issuelink));
                }));

                if (Tr.Get("Link.RequestFeature", out string featureLink))
                {
                    HelpMenu.AddItem(new NSMenuItem(Tr.Get("Common.RequestFeature"), "", (sender, e) =>
                    {
                        UIApp.OpenUrl(new Uri(featureLink));
                    }));
                }

                HelpMenu.AddItem(NSMenuItem.SeparatorItem);
            }

            if (Tr.Get("Link.Medium", out string mediumLink))
            {
                HelpMenu.AddItem(new NSMenuItem(Tr.Get("Menu.Medium"), "", (sender, e) =>
                {
                    UIApp.OpenUrl(new Uri(mediumLink));
                }));
            }

            if (Tr.Get("Link.Github", out string githubLink))
            {
                HelpMenu.AddItem(new NSMenuItem(Tr.Get("Menu.Github"), "", (sender, e) =>
                {
                    UIApp.OpenUrl(new Uri(githubLink));
                }));
            }

            if (Tr.Get("Link.Twitter", out string twitterLink))
            {
                HelpMenu.AddItem(new NSMenuItem(Tr.Get("Menu.Twitter"), "", (sender, e) =>
                {
                    UIApp.OpenUrl(new Uri(twitterLink));
                }));
            }

            if (Tr.Get("Link.Reddit", out string redditLink))
            {
                HelpMenu.AddItem(new NSMenuItem(Tr.Get("Menu.Reddit"), "", (sender, e) =>
                {
                    UIApp.OpenUrl(new Uri(redditLink));
                }));
            }

            if (Tr.Get("Link.Facebook", out string facebookLink))
            {
                HelpMenu.AddItem(new NSMenuItem(Tr.Get("Menu.Facebook"), "", (sender, e) =>
                {
                    UIApp.OpenUrl(new Uri(facebookLink));
                }));
            }

            if (CanRate)
            {
                HelpMenu.AddItem(NSMenuItem.SeparatorItem);

                HelpMenu.AddItem(new NSMenuItem(Tr.Get("Menu.Rate", Tr.Get("App.Name")), "", (sender, e) =>
                {
                    RateApp();
                }));
            }
        }

        public void SetMainMenu()
        {
            NSApplication.SharedApplication.Menu = MainMenu;
        }
    }
}
