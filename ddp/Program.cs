using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;

namespace ddp
{
    internal class Program
    {
        private static string RegexEmail
        {
            get { return @"\b[A-Za-z0-9._%+-]+@[A-Za-z0-9.-]+\.[a-z]{2,4}\b"; }
        }

        private static string RegexHtml
        {
            get { return @"<.*>"; }
        }

        private static string RegexString
        {
            get { return "\".*\""; }
        }

        private static string RegexNumbers
        {
            get { return @"\([0-9]*,\W?[0-9]*\)"; }
        }

        private static void Main(string[] args)
        {
            bool verbose = false, emails = false, htmls = false, strings = false, numbers = false;
            string filename = args.Length > 0 ? args[0] : string.Empty, output = "filtered.txt";
            var commands = args.Select((value, index) => new { value = args[index], index });

            try
            {
                // command parser
                foreach (var cmd in commands)
                {
                    switch (cmd.value.ToLower())
                    {
                        case "/o":
                        case "/output":
                            output = args[cmd.index + 1];
                            break;

                        case "/v":
                        case "/verbose":
                            verbose = true;
                            break;

                        case "/e":
                        case "/email":
                            emails = true;
                            break;

                        case "/h":
                        case "/html":
                            htmls = true;
                            break;

                        case "/s":
                        case "/strings":
                            strings = true;
                            break;

                        case "/n":
                        case "/numbers":
                            numbers = true;
                            break;

                        default:
                            break;
                    }
                }

                // do work
                var document = ReadFile(filename);
                var filtered = Parse(document, emails, htmls, strings, numbers).ToList();
                WriteFile(filtered, output, verbose);
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
            }
        }

        private static IEnumerable<string> Parse(string input, bool emails, bool htmls, bool strings, bool numbers)
        {
            var result = new List<string>();

            if (emails)
                result.AddRange(from Match email in Regex.Matches(input, RegexEmail) select email.Value.Trim());
            if (htmls)
                result.AddRange(from Match html in Regex.Matches(input, RegexHtml) select html.Value.Trim());
            if (strings)
                result.AddRange(from Match str in Regex.Matches(input, RegexString) select str.Value.Trim());
            if (numbers)
                result.AddRange(from Match number in Regex.Matches(input, RegexNumbers) select number.Value.Trim());

            if (!emails && !htmls && !strings && !numbers)
                result.AddRange(input.Split(new[] { ',', ';', '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries).Select(line => line.Trim()));

            return result;
        }

        private static string ReadFile(string filename)
        {
            var document = string.Empty;

            if (string.IsNullOrEmpty(filename) || !File.Exists(filename))
            {
                Console.WriteLine("No file supplied or no file exists");
                return string.Empty;
            }

            using (var reader = File.OpenText(filename))
            {
                while (!reader.EndOfStream)
                {
                    document = reader.ReadToEnd().ToLower();
                }
            }

            return document;
        }

        private static void WriteFile(ICollection<string> data, string filename, bool verbose = false)
        {
            var distinct = data.Distinct().ToList();

            using (var writer = new StreamWriter(filename))
            {
                if (verbose)
                {
                    writer.WriteLine("There are {0} records with {1} duplicates removed", distinct.Count, data.Count - distinct.Count);
                    writer.WriteLine();
                }

                writer.WriteLine(string.Join(verbose ? ",\r\n" : ",", distinct));
            }
        }
    }
}