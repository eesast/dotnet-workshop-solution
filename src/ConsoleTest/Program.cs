using CsvHelper;
using CsvHelper.Configuration;
using System.Globalization;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;

using LogParser.Parser;
using LogParser.Visitors;

System.Console.WriteLine("Hello, ConsoleTest of dotnet-workshop!");

var parser = new LogFileParser();
var results = parser.Parse(new StreamReader("dataset/basic.log")).ToList();
var kvdumper = new KeyValueVisitor();
var kvresults = results.Select(entry => kvdumper.Dump(entry)).ToList();

Console.WriteLine(string.Join("\n", kvresults.Select(kvresult => string.Join(", ", kvresult.Select(kv => $"{kv.Key}: {kv.Value}")))));
