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