using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;

namespace ddp
{
    internal class Program
    {
        private static void Main(string[] args)
        {
            bool verbose = false, emailOnly = false;
            string filename = args[0], output = "filtered.txt";
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

                        default:
                            verbose = cmd.value == "/v" || cmd.value == "/verbose";
                            emailOnly = cmd.value == "/e" || cmd.value == "/email";
                            break;
                    }
                }

                // do work
                var document = ReadFile(filename);
                var deduped = Parse(document, emailOnly).ToList();
                WriteFile(deduped, output, verbose);
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
            }
        }

        private static IEnumerable<string> Parse(string input, bool emailOnly = false)
        {
            var result = new List<string>();

            if (emailOnly)
                result.AddRange(from Match email in Regex.Matches(input, @"\b[a-z0-9._%+-]+@[a-z0-9.-]+\.[a-z]{2,4}\b") select email.Value.Trim());
            else
            {
                var csv = input.Split(',', ';', '\r', '\n').Select(line => line.Trim());
                var special = (from Match match in Regex.Matches(input, "(<.*>|\".*\")") select match.Value).ToList();

                result.AddRange(special.Count > 0 ? special : csv);
            }

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
                    document = reader.ReadToEnd();
                }
            }

            return document;
        }

        private static void WriteFile(ICollection<string> data, string filename, bool verbose = false)
        {
            var filtered = data.Distinct().ToList();

            using (var writer = new StreamWriter(filename))
            {
                if (verbose)
                {
                    writer.WriteLine("There are {0} records with {1} duplicates removed", filtered.Count, data.Count - filtered.Count);
                    writer.WriteLine();
                }

                writer.WriteLine(string.Join(verbose ? ",\r\n" : ",", filtered));
            }
        }
    }
}