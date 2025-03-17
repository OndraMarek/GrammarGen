using System;

abstract class Grammar
{
    public HashSet<string> NonTerminals { get; set; } = [];
    public HashSet<string> Terminals { get; set; } = [];
    public string StartSymbol { get; set; } = "";
    public string Word { get; set; } = "";
    public HashSet<string> Words { get; set; } = [];

    public static readonly Random random = new Random();

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
        Word = StartSymbol;

        ParseRules(lines[3]);
    }

    protected abstract void ParseRules(string rulesLine);

    protected abstract string GenerateRandomWord(string symbol);

    protected abstract HashSet<string> GenerateRandomWords(string symbol, int count);

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

    protected override string GenerateRandomWord(string symbol)
    {
        if (!NonTerminals.Contains(symbol))
        {
            return symbol;
        }

        if (Rules.ContainsKey(symbol))
        {
            List<string> rules = Rules[symbol];
            string rule = rules[random.Next(rules.Count)];

            Word = "";
            for (int i = 0; i < rule.Length; i++)
            {
                string currentSymbol = rule[i].ToString();
                Word += GenerateRandomWord(currentSymbol);
            }
            return Word;
        }

        return symbol;
    }

    protected override HashSet<string> GenerateRandomWords(string symbol, int count)
    {
        for (int i = 0; i < count; i++)
        {
            do
            {
                Word = GenerateRandomWord(symbol);
            } while (Words.Contains(Word));

            Words.Add(Word);
        }
        return Words;
    }

    public override void PrintGrammar()
    {
        Console.WriteLine("Bezkontextová gramatika");
        Console.WriteLine("Neterminály: " + string.Join(", ", NonTerminals));
        Console.WriteLine("Terminály: " + string.Join(", ", Terminals));
        Console.WriteLine("Pravidla:");
        foreach (KeyValuePair<string, List<string>> rule in Rules)
        {
            Console.WriteLine($"  {rule.Key} -> {string.Join(" | ", rule.Value)}");
        }
        Console.WriteLine("Startovací symbol: " + StartSymbol);
        Words = GenerateRandomWords(StartSymbol, 10);
        Console.WriteLine("Vygenerované řetězce:");
        foreach (string word in Words)
        {
            Console.WriteLine("  " + word);
        }
    }
}

class MatrixGrammar : Grammar
{
    public List<List<Tuple<string, string>>> Matrices { get; set; } = new();

    public MatrixGrammar(string filePath) : base(filePath) { }

    protected override void ParseRules(string rulesLine)
    {
        List<List<Tuple<string, string>>> matrices = new List<List<Tuple<string, string>>>();
        string[] matricesRaw = rulesLine.Split('|', StringSplitOptions.RemoveEmptyEntries);

        foreach (string matrixRaw in matricesRaw)
        {
            List<Tuple<string, string>> matrix = new List<Tuple<string, string>>();
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
    protected override string GenerateRandomWord(string symbol)
    {
        //TODO: Implementace generování náhodného řetězce
        return "";
    }

    protected override HashSet<string> GenerateRandomWords(string symbol, int count)
    {
        //TODO: Implementace generování count náhodných řetězců
        return [];
    }

    public override void PrintGrammar()
    {
        Console.WriteLine("Maticová gramatika");
        Console.WriteLine("Neterminály: " + string.Join(", ", NonTerminals));
        Console.WriteLine("Terminály: " + string.Join(", ", Terminals));
        Console.WriteLine("Startovací symbol: " + StartSymbol);
        Console.WriteLine("Maticová pravidla:");

        int index = 1;
        foreach (List<Tuple<string, string>> matrix in Matrices)
        {
            Console.WriteLine($"  Matice {index}:");
            foreach (Tuple<string, string> rule in matrix)
            {
                Console.WriteLine($"    {rule.Item1} -> {rule.Item2}");
            }
            index++;
        }
        Word = GenerateRandomWord(StartSymbol);
        Console.WriteLine("Uložené slovo: " + Word);
        Words = GenerateRandomWords(StartSymbol, 10);
        Console.WriteLine("Vygenerované řetězce:");
        foreach (string word in Words)
        {
            Console.WriteLine("  " + word);
        }
    }
}