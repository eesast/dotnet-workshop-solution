using LogParser.Models;
using LogParser.Parser;
using test_01_basic.dataset;

namespace test_01_basic
{
    [TestClass]
    public sealed class TestLogFileParserBasic
    {
        private static readonly string CallLogExample = "0,2026-06-05T16:00:29.045Z,userservice-0,\"{\"\"severity\"\": \"\"INFO\"\", \"\"event\"\": \"\"call\"\", \"\"request-id\"\": \"\"3a013a08-6853-49fc-8f06-50daeb5c1e51\"\", \"\"target-service\"\": \"\"authservice\"\", \"\"duration-ms\"\": 18}\"";
        private static readonly string RequestLogExample = "1,2026-06-05T16:00:31.086Z,userservice-1,\"{\"\"severity\"\": \"\"INFO\"\", \"\"event\"\": \"\"request\"\", \"\"request-id\"\": \"\"1177c344-115e-4f85-b8ec-c9164d132b79\"\", \"\"method\"\": \"\"GET\"\", \"\"path\"\": \"\"/api/user/john\"\", \"\"status-code\"\": 404}\"";
        private static readonly string InternalLogExample = "2,2026-06-05T16:05:45.322Z,gateway-0,\"{\"\"severity\"\": \"\"ERROR\"\", \"\"event\"\": \"\"internal\"\", \"\"exception\"\": \"\"System.InvalidOperationException: Failed to load gateway routing configuration.\"\"}\"";

        private void VerifyLogEntryBase(LogEntry targetEntry, LogEntry entry)
        {
            Assert.AreEqual(targetEntry.LineNo, entry.LineNo,
                $"Line number should be {targetEntry.LineNo}, got {entry.LineNo}");
            Assert.AreEqual(targetEntry.TimeStamp, entry.TimeStamp,
                $"Timestamp should be {targetEntry.TimeStamp}, got {entry.TimeStamp}, at line {entry.LineNo}");
            Assert.AreEqual(targetEntry.PodName, entry.PodName,
                $"Pod name should be '{targetEntry.PodName}', got '{entry.PodName}', at line {entry.LineNo}");
            Assert.AreEqual(targetEntry.Severity, entry.Severity,
                $"Severity should be {targetEntry.Severity}, got {entry.Severity}, at line {entry.LineNo}");
            Assert.AreEqual(targetEntry.EventType, entry.EventType,
                $"Event type should be {targetEntry.EventType}, got {entry.EventType}, at line {entry.LineNo}");
        }

        private void VerifyCallLogEntry(CallLogEntry targetEntry, LogEntry entry)
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

        private void VerifyCallLogEntryExample(LogEntry entry)
        {
            VerifyCallLogEntry(new CallLogEntry
            (
                LineNo: 0,
                Timestamp: DateTimeOffset.Parse("2026-06-05T16:00:29.045Z"),
                PodName: "userservice-0",
                Severity: LogSeverity.Info,
                RequestId: "3a013a08-6853-49fc-8f06-50daeb5c1e51",
                TargetService: "authservice",
                DurationMs: 18
            ), entry);
        }

        private void VerifyRequestLogEntry(RequestLogEntry targetEntry, LogEntry entry)
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

        private void VerifyRequestLogEntryExample(LogEntry entry)
        {
            VerifyRequestLogEntry(new RequestLogEntry
            (
                LineNo: 1,
                Timestamp: DateTimeOffset.Parse("2026-06-05T16:00:31.086Z"),
                PodName: "userservice-1",
                Severity: LogSeverity.Info,
                RequestId: "1177c344-115e-4f85-b8ec-c9164d132b79",
                Method: "GET",
                Path: "/api/user/john",
                StatusCode: 404
            ), entry);
        }

        private void VerifyInternalLogEntry(InternalLogEntry targetEntry, LogEntry entry)
        {
            VerifyLogEntryBase(targetEntry, entry);

            var internalEntry = (entry as InternalLogEntry)!;

            Assert.AreEqual(targetEntry.ExceptionName, internalEntry.ExceptionName,
                $"Exception name should be '{targetEntry.ExceptionName}', got '{internalEntry.ExceptionName}', at line {entry.LineNo}");
            Assert.AreEqual(targetEntry.ExceptionMessage, internalEntry.ExceptionMessage,
                $"Exception message should be '{targetEntry.ExceptionMessage}', got '{internalEntry.ExceptionMessage}', at line {entry.LineNo}");
        }

        private void VerifyInternalLogEntryExample(LogEntry entry)
        {
            VerifyInternalLogEntry(new InternalLogEntry(
                LineNo: 2,
                Timestamp: DateTimeOffset.Parse("2026-06-05T16:05:45.322Z"),
                PodName: "gateway-0",
                Severity: LogSeverity.Error,
                ExceptionName: "System.InvalidOperationException",
                ExceptionMessage: "Failed to load gateway routing configuration."
            ), entry);
        }

        [TestMethod]
        public void TestParseCallLogEntry()
        {
            var parser = new LogFileParser();
            var result = parser.Parse(new StringReader(CallLogExample)).ToList();

            Assert.IsNotNull(result, "Parse result should not be null");
            Assert.HasCount(1, result, $"Parse result should contain exactly 1 entry, got {result.Count}");

            VerifyCallLogEntryExample(result[0]);
        }

