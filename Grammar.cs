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
        TargetLength = 0;

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