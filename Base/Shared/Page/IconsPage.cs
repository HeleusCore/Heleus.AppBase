using System;
using System.Collections.Generic;

namespace Heleus.Apps.Shared
{
    public class IconsPage : StackPage
    {
        public IconsPage() : base("IconsPage")
        {
            var fields = typeof(Icons).GetFields();
            var icons = new List<KeyValuePair<string, char>>();
            foreach(var field in fields)
            {
                if(field.FieldType == typeof(char))
                {
                    var icon = (char)field.GetValue(null);
                    var name = field.Name;

                    icons.Add(new KeyValuePair<string, char>(name, icon));
                }
            }

            icons.Sort((a, b) => string.Compare(a.Key, b.Key, StringComparison.Ordinal));

            foreach(var icon in icons)
            {
                AddTextRow(icon.Key).SetDetailViewIcon(icon.Value);
            }
        }
    }
}
