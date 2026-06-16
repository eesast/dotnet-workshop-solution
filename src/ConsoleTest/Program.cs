using CsvHelper;
using CsvHelper.Configuration;
using System.Globalization;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;

using LogParser.Parser;

System.Console.WriteLine("Hello, ConsoleTest of dotnet-workshop!");

var parser = new LogFileParser();
var result = parser.Parse(new StreamReader("dataset/basic.log")).ToList();
Console.WriteLine(result.Count);
