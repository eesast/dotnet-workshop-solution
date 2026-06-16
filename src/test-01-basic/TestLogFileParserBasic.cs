using LogParser.Models;
using LogParser.Parser;
using System.Diagnostics;

namespace test_01_basic
{
    [TestClass]
    public sealed class TestLogFileParserBasic
    {
        private static readonly string CallLogExample = "0,2026-06-05T16:00:29.045Z,userservice-0,\"{\"\"severity\"\": \"\"INFO\"\", \"\"event\"\": \"\"call\"\", \"\"request-id\"\": \"\"3a013a08-6853-49fc-8f06-50daeb5c1e51\"\", \"\"target-service\"\": \"\"authservice\"\", \"\"duration-ms\"\": 18}\"";
        private static readonly string RequestLogExample = "1,2026-06-05T16:00:31.086Z,userservice-1,\"{\"\"severity\"\": \"\"INFO\"\", \"\"event\"\": \"\"request\"\", \"\"request-id\"\": \"\"1177c344-115e-4f85-b8ec-c9164d132b79\"\", \"\"method\"\": \"\"GET\"\", \"\"path\"\": \"\"/api/user/john\"\", \"\"status-code\"\": 404}\"";
        private static readonly string InternalLogExample = "2,2026-06-05T16:05:45.322Z,gateway-0,\"{\"\"severity\"\": \"\"ERROR\"\", \"\"event\"\": \"\"internal\"\", \"\"exception\"\": \"\"System.InvalidOperationException: Failed to load gateway routing configuration.\"\"}\"";

        [TestMethod]
        public void TestParseCallLogEntry()
        {
            var parser = new LogFileParser();
            var result = parser.Parse(new StringReader(CallLogExample)).ToList();

            Assert.IsNotNull(result, "Parse result should not be null");
            Assert.HasCount(1, result, $"Parse result should contain exactly 1 entry, got {result.Count}");

            var entry = result[0];

            Assert.AreEqual(0, entry.LineNo,
                $"Line number should be 0, got {entry.LineNo}");
            Assert.AreEqual(DateTimeOffset.Parse("2026-06-05T16:00:29.045Z"), entry.TimeStamp,
                $"Timestamp should be 2026-06-05T16:00:29.045Z, got {entry.TimeStamp}");
            Assert.AreEqual("userservice-0", entry.PodName,
                $"Pod name should be 'userservice-0', got '{entry.PodName}'");
            Assert.AreEqual(LogSeverity.Info, entry.Severity,
                $"Severity should be Info, got {entry.Severity}");
            Assert.AreEqual(LogEventType.Call, entry.EventType,
                $"Event type should be Call, got {entry.EventType}");
            Assert.IsInstanceOfType(entry, typeof(CallLogEntry),
                $"Entry should be of type CallLogEntry, got {entry.GetType().Name}");

            var callEntry = (entry as CallLogEntry)!;

            Assert.AreEqual("3a013a08-6853-49fc-8f06-50daeb5c1e51", callEntry.RequestId,
                $"Request ID should be '3a013a08-6853-49fc-8f06-50daeb5c1e51', got '{callEntry.RequestId}'");
            Assert.AreEqual("authservice", callEntry.TargetService,
                $"Target service should be 'authservice', got '{callEntry.TargetService}'");
            Assert.AreEqual(18, callEntry.DurationMs,
                $"Duration should be 18 ms, got {callEntry.DurationMs}");
        }

        [TestMethod]
        public void TestParseRequestLogEntry()
        {
            var parser = new LogFileParser();
            var result = parser.Parse(new StringReader(RequestLogExample)).ToList();

            Assert.IsNotNull(result, "Parse result should not be null");
            Assert.HasCount(1, result, $"Parse result should contain exactly 1 entry, got {result.Count}");

            var entry = result[0];

            Assert.AreEqual(1, entry.LineNo,
                $"Line number should be 1, got {entry.LineNo}");
            Assert.AreEqual(DateTimeOffset.Parse("2026-06-05T16:00:31.086Z"), entry.TimeStamp,
                $"Timestamp should be 2026-06-05T16:00:31.086Z, got {entry.TimeStamp}");
            Assert.AreEqual("userservice-1", entry.PodName,
                $"Pod name should be 'userservice-1', got '{entry.PodName}'");
            Assert.AreEqual(LogSeverity.Info, entry.Severity,
                $"Severity should be Info, got {entry.Severity}");
            Assert.AreEqual(LogEventType.Request, entry.EventType,
                $"Event type should be Request, got {entry.EventType}");
            Assert.IsInstanceOfType(entry, typeof(RequestLogEntry),
                $"Entry should be of type RequestLogEntry, got {entry.GetType().Name}");

            var requestEntry = (entry as RequestLogEntry)!;

            Assert.AreEqual("1177c344-115e-4f85-b8ec-c9164d132b79", requestEntry.RequestId,
                $"Request ID should be '1177c344-115e-4f85-b8ec-c9164d132b79', got '{requestEntry.RequestId}'");
            Assert.AreEqual("GET", requestEntry.Method,
                $"Method should be 'GET', got '{requestEntry.Method}'");
            Assert.AreEqual("/api/user/john", requestEntry.Path,
                $"Path should be '/api/user/john', got '{requestEntry.Path}'");
            Assert.AreEqual(404, requestEntry.StatusCode,
                $"Status code should be 404, got {requestEntry.StatusCode}");
        }

        [TestMethod]
        public void TestParseInternalLogEntry()
        {
            var parser = new LogFileParser();
            var result = parser.Parse(new StringReader(InternalLogExample)).ToList();

            Assert.IsNotNull(result, "Parse result should not be null");
            Assert.HasCount(1, result, $"Parse result should contain exactly 1 entry, got {result.Count}");

            var entry = result[0];

            Assert.AreEqual(2, entry.LineNo,
                $"Line number should be 2, got {entry.LineNo}");
            Assert.AreEqual(DateTimeOffset.Parse("2026-06-05T16:05:45.322Z"), entry.TimeStamp,
                $"Timestamp should be 2026-06-05T16:05:45.322Z, got {entry.TimeStamp}");
            Assert.AreEqual("gateway-0", entry.PodName,
                $"Pod name should be 'gateway-0', got '{entry.PodName}'");
            Assert.AreEqual(LogSeverity.Error, entry.Severity,
                $"Severity should be Error, got {entry.Severity}");
            Assert.AreEqual(LogEventType.Internal, entry.EventType,
                $"Event type should be Internal, got {entry.EventType}");
            Assert.IsInstanceOfType(entry, typeof(InternalLogEntry),
                $"Entry should be of type InternalLogEntry, got {entry.GetType().Name}");

            var internalEntry = (entry as InternalLogEntry)!;

            Assert.AreEqual("System.InvalidOperationException", internalEntry.ExceptionName);
            Assert.AreEqual("Failed to load gateway routing configuration.", internalEntry.ExceptionMessage,
                $"Exception message should be 'Failed to load gateway routing configuration.', got '{internalEntry.ExceptionMessage}'");
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
        }
    }
}
