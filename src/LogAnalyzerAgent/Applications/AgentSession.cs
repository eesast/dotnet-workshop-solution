using Google.Protobuf.WellKnownTypes;
using Grpc.Core;
using LogAnalyzer;
using LogAnalyzerRpc.Protos;
using LogAnalyzerRpc;
using LogParser.Visitors;

namespace LogAnalyzerAgent.Applications
{
    public class AgentSession
    {
        private readonly LogFileAnalyzer _analyzer;
        private readonly ILogger _logger;

        public AgentSession(LogFileAnalyzer analyzer, ILoggerFactory loggerFactory)
        {
            _analyzer = analyzer;
            _logger = loggerFactory.CreateLogger<AgentSession>();
        }

        private static OperationStatusMessage CreateInternalErrorOperationStatus(Exception ex)
        {
            return new OperationStatusMessage()
            {
                Success = false,
                Code = AgentErrorCode.InternalError,
                Message = $"An error occurred while retrieving agent status: {ex.Message}",
            };
        }

        private static OperationStatusMessage CreateNoErrorOperationStatus()
        {
            return new OperationStatusMessage()
            {
                Success = true,
                Code = AgentErrorCode.NoAgentError,
                Message = "",
            };
        }

        public Task<Empty> Ping(Empty empty, CancellationToken cancellationToken)
        {
            return Task.FromResult(new Empty());
        }

        public Task<GetAgentStatusResponse> GetAgentStatus(Empty empty, CancellationToken cancellationToken)
        {
            var response = new GetAgentStatusResponse();
            try
            {
                response.HasDirectory = _analyzer.HasDirectory;
                response.CurrentDirectory = _analyzer.CurrentDirectory ?? "";
                response.IsAnalyzing = _analyzer.IsAnalyzing;
                response.Status = CreateNoErrorOperationStatus();
            }
            catch (Exception ex)
            {
                response.Status = CreateInternalErrorOperationStatus(ex);
                _logger.LogError(ex, "An error occurred while retrieving agent status.");
            }
            return Task.FromResult(response);
        }

        public Task<ChangeDirectoryResponse> ChangeDirectory(ChangeDirectoryRequest request, CancellationToken cancellationToken)
        {
            var response = new ChangeDirectoryResponse();
            try
            {
                if (!_analyzer.ChangeDirectory(request.DirectoryPath))
                {
                    response.CurrentDirectory = _analyzer.CurrentDirectory ?? "";
                    response.Status = new OperationStatusMessage()
                    {
                        Success = false,
                        Code = AgentErrorCode.DirectoryNotFound,
                        Message = $"Directory not found: {request.DirectoryPath}",
                    };
                }
                else
                {
                    response.CurrentDirectory = _analyzer.CurrentDirectory ?? "";
                    response.FileNames.AddRange(_analyzer.GetLogFiles());
                    response.Status = CreateNoErrorOperationStatus();
                }
            }
            catch (Exception ex)
            {
                response.Status = CreateInternalErrorOperationStatus(ex);
                _logger.LogError(ex, "An error occurred while changing directory.");
            }
            return Task.FromResult(response);
        }

        public Task<GetLogFilesResponse> GetLogFiles(Empty empty, CancellationToken cancellationToken)
        {
            var response = new GetLogFilesResponse();
            try
            {
                response.FileNames.AddRange(_analyzer.GetLogFiles());
                response.Status = CreateNoErrorOperationStatus();
            }
            catch (Exception ex)
            {
                response.Status = CreateInternalErrorOperationStatus(ex);
                _logger.LogError(ex, "An error occurred while retrieving log files.");
            }
            return Task.FromResult(response);
        }

        public Task<AnalyzeAllResponse> AnalyzeAll(AnalyzeAllRequest request, CancellationToken cancellationToken)
        {
            var response = new AnalyzeAllResponse();
            try
            {
                try
                {
                    _analyzer.AnalyzeAll(request.DegreeOfParallelism);
                    response.Status = CreateNoErrorOperationStatus();
                }
                catch (ArgumentOutOfRangeException ex)
                {
                    response.Status = new OperationStatusMessage()
                    {
                        Success = false,
                        Code = AgentErrorCode.InvalidArgument,
                        Message = $"Argument out of range: {ex.Message}",
                    };
                }
                catch (ArgumentException ex)
                {
                    response.Status = new OperationStatusMessage()
                    {
                        Success = false,
                        Code = AgentErrorCode.InvalidArgument,
                        Message = $"Invalid argument: {ex.Message}",
                    };
                }
                catch (InvalidOperationException ex)
                {
                    response.Status = new OperationStatusMessage()
                    {
                        Success = false,
                        Code = AgentErrorCode.InvalidOperation,
                        Message = $"Invalid argument: {ex.Message}",
                    };
                }
            }
            catch (Exception ex)
            {
                response.Status = CreateInternalErrorOperationStatus(ex);
                _logger.LogError(ex, "An error occurred while analyzing log files.");
            }
            return Task.FromResult(response);
        }

