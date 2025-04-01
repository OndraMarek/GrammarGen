abstract class Grammar
{
    public HashSet<string> NonTerminals { get; set; } = [];
    public HashSet<string> Terminals { get; set; } = [];
    public string StartSymbol { get; set; } = "";
    public List<string> ?Derivation { get; set; } = [];
    public static readonly Random random = new();
    public int TargetLength { get; set; } = 0;
    public int MaxDepth { get; set; } = 100;
    public int MaxLength { get; set; } = 100;

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

    protected int GetUserInput()
    {
        Console.Write("Zadejte požadovanou délku slova (maximálně 100, 0 pro náhodné): ");
        if (int.TryParse(Console.ReadLine(), out int length))
        {
            if (length < 0 || length > MaxLength)
            {
                Console.WriteLine("Neplatná délka. Délka musí být mezi 0 a 100.");
                GetUserInput();
            }
            else return length;
        }
        else
        {
            Console.WriteLine("Neplatný vstup.");
            GetUserInput();
        }
        return length;
    }

    protected void PrintDerivation(int length)
    {
        if (length == 0)
        {
            Derivation = GenerateRandomWord();
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

    protected bool IsValidLength(string word)
    {
        return TargetLength == 0 || word.Length == TargetLength;
    }

    protected bool IsTerminalWord(string word)
    {
        return word.All(c => Terminals.Contains(c.ToString()));
    }

    protected abstract void ParseRules(string rulesLine);

    public List<string>? GenerateRandomWord()
    {
        Derivation = [StartSymbol];

        DFS(StartSymbol, 0);

        return Derivation.Last().All(c => Terminals.Contains(c.ToString())) ? Derivation : null;
    }

    public List<string>? GenerateWordOfLength(int targetLength)
    {
        Derivation = [StartSymbol];
        TargetLength = targetLength;

        DFS(StartSymbol, 0);

        return Derivation.Last().Length == TargetLength && IsTerminalWord(Derivation.Last()) ? Derivation : null;
    }

    protected abstract void DFS(string currentWord, int depth);

    protected abstract void PrintRules();

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

    protected override void DFS(string currentWord, int depth = 0)
    {
        if (depth > MaxDepth)
        {
            return;
        }

        if (IsValidLength(currentWord) && IsTerminalWord(currentWord))
        {
            return;
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

                    if (TargetLength != 0 && newWord.Length > Math.Min(TargetLength, MaxLength))
                    {
                        continue;
                    }

                    if (newWord.Length > MaxLength)
                    {
                        continue;
                    }

                    Derivation.Add(newWord);

                    DFS(newWord, depth + 1);

                    if (IsValidLength(Derivation.Last()) && IsTerminalWord(Derivation.Last()))
                    {
                        return;
                    }

                    Derivation.RemoveAt(Derivation.Count - 1);
                }
            }
        }
    }

    protected override void PrintRules()
    {
        Console.WriteLine("Pravidla P:");
        foreach (KeyValuePair<string, List<string>> rule in Rules)
        {
            Console.WriteLine($"  {rule.Key} -> {string.Join(" | ", rule.Value)}");
        }
    }

    public override void PrintGrammar()
    {
        Console.WriteLine($"Bezkontextová gramatika G=(N,T,P,{StartSymbol})");
        Console.WriteLine("Neterminály N: " + string.Join(", ", NonTerminals));
        Console.WriteLine("Terminály T: " + string.Join(", ", Terminals));
        PrintRules();
        Console.WriteLine("Startovací symbol: " + StartSymbol);

        int length = GetUserInput();

        PrintDerivation(length);
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

    protected override void DFS(string currentWord, int depth = 0)
    {
        if (depth > MaxDepth)
        {
            return;
        }

        if (IsValidLength(currentWord) && IsTerminalWord(currentWord))
        {
            return;
        }

        List<int> matrixIndices = Enumerable.Range(0, Matrices.Count).ToList();
        matrixIndices = matrixIndices.OrderBy(_ => random.Next()).ToList();

        foreach (int matrixIndex in matrixIndices)
        {
            List<Tuple<string, string>> matrix = Matrices[matrixIndex];
            string tempWord = currentWord;
            bool matrixApplicable = true;

            foreach (Tuple<string, string> rule in matrix)
            {
                if (!tempWord.Contains(rule.Item1))
                {
                    matrixApplicable = false;
                    break;
                }
            }

            if (matrixApplicable)
            {
                string newWord = tempWord;

                foreach (Tuple<string, string> rule in matrix)
                {
                    int index = newWord.IndexOf(rule.Item1);
                    if (index == -1)
                    {
                        matrixApplicable = false;
                        break;
                    }
                    newWord = newWord.Substring(0, index) + rule.Item2 + newWord.Substring(index + rule.Item1.Length);
                }

                if (TargetLength != 0 && newWord.Length > Math.Min(TargetLength, MaxLength))
                {
                    continue;
                }

                if (newWord.Length > MaxLength)
                {
                    continue;
                }

                Derivation.Add(newWord);

                DFS(newWord, depth + 1);

                if (IsValidLength(Derivation.Last()) && IsTerminalWord(Derivation.Last()))
                {
                    return;
                }

                Derivation.RemoveAt(Derivation.Count - 1);
            }
        }

        return;
    }

    protected override void PrintRules()
    {
        Console.WriteLine("Maticová pravidla M:");

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

    public override void PrintGrammar()
    {
        Console.WriteLine($"Maticová gramatika G=(N,T,M,{StartSymbol})");
        Console.WriteLine("Neterminály N: " + string.Join(", ", NonTerminals));
        Console.WriteLine("Terminály T: " + string.Join(", ", Terminals));
        PrintRules();
        Console.WriteLine("Startovací symbol: " + StartSymbol);
        
        int length = GetUserInput();

        PrintDerivation(length);
    }
}