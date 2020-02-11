using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Text;

namespace Heleus.Apps.Shared
{
    public static class SchemeRegistration
    {
        // https://www.meziantou.net/2017/04/18/registering-an-application-to-a-uri-scheme-using-net
        public static void RegisterUriScheme(string UriScheme, string FriendlyName)
        {
            using (var key = Registry.CurrentUser.CreateSubKey("SOFTWARE\\Classes\\" + UriScheme))
            {
                var applicationLocation = typeof(UIApp).Assembly.Location;

                key.SetValue("", "URL:" + FriendlyName);
                key.SetValue("URL Protocol", "");

                using (var defaultIcon = key.CreateSubKey("DefaultIcon"))
                {
                    defaultIcon.SetValue("", applicationLocation + ",1");
                }

                using (var commandKey = key.CreateSubKey(@"shell\open\command"))
                {
                    commandKey.SetValue("", "\"" + applicationLocation + "\" \"%1\"");
                }
            }
        }
    }
}
