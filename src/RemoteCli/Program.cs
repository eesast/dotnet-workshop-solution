using Google.Protobuf.WellKnownTypes;
using Grpc.Core;
using Grpc.Net.Client;
using LogAnalyzer.Grpc;
using Microsoft.Extensions.Logging;

namespace RemoteCli
{
    using LogAnalyzerAgentServiceClient = LogAnalyzerAgentService.LogAnalyzerAgentServiceClient;

    public class Program
    {
        static async Task Main(string[] args)
        {
            var address = args.FirstOrDefault()
                ?? Environment.GetEnvironmentVariable("LOG_ANALYZER_AGENT_ADDRESS")
                ?? "http://localhost:5000";
            Console.WriteLine($"Connecting to agent at {address}...");
            using var channel = GrpcChannel.ForAddress(address);
            var client = new LogAnalyzerAgentServiceClient(channel);
            _ = await client.PingAsync(new Empty());

            await ChooseAction(client);
        }

        private static async Task<bool> InputDirectory(LogAnalyzerAgentServiceClient client)
        {
            while (true)
            {
                Console.WriteLine("Please input directory containing log files:");
                var directory = Console.ReadLine();
                if (directory is null)
                {
                    return false;
                }
                var request = new ChangeDirectoryRequest()
                {
                    DirectoryPath = directory,
                };
                var response = await client.ChangeDirectoryAsync(request);
                if (!response.Status.Success)
                {
                    Console.WriteLine($"Error: {response.Status.Code}: {response.Status.Message}, please try again:");
                    continue;
                }
                break;
            }
            return true;
        }

        private static async Task ChooseAction(LogAnalyzerAgentServiceClient client)
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

                var actions = new Dictionary<int, Func<LogAnalyzerAgentServiceClient, Task>>
                {
                    { 1, ShowLogFiles },
                    { 2, AnalyzeFiles },
                    { 3, AnalyzeAll },
                    { 4, GetAnalysisResult }
                };
                switch (choice)
                {
                    case 1:
                    case 2:
                    case 3:
                    case 4:
                        await actions[choice](client);
                        break;
                    case 5:
                        var success = await InputDirectory(client);
                        if (!success)
                        {
                            return;
                        }
                        break;
                    case 6:
                        return;
                    default:
                        Console.WriteLine("Invalid choice, please try again.");
                        break;
                }
            }
        }

        private static async Task ShowLogFiles(LogAnalyzerAgentServiceClient client)
        {
            // var response = await client.GetLogFilesAsync(new Empty());
            var response = await client.GetLogFilesAsync(new Empty());
            if (response.Status.Success)
            {
                Console.WriteLine($"[{string.Join(", ", response.FileNames)}]");
            }
            else
            {
                Console.WriteLine($"Error: {response.Status.Code}: {response.Status.Message}");
            }
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

        private static async Task AnalyzeFiles(LogAnalyzerAgentServiceClient client)
        {
            int degreeOfParallelism = ReadDegreeOfParallelism();
            var fileNames = ReadFileNames();
            var request = new AnalyzeFilesRequest()
            {
                DegreeOfParallelism = degreeOfParallelism,
            };
            request.FileNames.AddRange(fileNames);
            var response = await client.AnalyzeFilesAsync(request);
            if (response.Status.Success)
            {
                Console.WriteLine($"Analysis completed: [{string.Join(", ", fileNames)}]");
            }
            else
            {
                Console.WriteLine($"Error: {response.Status.Code}: {response.Status.Message}");
            }
        }

        private static async Task AnalyzeAll(LogAnalyzerAgentServiceClient client)
        {
            int degreeOfParallelism = ReadDegreeOfParallelism();
            var request = new AnalyzeAllRequest()
            {
                DegreeOfParallelism = degreeOfParallelism,
            };
            var response = await client.AnalyzeAllAsync(request);
            if (response.Status.Success)
            {
                Console.WriteLine($"Analysis completed");
            }
            else
            {
                Console.WriteLine($"Error: {response.Status.Code}: {response.Status.Message}");
            }
        }

        private static async Task GetAnalysisResult(LogAnalyzerAgentServiceClient client)
        {
            Console.WriteLine("Please input log file name:");
            var fileName = Console.ReadLine();
            if (fileName is null || string.IsNullOrEmpty(fileName.Trim()))
            {
                Console.WriteLine("Empty input.");
                return;
            }
            fileName = fileName.Trim();

            using var call = client.GetAnalysisResult(new GetAnalysisResultRequest
            {
                FileName = fileName,
            });
            await foreach (var response in call.ResponseStream.ReadAllAsync())
            {
                if (response.Status.Success)
                {
                    switch (response.PayloadCase)
                    {
                        case GetAnalysisResultResponse.PayloadOneofCase.Header:
                            var header = response.Header;
                            if (header.State == AnalysisStateEnum.NotAnalyzed)
                            {
                                Console.WriteLine($"File {fileName} has not been analyzed yet.");
                            }
                            else if (header.State == AnalysisStateEnum.Failed)
                            {
                                Console.WriteLine("Analysis failed: " + header.ErrorMessage);
                            }
                            break;
                        case GetAnalysisResultResponse.PayloadOneofCase.LogEntry:
                            var kvresult = response.LogEntry.Fields;
                            Console.WriteLine(string.Join(", ", kvresult.Select(kv => $"{kv.Key}: {kv.Value}")));
                            break;
                        default:
                            Console.WriteLine($"Unknown payload type.");
                            break;
                    }
                }
                else
                {
                    Console.WriteLine($"Error: {response.Status.Code}: {response.Status.Message}");
                }
            }
        }
    }
}
