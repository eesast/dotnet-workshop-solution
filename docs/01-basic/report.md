# Answer to Basic Functions

## 目录

[TOC]

## 问答题

### (Q1.1)

Q：哪条语句或哪几条语句将日志按逗号进行分割？

A：

`LogFileParser` 中使用 `CsvReader` 对逗号进行分割：

```csharp
using var csv = new CsvReader(logFile, config);
csv.Context.RegisterClassMap<LogRecordMap>();

foreach (var logRecord in csv.GetRecords<LogRecord>()) {
    // ...
}
```

Q：代码中，我们是如何指定每一行的第几个字段代表何种意义的？

A：

`csv.Context.RegisterClassMap<LogRecordMap>();` 处通过 `LogRecordMap` 中 `.Index` 指定字段的列号，`m => m.Xxx` 指定将该列转换到的 `LogRecord` 的字段名：

```csharp
internal class LogRecordMap : ClassMap<LogRecord>
{
    public LogRecordMap()
    {
        Map(m => m.LineNo).Index(0);
        Map(m => m.Timestamp).Index(1);
        Map(m => m.PodName).Index(2);
        Map(m => m.Message).Index(3);
    }
}
```

### (Q1.2)

+ `Dictionary<string, string> KeyValueVisitor.Dump(LogEntry entry)`
+ `TResult LogEntry/CallLogEntry/RequestLogEntry/InternalLogEntry.Accept<Dictionary<string, string>>(ILogEntryVisitor<Dictionary<string, string>> visitor)`
+ `Dictionary<string, string> KeyValueVisitor.Visit(CallLogEntry/RequestLogEntry/InternalLogEntry entry)`

### (Q1.3)

略。

