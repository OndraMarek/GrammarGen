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

        Console.WriteLine("Pro pokračování zadejte 1, pro konec jakoukoliv jinou klávesu.");
        ConsoleKeyInfo key = Console.ReadKey(true);
        if (key.KeyChar == '1')
        {
            PrintGrammar();
        }
        else
        {
            Environment.Exit(0);
        }
    }
}