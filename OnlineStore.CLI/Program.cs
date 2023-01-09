
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
    
    // run commands based on words
}