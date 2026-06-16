namespace LogParser.Models
{
    public enum LogSeverity
    {
        Info,
        Warning,
        Error,
    }

    public enum LogEventType
    {
        Call,
        Request,
        Internal
    }

    public abstract record LogEntry(
        int LineNo,
        DateTimeOffset TimeStamp,
        string PodName,
        LogSeverity Severity,
        LogEventType EventType
    )
    {
    }

    public sealed record CallLogEntry(
        int LineNo,
        DateTimeOffset Timestamp,
        string PodName,
        LogSeverity Severity,
        string RequestId,
        string TargetService,
        int DurationMs
    ) : LogEntry(LineNo, Timestamp, PodName, Severity, LogEventType.Call)
    {
    }

    public sealed record RequestLogEntry(
        int LineNo,
        DateTimeOffset Timestamp,
        string PodName,
        LogSeverity Severity,
        string RequestId,
        string Method,
        string Path,
        int StatusCode)
        : LogEntry(LineNo, Timestamp, PodName, Severity, LogEventType.Request)
    {
    }

    public sealed record InternalLogEntry(
        int LineNo,
        DateTimeOffset Timestamp,
        string PodName,
        LogSeverity Severity,
        string ExceptionName,
        string ExceptionMessage)
        : LogEntry(LineNo, Timestamp, PodName, Severity, LogEventType.Internal)
    {
    }

}
