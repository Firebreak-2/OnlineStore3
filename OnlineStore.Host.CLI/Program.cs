﻿using System.Diagnostics;
using AdvancedCommands;
using OnlineStore.Core;

namespace OnlineStore.CLI;

public static class Program
{
    public static void Main(string[] args)
    {
        DoHost();
    }

    public static void DoHost()
    { 
        Task.Run(async () => await Backend.SafeHostInit(1030)).GetAwaiter().GetResult();
    }
}