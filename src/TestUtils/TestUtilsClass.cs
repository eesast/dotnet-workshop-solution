using LogParser.Models;

namespace TestUtils
{
    public static class TestUtilsClass
    {
        public static readonly string CallLogExample = "0,2026-06-05T16:00:29.045Z,userservice-0,\"{\"\"severity\"\": \"\"INFO\"\", \"\"event\"\": \"\"call\"\", \"\"request-id\"\": \"\"3a013a08-6853-49fc-8f06-50daeb5c1e51\"\", \"\"target-service\"\": \"\"authservice\"\", \"\"duration-ms\"\": 18}\"";
        public static readonly string RequestLogExample = "1,2026-06-05T16:00:31.086Z,userservice-1,\"{\"\"severity\"\": \"\"INFO\"\", \"\"event\"\": \"\"request\"\", \"\"request-id\"\": \"\"1177c344-115e-4f85-b8ec-c9164d132b79\"\", \"\"method\"\": \"\"GET\"\", \"\"path\"\": \"\"/api/user/john\"\", \"\"status-code\"\": 404}\"";
        public static readonly string InternalLogExample = "2,2026-06-05T16:05:45.322Z,gateway-0,\"{\"\"severity\"\": \"\"ERROR\"\", \"\"event\"\": \"\"internal\"\", \"\"exception\"\": \"\"System.InvalidOperationException: Failed to load gateway routing configuration.\"\"}\"";

        public static readonly string NoEventExampleFailed = "0,2026-06-05T16:00:29.045Z,userservice-0,\"{\"\"severity\"\": \"\"INFO\"\", \"\"request-id\"\": \"\"3a013a08-6853-49fc-8f06-50daeb5c1e51\"\", \"\"target-service\"\": \"\"authservice\"\", \"\"duration-ms\"\": 18}\"";
        public static readonly string CallLogExampleFailed = "0,2026-06-05T16:00:29.045Z,userservice-0,\"{\"\"severity\"\": \"\"INFO\"\", \"\"event\"\": \"\"call\"\", \"\"request-id\"\": \"\"3a013a08-6853-49fc-8f06-50daeb5c1e51\"\", \"\"target-service\"\": \"\"authservice\"\"}\"";
        public static readonly string RequestLogExampleFailed = "1,2026-06-05T16:00:31.086Z,userservice-1,\"{\"\"severity\"\": \"\"INFO\"\", \"\"event\"\": \"\"request\"\", \"\"request-id\"\": \"\"1177c344-115e-4f85-b8ec-c9164d132b79\"\", \"\"path\"\": \"\"/api/user/john\"\", \"\"status-code\"\": 404}\"";
        public static readonly string InternalLogExampleFailed = "2,2026-06-05T16:05:45.322Z,gateway-0,\"{\"\"severity\"\": \"\"ERROR\"\", \"\"event\"\": \"\"internal\"\", \"\"exception\"\": \"\"System.InvalidOperationException Failed to load gateway routing configuration.\"\"}\"";

        public static readonly CallLogEntry CallLogExampleEntry = new CallLogEntry(
            LineNo: 0,
            Timestamp: DateTimeOffset.Parse("2026-06-05T16:00:29.045Z"),
            PodName: "userservice-0",
            Severity: LogSeverity.Info,
            RequestId: "3a013a08-6853-49fc-8f06-50daeb5c1e51",
            TargetService: "authservice",
            DurationMs: 18
        );

        public static readonly RequestLogEntry RequestLogExampleEntry = new RequestLogEntry(
            LineNo: 1,
            Timestamp: DateTimeOffset.Parse("2026-06-05T16:00:31.086Z"),
            PodName: "userservice-1",
            Severity: LogSeverity.Info,
            RequestId: "1177c344-115e-4f85-b8ec-c9164d132b79",
            Method: "GET",
            Path: "/api/user/john",
            StatusCode: 404
        );

        public static readonly InternalLogEntry InternalLogExampleEntry = new InternalLogEntry(
            LineNo: 2,
            Timestamp: DateTimeOffset.Parse("2026-06-05T16:05:45.322Z"),
            PodName: "gateway-0",
            Severity: LogSeverity.Error,
            ExceptionName: "System.InvalidOperationException",
            ExceptionMessage: "Failed to load gateway routing configuration."
        );

