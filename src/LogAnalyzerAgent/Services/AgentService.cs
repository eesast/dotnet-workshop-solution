using Google.Protobuf.WellKnownTypes;
using Grpc.Core;
using LogAnalyzer;
using LogAnalyzerRpc.Protos;
using LogAnalyzerRpc;
using LogParser.Visitors;
using LogAnalyzerAgent.Applications;

namespace LogAnalyzerAgent.Services
{
    public class AgentService : LogAnalyzerAgentService.LogAnalyzerAgentServiceBase
    {
        private readonly AgentSession _session;

        public AgentService(LogFileAnalyzer analyzer, ILoggerFactory loggerFactory)
        {
            _session = new AgentSession(analyzer, loggerFactory);
        }

        public override Task<Empty> Ping(Empty empty, ServerCallContext context)
        {
            return _session.Ping(empty, context.CancellationToken);
        }

        public override Task<AgentStatusResponse> GetAgentStatus(Empty empty, ServerCallContext context)
        {
            return _session.GetAgentStatus(empty, context.CancellationToken);
        }

        public override Task<ChangeDirectoryResponse> ChangeDirectory(ChangeDirectoryRequest request, ServerCallContext context)
        {
            return _session.ChangeDirectory(request, context.CancellationToken);
        }

        public override Task<GetLogFilesResponse> GetLogFiles(Empty empty, ServerCallContext context)
        {
            return _session.GetLogFiles(empty, context.CancellationToken);
        }

        public override Task<AnalyzeAllResponse> AnalyzeAll(AnalyzeAllRequest request, ServerCallContext context)
        {
            return _session.AnalyzeAll(request, context.CancellationToken);
        }

        public override Task<AnalyzeFilesResponse> AnalyzeFiles(AnalyzeFilesRequest request, ServerCallContext context)
        {
            return _session.AnalyzeFiles(request, context.CancellationToken);
        }

        public override async Task GetAnalysisResult(GetAnalysisResultRequest request, IServerStreamWriter<GetAnalysisResultResponse> responseStream, ServerCallContext context)
        {
            var analysisResult = _session.GetAnalysisResult(request, context.CancellationToken);
            foreach (var result in analysisResult)
            {
                await responseStream.WriteAsync(result);
            }
        }
    }
}