        public Task<AnalyzeFilesResponse> AnalyzeFiles(AnalyzeFilesRequest request, CancellationToken cancellationToken)
        {
            var response = new AnalyzeFilesResponse();
            try
            {
                try
                {
                    _analyzer.AnalyzeFiles(request.DegreeOfParallelism, request.FileNames);
                    response.Status = CreateNoErrorOperationStatus();
                }
                catch (ArgumentOutOfRangeException ex)
                {
                    response.Status = new OperationStatusMessage()
                    {
                        Success = false,
                        Code = AgentErrorCode.InvalidArgument,
                        Message = $"Argument out of range: {ex.Message}",
                    };
                }
                catch (ArgumentException ex)
                {
                    response.Status = new OperationStatusMessage()
                    {
                        Success = false,
                        Code = AgentErrorCode.InvalidArgument,
                        Message = $"Invalid argument: {ex.Message}",
                    };
                }
                catch (InvalidOperationException ex)
                {
                    response.Status = new OperationStatusMessage()
                    {
                        Success = false,
                        Code = AgentErrorCode.InvalidOperation,
                        Message = $"Invalid argument: {ex.Message}",
                    };
                }
            }
            catch (Exception ex)
            {
                response.Status = CreateInternalErrorOperationStatus(ex);
                _logger.LogError(ex, "An error occurred while analyzing log files.");
            }
            return Task.FromResult(response);
        }

        public IReadOnlyList<GetAnalysisResultResponse> GetAnalysisResult(GetAnalysisResultRequest request, CancellationToken cancellationToken)
        {
            var analysisResult = new List<GetAnalysisResultResponse>();
            try
            {
                if (_analyzer.TryGetAnalysisResult(request.FileName, out var result))
                {
                    if (result is null)
                    {
                        analysisResult.Add(new GetAnalysisResultResponse
                        {
                            Status = new OperationStatusMessage()
                            {
                                Success = false,
                                Code = AgentErrorCode.FileNotFound,
                                Message = $"File not found: {request.FileName}",
                            }
                        });
                    }
                    else
                    {
                        if (result.State == AnalysisState.NotAnalyzed)
                        {
                            analysisResult.Add(new GetAnalysisResultResponse
                            {
                                Header = new AnalysisResultHeaderMessage()
                                {
                                    FileName = result.FileName,
                                    FullName = result.FullName,
                                    State = GrpcTypeConverter.ConvertToGrpc(result.State),
                                    WorkerId = result.WorkerId,
                                },
                                Status = new OperationStatusMessage()
                                {
                                    Success = true,
                                    Code = AgentErrorCode.NoAgentError
                                }
                            });
                        }
                        else if (result.State == AnalysisState.Succeeded)
                        {
                            analysisResult.Add(new GetAnalysisResultResponse
                            {
                                Header = new AnalysisResultHeaderMessage()
                                {
                                    FileName = result.FileName,
                                    FullName = result.FullName,
                                    State = GrpcTypeConverter.ConvertToGrpc(result.State),
                                    WorkerId = result.WorkerId,
                                },
                                Status = new OperationStatusMessage()
                                {
                                    Success = true,
                                    Code = AgentErrorCode.NoAgentError,
                                    Message = "",
                                }
                            });
                            foreach (var entry in result.Entries)
                            {
                                analysisResult.Add(new GetAnalysisResultResponse
                                {
                                    LogEntry = GrpcTypeConverter.ConvertToGrpc(entry),
                                    Status = new OperationStatusMessage()
                                    {
                                        Success = true,
                                        Code = AgentErrorCode.NoAgentError,
                                        Message = "",
                                    }
                                });
                            }
                        }
                        else if (result.State == AnalysisState.Failed)
                        {
                            analysisResult.Add(new GetAnalysisResultResponse
                            {
                                Header = new AnalysisResultHeaderMessage()
                                {
                                    FileName = result.FileName,
                                    FullName = result.FullName,
                                    State = GrpcTypeConverter.ConvertToGrpc(result.State),
                                    ErrorMessage = result.ErrorMessage,
                                    WorkerId = result.WorkerId,
                                },
                                Status = new OperationStatusMessage()
                                {
                                    Success = true,
                                    Code = AgentErrorCode.NoAgentError
                                }
                            });
                        }
                        else
                        {
                            throw new Exception($"Unexpected analysis state: {result.State}");
                        }
                    }
                }
                else
                {
                    analysisResult.Add(new GetAnalysisResultResponse
                    {
                        Status = new OperationStatusMessage()
                        {
                            Success = false,
                            Code = AgentErrorCode.FileNotFound,
                            Message = $"File not found: {request.FileName}",
                        }
                    });
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while retrieving analysis results.");
                analysisResult.Add(new GetAnalysisResultResponse
                {
                    Status = CreateInternalErrorOperationStatus(ex),
                });
            }
            return analysisResult;
        }
    }
}
