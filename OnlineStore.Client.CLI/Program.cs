using System.Diagnostics;
using AdvancedCommands;
using OnlineStore.Core;

namespace OnlineStore.CLI;

public static class Program
{
    public static CommandRunner Runner = new();
    public static void Main(string[] args)
    {
        DoClient();
    }

    public static void DoClient()
    { 
        Task.Run(async () => await Store.Initialize(false, 1030)).GetAwaiter().GetResult();
    }
}