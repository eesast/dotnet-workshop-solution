using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Google.Protobuf.WellKnownTypes;
using Grpc.Core;
using LogAnalyzerClient.Helpers;
using LogAnalyzerClient.Models;
using LogAnalyzerClient.Services;
using LogAnalyzerRpc;
using LogAnalyzerRpc.Protos;
using LogParser.Visitors;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;

namespace LogAnalyzerClient.ViewModels
{
    using LogAnalyzerAgentServiceClient = LogAnalyzerAgentService.LogAnalyzerAgentServiceClient;

    public partial class MainViewModel : ViewModelBase
    {
        internal IDialogHelper DialogHelper { get; set; } = new NullDialogHelper();

        private LogAnalyzerAgentServiceClient? _client = null;

        public IReadOnlyList<string> SelectedFiles { get; set; } = new List<string>();

        [ObservableProperty]
        private string _greeting = "Welcome to Avalonia!";

        [ObservableProperty]
        private string _directoryPath = "";

        [ObservableProperty]
        private string _degreeOfParallelismText = "1";

        [ObservableProperty]
        private string _currentAddress = "";

        private static class ConnectStatusString
        {
            public const string NOT_CONNECTED = "Not connected.";
            public const string CONNECTING = "Connecting...";
            public const string CONNECTED = "Connected.";
            public const string CONNECT_FAILED = "Connect failed.";
        }
        [ObservableProperty]
        private string _connectStatus = ConnectStatusString.NOT_CONNECTED;

        [ObservableProperty]
        private ObservableCollection<LogFileItem> _logFiles = new();

        [ObservableProperty]
        private LogFileItem? _selectedLogFile = null;

        [ObservableProperty]
        private ObservableCollection<LogFields> _resultEntries = new();

        [RelayCommand]
        private async Task ConnectAsync()
        {
            var address = await DialogHelper.ShowConnectDialogAsync(CurrentAddress);
            if (address is null)
            {
                // Do nothing if the user cancels the dialog
            }
            else if (string.IsNullOrEmpty(address.Trim()))
            {
                await DialogHelper.ShowMessageDialogAsync("Error", "Address cannot be empty.");
            }
            else
            {
                try
                {
                    ConnectStatus = ConnectStatusString.CONNECTING;
                    _client = AppService.ClientFactory.CreateClient(address);
                    await _client.PingAsync(new Empty());
                    CurrentAddress = address;
                    ConnectStatus = ConnectStatusString.CONNECTED;
                    LogFiles.Clear();
                }
                catch (Exception ex)
                {
                    ConnectStatus = ConnectStatusString.CONNECT_FAILED;
                    await DialogHelper.ShowMessageDialogAsync("Error", $"Failed to connect to agent: {ex.Message}");
                    ConnectStatus = ConnectStatusString.NOT_CONNECTED;
                }
            }
        }

        private async Task WithClientNotNull(Func<Task> action)
        {
            if (_client is null)
            {
                await DialogHelper.ShowMessageDialogAsync("Error",
                    "Agent is not connected. Please connect to an agent first.");
            }
            else
            {
                try
                {
                    await action();
                }
                catch (Exception ex)
                {
                    await DialogHelper.ShowMessageDialogAsync("Error", $"Error occurred: {ex.Message}");
                }
            }
        }

        [RelayCommand]
        private async Task ChangeDirectoryAsync()
        {
            await WithClientNotNull(async() =>
            {
                var request = new ChangeDirectoryRequest()
                {
                    DirectoryPath = DirectoryPath,
                };
                var response = await _client!.ChangeDirectoryAsync(request);
                if (!response.Status.Success)
                {
                    await DialogHelper.ShowMessageDialogAsync("Error",
                        $"{response.Status.Code}: {response.Status.Message}");
                }
                await RefreshAsync();
            });
        }

        [RelayCommand]
        private async Task RefreshAsync()
        {
            await WithClientNotNull(async () =>
            {
                var response = await _client!.GetLogFilesAsync(new Empty());
                if (!response.Status.Success)
                {
                    await DialogHelper.ShowMessageDialogAsync("Error",
                        $"{response.Status.Code}: {response.Status.Message}");
                }
                else
                {
                    LogFiles.Clear();
                    foreach (var name in response.FileNames)
                    {
                        LogFiles.Add(new LogFileItem(name));
                    }
                }
            });
        }

        private int? ReadDegreeOfParallelism()
        {
            try
            {
                int degreeOfParallelism = int.Parse(DegreeOfParallelismText);
                if (degreeOfParallelism < 0)
                {
                    DialogHelper.ShowMessageDialogAsync("Error",
                        "Degree of parallelism cannot be negative. Please enter a valid integer.");
                }
                else
                {
                    return degreeOfParallelism;
                }
            }
            catch (Exception)
            {
                DialogHelper.ShowMessageDialogAsync("Error",
                    "Invalid degree of parallelism. Please enter a valid integer.");
            }
            return null;
        }

