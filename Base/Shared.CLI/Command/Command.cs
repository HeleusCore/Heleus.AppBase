using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Heleus.Base;

namespace Heleus.Apps.Shared
{
    abstract class Command
    {
        static readonly Dictionary<string, Type> _commands = new Dictionary<string, Type>();

        public static void RegisterCommand<T>() where T : Command
        {
            var type = typeof(T);
            var field = type.GetField("CommandName");

            _commands[((string)field.GetValue(null)).ToLower()] = type;
        }

        public static async Task<int> RunCommand(string[] args, ArgumentsParser arguments)
        {
            var name = Tr.Get("App.FullName").Replace(" ", "").ToLower(); ;

            try
            {
                if (args.Length > 0 && _commands.TryGetValue(args[0].Trim(), out var type))
                {
                    var command = (Command)Activator.CreateInstance(type);
                    if (command != null)
                    {
                        if (command.Parse(arguments))
                        {
                            await command.Run();

                            if (command._succes)
                            {
                                if (!string.IsNullOrEmpty(command._succesMessage))
                                    Console.WriteLine(command._succesMessage);

                                return 0;
                            }

                            if (command._error)
                            {
                                if (!string.IsNullOrEmpty(command._errorMessage))
                                    Console.Error.WriteLine(command._errorMessage);

                                return -1;
                            }

                            Console.WriteLine("Internal Error.");
                            return -1;
                        }
                        else
                        {
                            if (command._error)
                            {
                                if (!string.IsNullOrEmpty(command._errorMessage))
                                {
                                    Console.Error.WriteLine(command._errorMessage);
                                    return -1;
                                }
                            }

                            var usageItems = command.GetUsageItems();
                            var commandUsage = new StringBuilder($"\nUsage: {name} {args[0]} [-options]\n" +
                                "\n" +
                                "Options:\n");

                            foreach (var item in usageItems)
                                commandUsage.Append($"  -{item.Key.PadRight(30)} {item.Value}\n");

                            Console.WriteLine(commandUsage);

                            return -1;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Log.HandleException(ex);
            }

            var usage = new StringBuilder(Tr.Get("App.Name") + " " + Tr.Get("App.Version") + "\n" +
              $"Usage: {name} [command] [-verbose] [-options]\n" +
              "\n" +
              "Commands:\n");

            foreach (var command in _commands)
                usage.Append($"  {command.Key.PadRight(30)} {command.Value.GetField("CommandDescription")?.GetValue(null)?.ToString()?.ToLower()}\n");

            Console.WriteLine(usage);

            return -1;
        }

        bool _succes;
        string _succesMessage;

        bool _error;
        string _errorMessage;

        protected void SetSuccess(string message)
        {
            _succes = true;
            _succesMessage = message;
        }

        protected void SetError(string message)
        {
            _error = true;
            _errorMessage = message;
        }

        abstract protected bool Parse(ArgumentsParser arguments);
        abstract protected List<KeyValuePair<string, string>> GetUsageItems();

        abstract protected Task Run();
    }
}
