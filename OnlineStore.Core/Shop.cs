namespace OnlineStore.Core;

public static class Shop
{
    private static string Path => $"{Directory.GetCurrentDirectory()}/db.csv";
    public static Dictionary<string, int> AccountBalances = new();

    public static readonly ShopItem[] Items = {
        new("Bread", 3),
        new("Sandwich", 7),
        new("Water Bottle", 1),
        new("Expensive Water Bottle", 2),
        new("Free Money", -5),
    };

    public static void EnsureDatabase()
    {
        if (!File.Exists(Path))
            File.Create(Path).Close();
    }

    public static void SaveDatabase()
    {
        File.WriteAllLines(Path, AccountBalances.Select(x => $"{x.Key},{x.Value}"));
    }

    public static void LoadDatabase()
    {
        var lines = File.ReadAllLines(Path);
        foreach (var line in lines)
        {
            var values = line.Split(',');
            AccountBalances[values[0]] = int.Parse(values[1]);
        }
    }
}