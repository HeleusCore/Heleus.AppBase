using System;
using System.Collections.Generic;
using System.IO;

namespace AppBuilder
{
    static class Template
    {
        static List<FileInfo> SearchFiles(DirectoryInfo directoryInfo, string searchPattern, string[] ignoreList = null)
        {
            var result = new List<FileInfo>();
            try
            {
                foreach (var file in directoryInfo.GetFiles(searchPattern))
                {
                    bool skip = false;
                    if (ignoreList != null)
                    {
                        foreach (var ignore in ignoreList)
                        {
                            if (file.FullName.Contains(ignore))
                            {
                                Console.WriteLine("Ignored " + file.FullName);
                                skip = true;
                                break;
                            }
                        }
                    }

                    if (!skip)
                        result.Add(file);
                }

                foreach (var directory in directoryInfo.GetDirectories())
                {
                    bool skip = false;
                    if (ignoreList != null)
                    {
                        foreach (var ignore in ignoreList)
                        {
                            if ((directory.FullName + Path.DirectorySeparatorChar).Contains(ignore))
                            {
                                Console.WriteLine("Ignored " + directory.FullName);
                                skip = true;
                                break;
                            }
                        }
                    }

                    if (!skip)
                        result.AddRange(SearchFiles(directory, searchPattern, ignoreList));
                }
            }
            catch (Exception excpt)
            {
                Console.WriteLine(excpt.Message);
            }

            return result;
        }

        public static bool Copy(Info info, DirectoryInfo templatePath, DirectoryInfo targetPath, bool force)
        {
            var modifiers = Modifier.Parse(info, templatePath);
            var files = SearchFiles(templatePath, "*.*", File.ReadAllLines(Path.Combine(templatePath.FullName, "ignore.txt")));

            foreach (var file in files)
            {
                var fullName = file.FullName;
                var replaces = new List<Tuple<string, string>>();
                foreach (var modifier in modifiers)
                {
                    if (fullName.Contains(modifier.Name))
                    {
                        fullName = fullName.Replace(modifier.Name, modifier.NewName);
                        replaces.AddRange(modifier.Replaces);
                    }
                }

                fullName = fullName.Replace(templatePath.FullName, targetPath.FullName);

                if (!force)
                {
                    if (File.Exists(fullName))
                    {
                        Console.WriteLine($"Stopping, template file already copied {fullName}.");
                        return false;
                    }
                }

                var directory = Path.GetDirectoryName(fullName);
                Directory.CreateDirectory(directory);
                Console.WriteLine($"Copy file {fullName}.");

                if (replaces.Count > 0)
                {
                    var text = File.ReadAllText(file.FullName);

                    foreach (var replace in replaces)
                        text = text.Replace(replace.Item1, replace.Item2);

                    File.WriteAllText(fullName, text);
                }
                else
                {
                    File.Copy(file.FullName, fullName, true);
                }
            }
            return true;
        }
    }
}
