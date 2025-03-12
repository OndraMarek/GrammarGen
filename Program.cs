abstract class Grammar
{
    public HashSet<string> NonTerminals { get; set; } = new();
    public HashSet<string> Terminals { get; set; } = new();
    public string StartSymbol { get; set; } = "";

    public Grammar(string filePath)
    {
        if (!File.Exists(filePath))
        {
            throw new FileNotFoundException("Soubor s gramatikou neexistuje.");
        }

        string[] lines = File.ReadAllLines(filePath);
        if (lines.Length < 5)
        {
            throw new Exception("Neplatný formát souboru.");
        }

        NonTerminals = new HashSet<string>(lines[1].Split(';', StringSplitOptions.RemoveEmptyEntries));
        Terminals = new HashSet<string>(lines[2].Split(';', StringSplitOptions.RemoveEmptyEntries));
        StartSymbol = lines[4].Trim();

        ParseRules(lines[3]);
    }

    protected abstract void ParseRules(string rulesLine);

    public abstract void PrintGrammar();
}

class ContextFreeGrammar : Grammar
{
    public Dictionary<string, List<string>> Rules { get; set; } = new();

    public ContextFreeGrammar(string filePath) : base(filePath) { }

    protected override void ParseRules(string rulesLine)
    {
        string[] rules = rulesLine.Split(';', StringSplitOptions.RemoveEmptyEntries);
        foreach (string rule in rules)
        {
            string[] parts = rule.Split("->", StringSplitOptions.RemoveEmptyEntries);
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

    public override void PrintGrammar()
    {
        Console.WriteLine("Bezkontextová gramatika:");
        Console.WriteLine("Neterminály: " + string.Join(", ", NonTerminals));
        Console.WriteLine("Terminály: " + string.Join(", ", Terminals));
        Console.WriteLine("Pravidla:");
        foreach (var rule in Rules)
        {
            Console.WriteLine($"  {rule.Key} -> {string.Join(" | ", rule.Value)}");
        }
        Console.WriteLine("Startovací symbol: " + StartSymbol);
    }
}

class MatrixGrammar : Grammar
{
    public List<List<Tuple<string, string>>> Matrices { get; set; } = new();

    public MatrixGrammar(string filePath) : base(filePath) { }

    protected override void ParseRules(string rulesLine)
    {
        var matrices = new List<List<Tuple<string, string>>>();
        string[] matricesRaw = rulesLine.Split('|', StringSplitOptions.RemoveEmptyEntries);

        foreach (string matrixRaw in matricesRaw)
        {
            var matrix = new List<Tuple<string, string>>();
            string[] rules = matrixRaw.Split(';', StringSplitOptions.RemoveEmptyEntries);

            foreach (string rule in rules)
            {
                string[] parts = rule.Split("->", StringSplitOptions.RemoveEmptyEntries);
                if (parts.Length == 2)
                {
                    string left = parts[0].Trim();
                    string right = parts[1].Trim();
                    matrix.Add(new Tuple<string, string>(left, right));
                }
            }

            if (matrix.Count > 0)
            {
                matrices.Add(matrix);
            }
        }

        Matrices = matrices;
    }

    public override void PrintGrammar()
    {
        Console.WriteLine("Maticová gramatika:");
        Console.WriteLine("Neterminály: " + string.Join(", ", NonTerminals));
        Console.WriteLine("Terminály: " + string.Join(", ", Terminals));
        Console.WriteLine("Startovací symbol: " + StartSymbol);
        Console.WriteLine("Maticová pravidla:");

        int index = 1;
        foreach (var matrix in Matrices)
        {
            Console.WriteLine($"  Matice {index}:");
            foreach (var rule in matrix)
            {
                Console.WriteLine($"    {rule.Item1} -> {rule.Item2}");
            }
            index++;
        }
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

        Grammar grammar;
        string type = File.ReadLines(filePath).First().Trim();

        switch (type)
        {
            case "CFG":
                grammar = new ContextFreeGrammar(filePath);
                break;
            case "MG":
                grammar = new MatrixGrammar(filePath);
                break;
            default:
                Console.WriteLine("Neznámý typ gramatiky.");
                return;
        }

        grammar.PrintGrammar();
    }
}
