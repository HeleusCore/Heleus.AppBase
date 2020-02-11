using System;
using System.IO;

namespace AppBuilder
{
    class MainClass
    {
        public static void Main(string[] args)
        {
            if(args.Length != 2)
            {
                Console.WriteLine("Usage: AppMaker (init|forceinit|images|version) targetdirectory");
                return;
            }

            var operation = args[0];
            var targetPath = new DirectoryInfo(args[1]);
            var infoPath = new DirectoryInfo(Path.Combine(targetPath.FullName, "AppInfo"));
            if (!infoPath.Exists)
            {
                Console.WriteLine($"AppInfo directory does not exist.");
                return;
            }

            var info = new Info(infoPath);
            var templatePath = new DirectoryInfo(Path.Combine(Directory.GetCurrentDirectory(), "..", "AppTemplate"));

            if (operation == "init" || operation == "forceinit")
            {
                if (Template.Copy(info, templatePath, targetPath, operation == "forceinit"))
                {
                    Images.Generate(infoPath, templatePath, targetPath);
                    Version.Update(infoPath, templatePath, targetPath);
                }
            }
            else if (operation == "images")
            {
                Images.Generate(infoPath, templatePath, targetPath);
            }
            else if (operation == "version")
            {
                Version.Update(infoPath, templatePath, targetPath);
            }
            else
            {
                Console.WriteLine("Usage: AppMaker (init|images|version) targetdirectory");
            }
        }
    }
}
