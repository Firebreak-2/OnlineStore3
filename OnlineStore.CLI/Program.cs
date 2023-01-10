using OnlineStore.Core;

bool running = true;

#if DEBUG
Console.WriteLine("running...");
#endif

while (running)
{
    string? line = Console.ReadLine();

    if (line is null or {Length: 0})
        continue;

    string[] words = line.Split(' ', StringSplitOptions.RemoveEmptyEntries);

    switch (words)
    {
        case ["host"]:
        {
            Store.Initialize(true, 9999);
            break;
        }
        case ["client"]:
        {
            Store.Initialize(false, 9999);
            break;
        }
    }
}