        private async Task AnalyzeFilesAsync(IReadOnlyList<string> fileNames)
        {
            var degreeOfParallelism = ReadDegreeOfParallelism();
            if (degreeOfParallelism is null)
            {
                return;
            }

            if (fileNames.Count == 0)
            {
                await DialogHelper.ShowMessageDialogAsync("Error",
                    "No log files selected. Please select at least one log file.");
                return;
            }

            var request = new AnalyzeFilesRequest()
            {
                DegreeOfParallelism = degreeOfParallelism.Value,
            };
            request.FileNames.AddRange(fileNames);
            var response = await _client!.AnalyzeFilesAsync(request);
            if (response.Status.Success)
            {
                await DialogHelper.ShowMessageDialogAsync("Analysis Started",
                    "Log files have been started to analyzed.");
            }
            else
            {
                await DialogHelper.ShowMessageDialogAsync("Error",
                        $"{response.Status.Code}: {response.Status.Message}");
            }
        }

        [RelayCommand]
        private async Task AnalyzeSelectedFilesAsync()
        {
            await AnalyzeFilesAsync(SelectedFiles);
        }

        [RelayCommand]
        private async Task AnalyzeRightClickedFileAsync()
        {
            if (SelectedLogFile is null)
            {
                await DialogHelper.ShowMessageDialogAsync("Error",
                    "No log file selected. Please select a log file first.");
            }
            else
            {
                await AnalyzeFilesAsync(new List<string> { SelectedLogFile.FileName });
            }
        }

        [RelayCommand]
        private async Task AnalyzeAllAsync()
        {
            var degreeOfParallelism = ReadDegreeOfParallelism();
            if (degreeOfParallelism is null)
            {
                return;
            }

            var request = new AnalyzeAllRequest()
            {
                DegreeOfParallelism = degreeOfParallelism.Value,
            };
            var response = await _client!.AnalyzeAllAsync(request);
            if (response.Status.Success)
            {
                await DialogHelper.ShowMessageDialogAsync("Analysis Started",
                    "Log files have been started to analyzed.");
            }
            else
            {
                await DialogHelper.ShowMessageDialogAsync("Error",
                    $"{response.Status.Code}: {response.Status.Message}");
            }
        }

        [RelayCommand]
        private async Task GetAnalysisResultAsync()
        {
            var selected = SelectedLogFile;
            if (selected is null)
            {
                await DialogHelper.ShowMessageDialogAsync("Error",
                    "No log file selected. Please select a log file first.");
            }
            else
            {
                var fileName = selected.FileName;
                await WithClientNotNull(async () =>
                {
                    using var call = _client!.GetAnalysisResult(new GetAnalysisResultRequest
                    {
                        FileName = fileName,
                    });
                    ResultEntries.Clear();
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
                                        ResultEntries.Add(new LogFields(-1, new List<LogFieldItem>(),
                                            $"File {fileName} has not been analyzed yet."));
                                    }
                                    else if (header.State == AnalysisStateEnum.Failed)
                                    {
                                        ResultEntries.Add(new LogFields(-1, new List<LogFieldItem>(),
                                            "Analysis failed: " + header.ErrorMessage));
                                    }
                                    else if (header.State == AnalysisStateEnum.Succeeded)
                                    {
                                        ResultEntries.Add(new LogFields(-1, new List<LogFieldItem>(),
                                            $"File: {header.FileName}; Worder ID: {header.WorkerId}"));
                                    }
                                    break;
                                case GetAnalysisResultResponse.PayloadOneofCase.LogEntry:
                                    var entry = GrpcTypeConverter.ConvertFromGrpc(response.LogEntry);
                                    var dumper = new KeyValueVisitor();
                                    var kvresult = dumper.Dump(entry);
                                    var logfields = kvresult.Select(kv => new LogFieldItem(Key: kv.Key, Value: kv.Value)).ToList();
                                    ResultEntries.Add(new LogFields(entry.LineNo, logfields, null));
                                    break;
                                default:
                                    await DialogHelper.ShowMessageDialogAsync("Error",
                                        $"Unknown payload type: {response.PayloadCase}");
                                    break;
                            }
                        }
                        else
                        {
                            await DialogHelper.ShowMessageDialogAsync("Error",
                                $"{response.Status.Code}: {response.Status.Message}");
                        }
                    }
                });
            }
        }

        [RelayCommand]
        private async Task AboutAsync()
        {
            await DialogHelper.ShowMessageDialogAsync("About",
                """
                LogAnalyzerClient
                EESAST Software Center
                Solution to https://github.com/eesast/dotnet-workshop
                Source Code (internal reference, not visible to the public):
                    https://github.com/eesast/dotnet-workshop-solution
                """);
        }
    }
}
