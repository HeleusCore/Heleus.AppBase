using System;
using System.IO;

namespace AppBuilder
{
    static class Version
    {
        static void Process(DirectoryInfo templatePath, DirectoryInfo targetPath, string version, int build)
        {
            var imageLines = File.ReadAllLines(Path.Combine(templatePath.FullName, "versions.txt"));
            foreach (var line in imageLines)
            {
                if (string.IsNullOrWhiteSpace(line))
                    continue;

                if (line.StartsWith("//", StringComparison.Ordinal))
                    continue;

                var parts = line.Split(';');
                var fileName = parts[0].Trim();
                var updateType = parts[1].Trim();

                var filePath = Path.Combine(targetPath.FullName, fileName);

                var text = File.ReadAllText(filePath);

                VersionUpdate update = null;
                if (updateType == PListVersionUpdate.UpdateType)
                    update = new PListVersionUpdate();
                else if (updateType == AndroidManifestVersionUpdate.UpdateType)
                    update = new AndroidManifestVersionUpdate();
                else if (updateType == AssemblyVersionUpdate.UpdateType)
                    update = new AssemblyVersionUpdate();
                else if (updateType == AppxmanifestVersionUpdate.UpdateType)
                    update = new AppxmanifestVersionUpdate();
                else if (updateType == AppVersionUpdate.UpdateType)
                    update = new AppVersionUpdate();
                else if (updateType == BuildVersionUpdate.UpdateType)
                    update = new BuildVersionUpdate();

                update.Setup(text, version, build);
                text = update.GetUpdatedText();

                Console.WriteLine($"Update version in {filePath}.");
                File.WriteAllText(filePath, text);
            }
        }

        public static void Update(DirectoryInfo infoPath, DirectoryInfo templatePath, DirectoryInfo targetPath)
        {
            var versionString = File.ReadAllText(Path.Combine(infoPath.FullName, "version.txt"));
            var idx = versionString.LastIndexOf('.');

            var version = versionString.Substring(0, idx);
            var build = int.Parse(versionString.Substring(idx + 1));

            Process(templatePath, targetPath, version, build);
        }
    }
}
