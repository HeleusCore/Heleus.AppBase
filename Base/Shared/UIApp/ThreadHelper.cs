using System;
using System.Runtime.CompilerServices;

namespace Heleus.Apps.Shared
{
    public static class ThreadHelper
    {
        public static void ShowThreadId(string message = "", [CallerMemberName] string memberName = null, [CallerFilePath] string sourceFilePath = null, [CallerLineNumber] int sourceLineNumber = 0)
        {
            Console.WriteLine($"Thread Id: {System.Threading.Thread.CurrentThread.ManagedThreadId}, {message} (@{memberName}):{sourceFilePath}:{sourceLineNumber}");
        }
    }
}
