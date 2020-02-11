using System;
using System.Threading.Tasks;
using Heleus.Base;

namespace Heleus.Apps.Shared
{
    public static class CLI
    {
        public static async Task<int> Run(string[] args)
        {
            Tr.Initalize("en");

            var arguments = new ArgumentsParser(args);

            // silence output
            Log.ShowSystemDiagnostics = false;
            Log.ShowConsoleOutput = arguments.IsSet("verbose");

            var app = new UIApp();

            return Command.RunCommand(args, arguments).Result;
        }
    }
}
