using System;
using System.IO;
using System.Text;

namespace AppBuilder
{
    sealed class Info
    {
        class JsonInfo
        {
            public string fullname = String.Empty;
            public string name = String.Empty;
            public string description = String.Empty;
            public string developer = String.Empty;
            public string domain = String.Empty;
            public string link = String.Empty;
            public string package = String.Empty;

            public string primarycolor = String.Empty;
            public string secondarycolor = String.Empty;
        }

        readonly JsonInfo _info;

        public string FullName { get => _info.fullname; }
        public string Name { get => _info.name; }
        public string Description { get => _info.description; }
        public string Developer { get => _info.developer; }
        public string Domain { get => _info.domain; }
        public string Link { get => _info.link; }
        public string Package { get => _info.package; }

        public readonly Color PrimaryColor;
        public readonly Color SecondaryColor;

        public Info(DirectoryInfo infoPath)
        {
            _info = Newtonsoft.Json.JsonConvert.DeserializeObject<JsonInfo>(File.ReadAllText(Path.Combine(infoPath.FullName, "info.json")));
            PrimaryColor = new Color(_info.primarycolor);
            SecondaryColor = new Color(_info.secondarycolor);
        }

        public string GetModifiedText(string text)
        {
            var builder = new StringBuilder(text);

            builder.Replace("$fullname", FullName);
            builder.Replace("$name", Name);
            builder.Replace("$exec", FullName.Replace(" ", "") );
            builder.Replace("$description", Description);
            builder.Replace("$developer", Developer);
            builder.Replace("$domain", Domain);
            builder.Replace("$link", Link);
            builder.Replace("$package", Package);

            builder.Replace("$scheme", FullName.Replace(" ", "").ToLower());

            builder.Replace("$primarycolorhex", PrimaryColor.HexValue);
            builder.Replace("$secondarycolorhex", SecondaryColor.HexValue);
            builder.Replace("$primarycolordoubler", PrimaryColor.DoubleValueR);
            builder.Replace("$primarycolordoubleg", PrimaryColor.DoubleValueG);
            builder.Replace("$primarycolordoubleb", PrimaryColor.DoubleValueB);

            builder.Replace("$primarycolor", PrimaryColor.StringValue);
            builder.Replace("$secondarycolor", SecondaryColor.StringValue);

            return builder.ToString();
        }
    }
}
