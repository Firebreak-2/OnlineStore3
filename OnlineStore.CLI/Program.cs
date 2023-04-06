using System.Diagnostics;
using AdvancedCommands;
using OnlineStore.Core;

namespace OnlineStore.CLI;

public static class Program
{
    public static void Main(string[] args)
    {
        CommandRunner runner = new();
        runner.AddDefaultTypeInterpretters();
        runner.AddCommandsUsingAttribute();
        
        #if DEBUG
        Console.WriteLine("running...");
        #endif

        while (true)
        {
            string line = Console.ReadLine() ?? "";

            runner.Run(line).TryUse(Console.WriteLine, Console.WriteLine);
        }
    }

    [AdvancedCommand]
    public static void Host()
    { 
        Task.Run(async () => await Store.Initialize(true, 1030)).GetAwaiter().GetResult();
    }
    
    [AdvancedCommand]
    public static void Client()
    { 
        Task.Run(async () => await Store.Initialize(false, 1030)).GetAwaiter().GetResult();
    }

    [AdvancedCommand]
    public static void Exit()
    {
        Process.GetCurrentProcess().Kill();
    }
}