
bool running = true;
Console.WriteLine("running...");

while (running)
{
    string? line = Console.ReadLine();

    if (line is null or {Length: 0})
        continue;

    string[] words = line.Split(' ', StringSplitOptions.RemoveEmptyEntries);
    
    // run commands based on words
}