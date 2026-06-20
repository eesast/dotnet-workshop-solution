using LogAnalyzer;
using LogParser.Visitors;

namespace LocalCli
{
    public static class Program
    {
        public static void Main(string[] args)
        {
            var analyzer = InputDirectory();
            if (analyzer is null)
            {
                return;
            }

            ChooseAction(analyzer);
        }

        private static LogFileAnalyzer? InputDirectory()
        {
            var analyzer = new LogFileAnalyzer();
            while (true)
            {
                Console.WriteLine("Please input directory containing log files:");
                var directory = Console.ReadLine();
                if (directory is null)
                {
                    return null;
                }
                try
                {
                    if (!analyzer.ChangeDirectory(directory))
                    {
                        Console.WriteLine("Directory not exists, please try again:");
                        continue;
                    }
                    break;
                }
                catch (ArgumentException)
                {
                    Console.WriteLine("Directory illegal, please try again:");
                    continue;
                }
            }
            return analyzer;
        }

        private static void ChooseAction(LogFileAnalyzer analyzer)
        {
            while (true)
            {
                Console.WriteLine();
                Console.WriteLine("""
                Please choose:
                1. Show log files.
                2. Analyze specified log files.
                3. Analyze all log files.
                4. Get log file analysis result.
                5. Change directory.
                6. Exit.
                """);
                Console.Write(">>> ");
                Console.Out.Flush();

                int choice = 0;
                var choiceStr = Console.ReadLine();
                if (choiceStr is null)
                {
                    return;
                }
                try
                {
                    choice = int.Parse(choiceStr);
                }
                catch (Exception)
                {
                    Console.WriteLine("Invalid input, please try again.");
                    continue;
                }

                var actions = new Dictionary<int, Action<LogFileAnalyzer>>
                {
                    { 1, ShowLogFiles },
                    { 2, AnalyzeFiles },
                    { 3, AnalyzeAll },
                    { 4, GetAnalysisResult }
                };
                switch (choice)
                {
                    case 1: case 2: case 3: case 4:
                        actions[choice](analyzer);
                        break;
                    case 5:
                        var newAnalyzer = InputDirectory();
                        if (newAnalyzer is null)
                        {
                            return;
                        }
                        analyzer = newAnalyzer;
                        break;
                    case 6:
                        return;
                    default:
                        Console.WriteLine("Invalid choice, please try again.");
                        break;
                }
            }
        }

        private static void ShowLogFiles(LogFileAnalyzer analyzer)
        {
            Console.WriteLine($"[{string.Join(", ", analyzer.GetLogFiles())}]");
        }

        private static int ReadDegreeOfParallelism()
        {
            while (true)
            {
                Console.WriteLine("Please input degree of parallelism:");
                var input = Console.ReadLine();
                if (input is null)
                {
                    Console.WriteLine("Invalid input, please try again.");
                    continue;
                }
                try
                {
                    int degree = int.Parse(input);
                    if (degree < 1)
                    {
                        Console.WriteLine("Degree of parallelism must be at least 1, please try again.");
                        continue;
                    }
                    return degree;
                }
                catch (Exception)
                {
                    Console.WriteLine("Invalid input, please try again.");
                    continue;
                }
            }
        }

        private static List<string> ReadFileNames()
        {
            Console.WriteLine("Please input log file names (comma separated):");
            var input = Console.ReadLine();
            if (input is null)
            {
                return new List<string>();
            }

            return input.Split(',').Select(name => name.Trim()).Where(name => !string.IsNullOrEmpty(name)).ToList();
        }

        private static void AnalyzeFiles(LogFileAnalyzer analyzer)
        {
            int degreeOfParallelism = ReadDegreeOfParallelism();
            var fileNames = ReadFileNames();
            analyzer.AnalyzeFiles(degreeOfParallelism, fileNames);
            Console.WriteLine($"Analysis completed: [{string.Join(", ", fileNames)}]");
        }

        private static void AnalyzeAll(LogFileAnalyzer analyzer)
        {
            int degreeOfParallelism = ReadDegreeOfParallelism();
            analyzer.AnalyzeAll(degreeOfParallelism);
            Console.WriteLine($"Analysis completed");
        }

        private static void GetAnalysisResult(LogFileAnalyzer analyzer)
        {
            Console.WriteLine("Please input log file name:");
            var fileName = Console.ReadLine();
            if (fileName is null || string.IsNullOrEmpty(fileName.Trim()))
            {
                Console.WriteLine("Empty input.");
                return;
            }
            fileName = fileName.Trim();
            if (analyzer.TryGetAnalysisResult(fileName, out var result))
            {
                if (result is null)
                {
                    Console.WriteLine("No analysis result for this file.");
                }
                else
                {
                    if (result.State == AnalysisState.NotAnalyzed)
                    {
                        Console.WriteLine($"File {fileName} has not been analyzed yet.");
                    }

                    Console.WriteLine($"Analysis result for {fileName}:");
                    if (result.State == AnalysisState.Succeeded)
                    {
                        var kvdumper = new KeyValueVisitor();
                        var kvresults = result.Entries.Select(entry => kvdumper.Dump(entry)).ToList();
                        Console.WriteLine(string.Join("\n", kvresults.Select(kvresult => string.Join(", ", kvresult.Select(kv => $"{kv.Key}: {kv.Value}")))));
                    }
                    else if (result.State == AnalysisState.Failed)
                    {
                        Console.WriteLine("Analysis failed: " + result.ErrorMessage);
                    }
                }
            }
            else
            {
                Console.WriteLine("File not found.");
            }
        }
    }
}
