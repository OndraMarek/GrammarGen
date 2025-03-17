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
