using System.Collections.Generic;
using System.Diagnostics;
using System.IO;

namespace MonoalphabeticDeciphering
{
    static class EnglishLetterFrequency
    {

        private static bool loaded = false;
        private static Dictionary<char, double> proportions = null;

        private const string letterFrequencyFilename = "EnglishLetterFrequencies.dat";

        public static Dictionary<char, double> GetLetterProportions()
        {

            if (loaded)
            {

                return proportions;

            }
            else
            {

                Dictionary<char, double> letterProportions = new Dictionary<char, double>();

                foreach (string line in LoadFilteredFrequencyFileLines())
                {

                    string[] parts = line.Split(':');
                    char c = parts[0].ToUpper()[0];

                    double percentage = double.Parse(parts[1]);
                    double proportion = percentage / 100;

                    letterProportions[c] = proportion;

                }

                Debug.Assert(letterProportions.Count == Program.validCharacters.Length);

                proportions = letterProportions;
                loaded = true;

                return letterProportions;

            }

        }

        private static string[] LoadFilteredFrequencyFileLines()
        {

            string[] lines = File.ReadAllLines(letterFrequencyFilename);

            List<string> filteredLines = new List<string>();

            foreach (string line in lines)
            {

                if (line[0] != '/' || line[1] != '/')
                {

                    filteredLines.Add(line.Replace(" ", ""));

                }

            }

            return filteredLines.ToArray();

        }

    }
}