        public static void VerifyLogEntryBase(LogEntry targetEntry, LogEntry entry)
        {
            Assert.AreEqual(targetEntry.LineNo, entry.LineNo,
                $"Line number should be {targetEntry.LineNo}, got {entry.LineNo}");
            Assert.AreEqual(targetEntry.Timestamp, entry.Timestamp,
                $"Timestamp should be {targetEntry.Timestamp}, got {entry.Timestamp}, at line {entry.LineNo}");
            Assert.AreEqual(targetEntry.PodName, entry.PodName,
                $"Pod name should be '{targetEntry.PodName}', got '{entry.PodName}', at line {entry.LineNo}");
            Assert.AreEqual(targetEntry.Severity, entry.Severity,
                $"Severity should be {targetEntry.Severity}, got {entry.Severity}, at line {entry.LineNo}");
            Assert.AreEqual(targetEntry.EventType, entry.EventType,
                $"Event type should be {targetEntry.EventType}, got {entry.EventType}, at line {entry.LineNo}");
        }

        public static void VerifyCallLogEntry(CallLogEntry targetEntry, LogEntry entry)
        {
            VerifyLogEntryBase(targetEntry, entry);

            var callEntry = (entry as CallLogEntry)!;

            Assert.AreEqual(targetEntry.RequestId, callEntry.RequestId,
                $"Request ID should be '{targetEntry.RequestId}', got '{callEntry.RequestId}', at line {entry.LineNo}");
            Assert.AreEqual(targetEntry.TargetService, callEntry.TargetService,
                $"Target service should be '{targetEntry.TargetService}', got '{callEntry.TargetService}', at line {entry.LineNo}");
            Assert.AreEqual(targetEntry.DurationMs, callEntry.DurationMs,
                $"Duration should be {targetEntry.DurationMs} ms, got {callEntry.DurationMs}, at line {entry.LineNo}");
        }

        public static void VerifyCallLogEntryExample(LogEntry entry)
        {
            VerifyCallLogEntry(CallLogExampleEntry, entry);
        }

        public static void VerifyRequestLogEntry(RequestLogEntry targetEntry, LogEntry entry)
        {
            VerifyLogEntryBase(targetEntry, entry);

            var requestEntry = (entry as RequestLogEntry)!;

            Assert.AreEqual(targetEntry.RequestId, requestEntry.RequestId,
                $"Request ID should be '{requestEntry.RequestId}', got '{requestEntry.RequestId}', at line {entry.LineNo}");
            Assert.AreEqual(targetEntry.Method, requestEntry.Method,
                $"Method should be '{requestEntry.Method}', got '{requestEntry.Method}', at line {entry.LineNo}");
            Assert.AreEqual(targetEntry.Path, requestEntry.Path,
                $"Path should be '{targetEntry.Path}', got '{requestEntry.Path}', at line {entry.LineNo}");
            Assert.AreEqual(targetEntry.StatusCode, requestEntry.StatusCode,
                $"Status code should be {targetEntry.StatusCode}, got {requestEntry.StatusCode}, at line {entry.LineNo}");
        }

        public static void VerifyRequestLogEntryExample(LogEntry entry)
        {
            VerifyRequestLogEntry(RequestLogExampleEntry, entry);
        }

        public static void VerifyInternalLogEntry(InternalLogEntry targetEntry, LogEntry entry)
        {
            VerifyLogEntryBase(targetEntry, entry);

            var internalEntry = (entry as InternalLogEntry)!;

            Assert.AreEqual(targetEntry.ExceptionName, internalEntry.ExceptionName,
                $"Exception name should be '{targetEntry.ExceptionName}', got '{internalEntry.ExceptionName}', at line {entry.LineNo}");
            Assert.AreEqual(targetEntry.ExceptionMessage, internalEntry.ExceptionMessage,
                $"Exception message should be '{targetEntry.ExceptionMessage}', got '{internalEntry.ExceptionMessage}', at line {entry.LineNo}");
        }

        public static void VerifyInternalLogEntryExample(LogEntry entry)
        {
            VerifyInternalLogEntry(InternalLogExampleEntry, entry);
        }
    }
}
