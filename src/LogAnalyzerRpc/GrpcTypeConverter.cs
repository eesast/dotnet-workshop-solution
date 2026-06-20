using LogAnalyzer;
using LogAnalyzer.Grpc;
using LogParser.Models;

namespace LogAnalyzerRpc
{
    public static class GrpcTypeConverter
    {
        public static AnalysisStateEnum ConvertToGrpc(AnalysisState state)
        {
            return state switch
            {
                AnalysisState.NotAnalyzed => AnalysisStateEnum.NotAnalyzed,
                AnalysisState.Succeeded => AnalysisStateEnum.Succeeded,
                AnalysisState.Failed => AnalysisStateEnum.Failed,
                _ => throw new ArgumentOutOfRangeException(nameof(state), state, null)
            };
        }

        public static LogSeverityEnum ConvertToGrpc(LogSeverity severity)
        {
            return severity switch
            {
                LogSeverity.Info => LogSeverityEnum.Info,
                LogSeverity.Warning => LogSeverityEnum.Warning,
                LogSeverity.Error => LogSeverityEnum.Error,
                _ => throw new ArgumentOutOfRangeException(nameof(severity), severity, null)
            };
        }

        public static LogEventTypeEnum ConvertToGrpc(LogEventType eventType)
        {
            return eventType switch
            {
                LogEventType.Call => LogEventTypeEnum.Call,
                LogEventType.Request => LogEventTypeEnum.Request,
                LogEventType.Internal => LogEventTypeEnum.Internal,
                _ => throw new ArgumentOutOfRangeException(nameof(eventType), eventType, null)
            };
        }

        public static AnalysisState ConvertFromGrpc(AnalysisStateEnum state)
        {
            return state switch
            {
                AnalysisStateEnum.NotAnalyzed => AnalysisState.NotAnalyzed,
                AnalysisStateEnum.Succeeded => AnalysisState.Succeeded,
                AnalysisStateEnum.Failed => AnalysisState.Failed,
                _ => throw new ArgumentOutOfRangeException(nameof(state), state, null)
            };
        }

        public static LogSeverity ConvertFromGrpc(LogSeverityEnum severity)
        {
            return severity switch
            {
                LogSeverityEnum.Info => LogSeverity.Info,
                LogSeverityEnum.Warning => LogSeverity.Warning,
                LogSeverityEnum.Error => LogSeverity.Error,
                _ => throw new ArgumentOutOfRangeException(nameof(severity), severity, null)
            };
        }

        public static LogEventType ConvertFromGrpc(LogEventTypeEnum eventType)
        {
            return eventType switch
            {
                LogEventTypeEnum.Call => LogEventType.Call,
                LogEventTypeEnum.Request => LogEventType.Request,
                LogEventTypeEnum.Internal => LogEventType.Internal,
                _ => throw new ArgumentOutOfRangeException(nameof(eventType), eventType, null)
            };
        }
    }
}