        [TestMethod]
        public void TestParseRequestLogEntry()
        {
            var parser = new LogFileParser();
            var result = parser.Parse(new StringReader(RequestLogExample)).ToList();

            Assert.IsNotNull(result, "Parse result should not be null");
            Assert.HasCount(1, result, $"Parse result should contain exactly 1 entry, got {result.Count}");

            VerifyRequestLogEntryExample(result[0]);
        }

        [TestMethod]
        public void TestParseInternalLogEntry()
        {
            var parser = new LogFileParser();
            var result = parser.Parse(new StringReader(InternalLogExample)).ToList();

            Assert.IsNotNull(result, "Parse result should not be null");
            Assert.HasCount(1, result, $"Parse result should contain exactly 1 entry, got {result.Count}");

            VerifyInternalLogEntryExample(result[0]);
        }

        [TestMethod]
        public void TestParseMultipleLogEntries()
        {
            var parser = new LogFileParser();
            var logEntries = new List<string>
            {
                CallLogExample,
                RequestLogExample,
                InternalLogExample
            };
            var result = parser.Parse(new StringReader(string.Join(Environment.NewLine, logEntries))).ToList();
            Assert.IsNotNull(result, "Parse result should not be null");
            Assert.HasCount(3, result, $"Parse result should contain exactly 3 entries, got {result.Count}");

            // Validate each entry type
            Assert.IsInstanceOfType(result[0], typeof(CallLogEntry),
                $"The first entry should be of type CallLogEntry, got {result[0].GetType().Name}");
            Assert.IsInstanceOfType(result[1], typeof(RequestLogEntry),
                $"The second entry should be of type RequestLogEntry, got {result[1].GetType().Name}");
            Assert.IsInstanceOfType(result[2], typeof(InternalLogEntry),
                $"The third entry should be of type InternalLogEntry, got {result[2].GetType().Name}");

            VerifyCallLogEntryExample(result[0]);
            VerifyRequestLogEntryExample(result[1]);
            VerifyInternalLogEntryExample(result[2]);
        }

        [TestMethod]
        public void TestParseLogEntriesInFile()
        {
            var parser = new LogFileParser();
            var result = parser.Parse(new StreamReader("dataset/basic.log")).ToList();
            Assert.IsNotNull(result, "Parse result should not be null");
            Assert.HasCount(3, result, $"Parse result should contain exactly 3 entries, got {result.Count}");

            // Validate each entry type
            Assert.IsInstanceOfType(result[0], typeof(CallLogEntry),
                $"The first entry should be of type CallLogEntry, got {result[0].GetType().Name}");
            Assert.IsInstanceOfType(result[1], typeof(RequestLogEntry),
                $"The second entry should be of type RequestLogEntry, got {result[1].GetType().Name}");
            Assert.IsInstanceOfType(result[2], typeof(InternalLogEntry),
                $"The third entry should be of type InternalLogEntry, got {result[2].GetType().Name}");

            VerifyCallLogEntryExample(result[0]);
            VerifyRequestLogEntryExample(result[1]);
            VerifyInternalLogEntryExample(result[2]);
        }

        [TestMethod]
        public void TestParseMultipleLogEntriesInFile()
        {
            var parser = new LogFileParser();
            var result = parser.Parse(new StreamReader("dataset/basic-multiple.log")).ToList();
            Assert.IsNotNull(result, "Parse result should not be null");

            var targetLogEntries = BasicMultiple.LogData;
            Assert.HasCount(targetLogEntries.Count, result, $"Parse result should contain exactly {targetLogEntries.Count} entries, got {result.Count}");

            // Zip together target and actual entries to validate them in pairs
            foreach (var ((targetEventType, targetLogEntry), entry) in targetLogEntries.Zip(result, (target, actual) => (Target: target, Actual: actual)))
            {
                switch (targetEventType)
                {
                    case LogEventType.Call:
                        Assert.IsInstanceOfType(targetLogEntry, typeof(CallLogEntry),
                            $"Dataset error: Expected a CallLogEntry, got {entry.GetType().Name}, at line {targetLogEntry.LineNo}");
                        Assert.IsInstanceOfType(entry, typeof(CallLogEntry),
                            $"Expected a CallLogEntry, got {entry.GetType().Name}, at line {entry.LineNo}");
                        VerifyCallLogEntry((CallLogEntry)targetLogEntry, entry);
                        break;
                    case LogEventType.Request:
                        Assert.IsInstanceOfType(targetLogEntry, typeof(RequestLogEntry),
                            $"Dataset error: Expected a RequestLogEntry, got {entry.GetType().Name}, at line  {targetLogEntry.LineNo}");
                        Assert.IsInstanceOfType(entry, typeof(RequestLogEntry),
                            $"Expected a RequestLogEntry, got {entry.GetType().Name}, at line {entry.LineNo}");
                        VerifyRequestLogEntry((RequestLogEntry)targetLogEntry, entry);
                        break;
                    case LogEventType.Internal:
                        Assert.IsInstanceOfType(targetLogEntry, typeof(InternalLogEntry),
                            $"Dataset error: Expected an InternalLogEntry, got {entry.GetType().Name}, at line  {targetLogEntry.LineNo}");
                        Assert.IsInstanceOfType(entry, typeof(InternalLogEntry),
                            $"Expected an InternalLogEntry, got {entry.GetType().Name}, at line {entry.LineNo}");
                        VerifyInternalLogEntry((InternalLogEntry)targetLogEntry, entry);
                        break;
                    default:
                        Assert.Fail($"Unknown log event type: {targetEventType}, at line {targetLogEntry.LineNo}");
                        break;
                }
            }
        }
    }
}
