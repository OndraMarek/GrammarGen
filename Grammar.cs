abstract class Grammar
{
    public HashSet<string> NonTerminals { get; set; } = [];
    public HashSet<string> Terminals { get; set; } = [];
    public string StartSymbol { get; set; } = "";

    public static readonly Random random = new();

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

    protected abstract string GenerateRandomWord(string symbol);

    protected abstract HashSet<string> GenerateRandomWords(string symbol, int count);

    public abstract void PrintGrammar();
}

class ContextFreeGrammar(string filePath) : Grammar(filePath)
{
    public Dictionary<string, List<string>> Rules { get; set; } = [];

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

                ValidateRule(left, right);

                if (!Rules.TryGetValue(left, out List<string>? value))
                {
                    value = [];
                    Rules[left] = value;
                }

                value.Add(right);
            }
        }
    }

    private void ValidateRule(string left, string right)
    {
        if (!NonTerminals.Contains(left))
        {
            throw new Exception("Neplatné pravidlo gramatiky.");
        }

        foreach (char c in right)
        {
            if (!NonTerminals.Contains(c.ToString()) && !Terminals.Contains(c.ToString()))
            {
                throw new Exception("Neplatné pravidlo gramatiky.");
            }
        }
    }

    protected override string GenerateRandomWord(string symbol)
    {
        if (!NonTerminals.Contains(symbol))
        {
            return symbol;
        }

        if (Rules.TryGetValue(symbol, out List<string>? value))
        {
            List<string> rules = value;
            string rule = rules[random.Next(rules.Count)];

            string word = "";
            for (int i = 0; i < rule.Length; i++)
            {
                string currentSymbol = rule[i].ToString();
                word += GenerateRandomWord(currentSymbol);
            }
            return word;
        }

        return symbol;
    }

    protected override HashSet<string> GenerateRandomWords(string symbol, int count)
    {
        HashSet<string> words = [];
        string word;

        for (int i = 0; i < count; i++)
        {
            do
            {
                word = GenerateRandomWord(symbol);
            } while (words.Contains(word));

            words.Add(word);
        }
        return words;
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
        HashSet<string> words = GenerateRandomWords(StartSymbol, 10);
        Console.WriteLine("Vygenerované řetězce:");
        foreach (string word in words)
        {
            Console.WriteLine("  " + word);
        }
    }
}

class MatrixGrammar(string filePath) : Grammar(filePath)
{
    public List<List<Tuple<string, string>>> Matrices { get; set; } = [];

    protected override void ParseRules(string rulesLine)
    {
        List<List<Tuple<string, string>>> matrices = [];
        string[] matricesRaw = rulesLine.Split('|', StringSplitOptions.RemoveEmptyEntries);

        foreach (string matrixRaw in matricesRaw)
        {
            List<Tuple<string, string>> matrix = [];
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
        HashSet<string> words = GenerateRandomWords(StartSymbol, 10);
        Console.WriteLine("Vygenerované řetězce:");
        foreach (string word in words)
        {
            Console.WriteLine("  " + word);
        }
    }
}