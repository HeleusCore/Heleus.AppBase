using System;

namespace AppBuilder
{
    abstract class VersionUpdate
    {
        protected static string InnerQuoteReplace(string text, string key, string newVersion)
        {
            return InnerQuoteReplace(0, text, key, newVersion);
        }

        protected static string InnerQuoteReplace(int startIndex, string text, string key, string newVersion)
        {
            var tagIndex = text.IndexOf(key, startIndex, StringComparison.Ordinal);
            if (tagIndex > 0)
            {
                var strStart = text.IndexOf('"', tagIndex) + 1;
                var strEnd = text.IndexOf('"', strStart);

                return text.Substring(0, strStart) + newVersion + text.Substring(strEnd);
            }
            return text;
        }

        public string Text { get; private set; }
        public string Version { get; private set; }
        public int Build { get; private set; }

        public void Setup(string text, string version, int build)
        {
            Text = text;
            Version = version;
            Build = build;
        }

        public abstract string GetUpdatedText();
    }

    sealed class PListVersionUpdate : VersionUpdate
    {
        public const string UpdateType = "plist";

        static string Replace(string text, string tag, string newVersion)
        {
            var tagIndex = text.IndexOf(tag, StringComparison.Ordinal);
            if (tagIndex > 0)
            {
                var strStart = text.IndexOf("<string>", tagIndex, StringComparison.Ordinal) + 8;
                var strEnd = text.IndexOf("</string>", strStart, StringComparison.Ordinal);

                return text.Substring(0, strStart) + newVersion + text.Substring(strEnd);
            }
            return text;
        }

        public override string GetUpdatedText()
        {
            var result = Replace(Text, "CFBundleShortVersionString", Version);
            return Replace(result, "CFBundleVersion", Build.ToString());
        }
    }

    sealed class AndroidManifestVersionUpdate : VersionUpdate
    {
        public const string UpdateType = "androidmanifest";

        public override string GetUpdatedText()
        {
            var result = InnerQuoteReplace(Text, "android:versionName", Version);
            return InnerQuoteReplace(result, "android:versionCode", Build.ToString());
        }
    }

    sealed class AssemblyVersionUpdate : VersionUpdate
    {
        public const string UpdateType = "assembly";

        public override string GetUpdatedText()
        {
            return InnerQuoteReplace(Text, "AssemblyVersion", Version + "." + Build);
        }
    }

    sealed class AppxmanifestVersionUpdate : VersionUpdate
    {
        public const string UpdateType = "appxmanifest";

        public override string GetUpdatedText()
        {
            return InnerQuoteReplace(Text.IndexOf("Identity", StringComparison.Ordinal), Text, "Version", Version + ".0");
        }
    }

    sealed class BuildVersionUpdate : VersionUpdate
    {
        public const string UpdateType = "build";

        public override string GetUpdatedText()
        {
            return InnerQuoteReplace(Text, "_VERSION", Version);
        }
    }

    sealed class AppVersionUpdate : VersionUpdate
    {
        public const string UpdateType = "app";

        static string Replace(string text, string newVersion)
        {
            var tagIndex = text.IndexOf("App.Version:", StringComparison.Ordinal);
            if (tagIndex > 0)
            {
                var strStart = tagIndex + 12;
                var strEnd = text.IndexOf('\n', strStart);

                return text.Substring(0, strStart) + newVersion + text.Substring(strEnd);
            }
            return text;
        }

        public override string GetUpdatedText()
        {
            return Replace(Text, " " + Version + " (build " + Build + ")");
        }
    }
}
