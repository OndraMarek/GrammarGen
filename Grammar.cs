abstract class Grammar
{
    public HashSet<string> NonTerminals { get; set; } = [];
    public HashSet<string> Terminals { get; set; } = [];
    public string StartSymbol { get; set; } = "";

    public List<string> ?Derivation { get; set; } = [];

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

    public List<string>? GenerateRandomWord(int maxLength)
    {
        int randomLength = random.Next(1, maxLength + 1);
        List<string> derivationSteps = [StartSymbol];

        return DFS(StartSymbol, randomLength, derivationSteps) ? derivationSteps : null;
    }

    public List<string>? GenerateWordOfLength(int targetLength)
    {
        List<string> derivationSteps = [StartSymbol];

        return DFS(StartSymbol, targetLength, derivationSteps) ? derivationSteps : null;
    }

    private bool DFS(string currentWord, int targetLength, List<string> derivationSteps)
    {
        if (currentWord.Length > targetLength) return false;
        if (currentWord.Length == targetLength && currentWord.All(c => Terminals.Contains(c.ToString())))
        {
            return true;
        }

        List<string> presentNonTerminals = NonTerminals.Where(currentWord.Contains).ToList();

        presentNonTerminals = presentNonTerminals.OrderBy(_ => random.Next()).ToList();

        foreach (string nt in presentNonTerminals)
        {
            int index = currentWord.IndexOf(nt);
            if (index == -1) continue;

            if (Rules.TryGetValue(nt, out List<string>? rules))
            {
                List<string> shuffledRules = rules.OrderBy(_ => random.Next()).ToList();

                foreach (string rule in shuffledRules)
                {
                    string newWord = currentWord.Substring(0, index) + rule + currentWord.Substring(index + nt.Length);
                    derivationSteps.Add(newWord);

                    if (DFS(newWord, targetLength, derivationSteps))
                    {
                        return true;
                    }

                    derivationSteps.RemoveAt(derivationSteps.Count - 1);
                }
            }
        }

        return false;
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

        Console.Write("Zadejte požadovanou délku slova (0 pro náhodné): ");
        if (int.TryParse(Console.ReadLine(), out int length))
        {
            if (length == 0)
            {
                Derivation = GenerateRandomWord(20);
            }
            else
            {
                Derivation = GenerateWordOfLength(length);
            }

            if (Derivation != null)
            {
                Console.WriteLine("Derivační sekvence:");
                Console.WriteLine(string.Join(" => ", Derivation));
            }
            else
            {
                Console.WriteLine("Pro zadanou délku nelze vygenerovat žádné slovo.");
            }
        }
        else
        {
            Console.WriteLine("Neplatný vstup.");
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
    }
}