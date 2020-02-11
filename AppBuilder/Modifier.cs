using System;
using System.Collections.Generic;
using System.IO;

namespace AppBuilder
{
    sealed class Modifier
    {
        public string Name
        {
            get;
            private set;
        }
        public string NewName
        {
            get;
            private set;
        }

        public List<Tuple<string, string>> Replaces
        {
            get;
            private set;
        } = new List<Tuple<string, string>>();

        public static List<Modifier> Parse(Info info, DirectoryInfo templatePath)
        {
            var result = new List<Modifier>();

            var modifyLines = File.ReadAllLines(Path.Combine(templatePath.FullName, "modify.txt"));
            foreach (var line in modifyLines)
            {
                if (string.IsNullOrWhiteSpace(line))
                    continue;

                if (line.StartsWith("//", StringComparison.Ordinal))
                    continue;

                var modifier = new Modifier();
                var parts = info.GetModifiedText(line).Split(';');

                var name = parts[0].Split(new string[] { "=>" }, StringSplitOptions.None);
                modifier.Name = name[0].Trim();
                if (name.Length > 1)
                    modifier.NewName = name[1].Trim();
                else
                    modifier.NewName = modifier.Name;

                for (int i = 1; i < parts.Length; i++)
                {
                    if (string.IsNullOrWhiteSpace(parts[i]))
                        continue;

                    var item = parts[i].Split(new string[] { "=>" }, StringSplitOptions.None);
                    modifier.Replaces.Add(new Tuple<string, string>(item[0].Trim(), item[1].Trim()));
                }
                result.Add(modifier);
            }

            return result;
        }
    }
}
