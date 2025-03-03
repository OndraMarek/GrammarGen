class Grammar
{
    public HashSet<string> NonTerminals { get; set; } = new();
    public HashSet<string> Terminals { get; set; } = new();
    public Dictionary<string, List<string>> Rules { get; set; } = new();
    public string StartSymbol { get; set; } = "";

    public Grammar(string filePath)
    {
        if (!File.Exists(filePath))
        {
            throw new FileNotFoundException("Soubor s gramatikou neexistuje.");
        }

        string[] lines = File.ReadAllLines(filePath);
        if (lines.Length < 4)
        {
            throw new Exception("Neplatný formát souboru.");
        }

        NonTerminals = new HashSet<string>(lines[0].Split(';'));
        Terminals = new HashSet<string>(lines[1].Split(';'));
        StartSymbol = lines[3].Trim();

        foreach (string rule in lines[2].Split(';'))
        {
            string[] parts = rule.Split("->");
            if (parts.Length == 2)
            {
                string left = parts[0].Trim();
                string right = parts[1].Trim();

                if (!Rules.ContainsKey(left))
                {
                    Rules[left] = new List<string>();
                }
                Rules[left].Add(right);
            }
        }
    }

    public void PrintGrammar()
    {
        Console.WriteLine("Neterminály: " + string.Join(", ", NonTerminals));
        Console.WriteLine("Terminály: " + string.Join(", ", Terminals));
        Console.WriteLine("Pravidla:");
        foreach (KeyValuePair<string, List<string>> rule in Rules)
        {
            Console.WriteLine($"  {rule.Key} -> {string.Join(" | ", rule.Value)}");
        }
        Console.WriteLine("Startovací symbol: " + StartSymbol);
    }
}

class Program
{
    static void Main()
    {
        string filePath = "grammar.txt";

        if (!File.Exists(filePath))
        {
            Console.WriteLine("Soubor s gramatikou neexistuje.");
            return;
        }

        Grammar grammar = new Grammar(filePath);
        grammar.PrintGrammar();
    }
}
