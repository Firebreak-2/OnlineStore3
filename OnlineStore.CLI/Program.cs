using System.Diagnostics;
using AdvancedCommands;
using OnlineStore.Core;

namespace OnlineStore.CLI;

public static class Program
{
    public static CommandRunner Runner = new();
    public static void Main(string[] args)
    {
        Runner.AddDefaultTypeInterpretters();
        Runner.AddCommandsUsingAttribute();
        
        #if DEBUG
        Console.WriteLine("running...");
        #endif

        while (true)
        {
            string line = Console.ReadLine() ?? "";

            Runner.Run(line).TryUse(Console.WriteLine, Console.WriteLine);
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