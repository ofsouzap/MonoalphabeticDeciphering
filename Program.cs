using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text.RegularExpressions;
using System.IO;

/*Code taken and edited from unused code from VignereDeciphering C# program (C:\Users\paulc\OneDrive\Documents\Computing\C Sharp\Console\VignereDeciphering)*/

namespace MonoalphabeticDeciphering
{
    class Program
    {

        public static readonly char[] validCharacters = new char[]
        {
            'A',
            'B',
            'C',
            'D',
            'E',
            'F',
            'G',
            'H',
            'I',
            'J',
            'K',
            'L',
            'M',
            'N',
            'O',
            'P',
            'Q',
            'R',
            'S',
            'T',
            'U',
            'V',
            'W',
            'X',
            'Y',
            'Z'
        };

        static void Main(string[] args)
        {

            string text;

            Console.Write("Text> ");

            string input = Console.ReadLine().ToUpper();

            if (input[0] == '\\')
            {
                text = GetTextFromFile(input.Substring(1)).ToUpper();
            }
            else
            {
                text = input;
            }

            GetMonoalphabeticMapping(text);

        }

        static string GetTextFromFile(string filename)
        {

            Debug.Assert(File.Exists(filename));
            return File.ReadAllText(filename);

        }

        static string FilteredInput(string text)
        {

            string output = "";

            foreach (char c in text)
            {

                if (validCharacters.Contains(c)) output += c;

            }

            return output;

        }

        static void PrintSeperator(char seperatorCharacter = '-',
            int length = 50,
            int lengthMultiplier = 1)
        {
            for (int i = 0; i < (length * lengthMultiplier); i++)
            {
                Console.Write(seperatorCharacter);
            }
            Console.WriteLine();
        }

        static bool InputIsCharacterRemapping(string inputText)
        {
            return inputText.Length == 3 &&
                validCharacters.Contains(inputText[0]) &&
                inputText[1] == ':' &&
                validCharacters.Contains(inputText[2]);
        }

        static Dictionary<char, char> GetMonoalphabeticMapping(string text)
        {

            int graphHeight = 20;

            Dictionary<char, char> currentMapping = new Dictionary<char, char>();

            foreach (char c in validCharacters)
            {

                currentMapping.Add(c, c);

            }

            bool editing = true;
            List<char> setChars = new List<char>();

            while (editing)
            {

                Console.WriteLine("\n\n");

                string currentDeciphering = DecipherTextByMapping(text, currentMapping);

                Dictionary<char, double> englishCharacterProportions = EnglishLetterFrequency.GetLetterProportions();

                Dictionary<char, double> selectionCharacterProportion = CharFrequencyToCharProportion(GetTextCharFrequency(FilteredInput(currentDeciphering)));

                PrintSeperator();
                Console.WriteLine("English Language Average Character Proportions (Target Proportions):");
                DrawConsoleCharFrequencyGraph(englishCharacterProportions,
                    graphHeight,
                    forcedContainChars: validCharacters);
                PrintSeperator();

                Console.WriteLine();
                PrintSeperator();
                Console.WriteLine("Current Character Proportions:");
                DrawConsoleCharFrequencyGraph(selectionCharacterProportion,
                    graphHeight,
                    forcedContainChars: validCharacters);
                PrintSeperator();

                Console.WriteLine();
                PrintSeperator();
                Console.WriteLine("Current Frequency Difference: " + GetKeyFrequencyDifference(selectionCharacterProportion, englishCharacterProportions));

                Console.Write("Set characters: ");
                foreach (char c in setChars) Console.Write(c + " ");
                Console.WriteLine();

                Console.WriteLine("Current Deciphering:");
                Console.WriteLine(currentDeciphering);
                PrintSeperator();

                Console.WriteLine("Request mapping swap ({character 1}:{character 2}) or leave blank to finish or +/- to increase/decrease graph magnification or \"(un)set {char}\" to set/unset a char (personal usage, no set behavior (apart from displaying))");
                Console.Write("> ");
                string inputRequest = Console.ReadLine().ToUpper();

                if (inputRequest.Length == 0)
                {
                    editing = false;
                }
                else if (InputIsCharacterRemapping(inputRequest))
                {

                    char requestCharA = inputRequest[0];
                    char requestCharB = inputRequest[2];

                    Debug.Assert(inputRequest[1] == ':');

                    char currentInputCharToRequestCharA = ' ';
                    char currentInputCharToRequestCharB = ' ';

                    foreach (char c in currentMapping.Keys)
                    {

                        if (currentMapping[c] == requestCharA) currentInputCharToRequestCharA = c;
                        if (currentMapping[c] == requestCharB) currentInputCharToRequestCharB = c;

                    }

                    if (currentInputCharToRequestCharA == ' ' ||
                       currentInputCharToRequestCharB == ' ')
                    {

                        Debug.Fail("Error: Failed to locate char mapping to request char");

                    }

                    char requestInputCharOldMapping = currentMapping[currentInputCharToRequestCharA];
                    currentMapping[currentInputCharToRequestCharA] = requestCharB;
                    currentMapping[currentInputCharToRequestCharB] = requestInputCharOldMapping;

                }
                else if (inputRequest == "+")
                {

                    graphHeight += 5;

                }
                else if (inputRequest == "-")
                {

                    if (graphHeight > 5) graphHeight -= 5;
                    else
                    {
                        Console.ForegroundColor = ConsoleColor.Yellow;
                        Console.WriteLine("Unable to further reduce graph height");
                        Console.ForegroundColor = ConsoleColor.White;
                    }

                }
                else if (Regex.IsMatch(inputRequest, "^SET [A-Z]$", RegexOptions.IgnoreCase)) setChars.Add(inputRequest[4]);
                else if (Regex.IsMatch(inputRequest, "^UNSET [A-Z]$", RegexOptions.IgnoreCase))
                {

                    char c = inputRequest[6];
                    if (setChars.Contains(c))
                    {
                        setChars.Remove(c);
                    }

                }
                else if (inputRequest == "PRINT MAPPING" || inputRequest == "PRINTMAPPING")
                {
                    PrintCharacterMapping(currentMapping);
                }
                else
                {

                    Console.WriteLine("Unknown request");

                }

                List<char> outputCharsUnique = new List<char>();
                List<char> warnedCharsNonUnique = new List<char>();
                foreach (char c in currentMapping.Keys)
                {

                    char outputChar = currentMapping[c];

                    if (outputCharsUnique.Contains(outputChar) &&
                        !warnedCharsNonUnique.Contains(outputChar))
                    {

                        Console.ForegroundColor = ConsoleColor.Red;
                        Console.WriteLine($"WARNING: duplicate mapping to character '{outputChar}'");
                        Console.ForegroundColor = ConsoleColor.White;
                        warnedCharsNonUnique.Add(outputChar);

                    }
                    else
                    {
                        outputCharsUnique.Add(outputChar);
                    }

                }

            }

            return currentMapping;

        }

        public static Dictionary<char, int> GetTextCharFrequency(string text)
        {

            Dictionary<char, int> charFrequency = new Dictionary<char, int>();

            foreach (char c in text)
            {

                if (charFrequency.ContainsKey(c))
                {

                    charFrequency[c]++;

                }
                else
                {

                    charFrequency.Add(c, 1);

                }

            }

            return charFrequency;

        }

        public static Dictionary<char, double> CharFrequencyToCharProportion(Dictionary<char, int> charFrequency)
        {

            int totalChars = 0;

            foreach (char c in charFrequency.Keys) totalChars += charFrequency[c];

            Dictionary<char, double> charProportion = new Dictionary<char, double>();

            foreach (char c in charFrequency.Keys)
            {

                charProportion[c] = (double)charFrequency[c] / totalChars;

            }

            return charProportion;

        }

        public static void DrawConsoleCharFrequencyGraph(Dictionary<char, double> charProportions,
            int graphCharacterHeight,
            char graphCharacter = '|',
            char[] forcedContainChars = null)
        {

            if (forcedContainChars != null)
            {
                foreach (char c in forcedContainChars)
                {

                    if (!charProportions.ContainsKey(c))
                    {
                        charProportions.Add(c, 0);
                    }

                }
            }

            char[,] graph = new char[graphCharacterHeight, charProportions.Count];

            List<KeyValuePair<char, double>> charProportionList = charProportions.ToList();
            charProportionList.Sort((valueA, valueB) => valueA.Key.CompareTo(valueB.Key));

            double maxValue = charProportionList[0].Value;

            foreach (KeyValuePair<char, double> pair in charProportionList)
            {

                if (pair.Value > maxValue)
                {
                    maxValue = pair.Value;
                }

            }

            for (int i = 0; i < charProportionList.Count; i++)
            {

                double normalizedValue = charProportionList[i].Value / maxValue;

                if (normalizedValue == 0) continue;

                int barTopIndex = graphCharacterHeight - (int)Math.Round(normalizedValue * (graphCharacterHeight - 1) + 1);

                for (int barIndex = barTopIndex; barIndex < graphCharacterHeight; barIndex++)
                {

                    graph[barIndex, i] = graphCharacter;

                }

            }

            const int barSpacing = 2;

            for (int y = 0; y < graph.GetLength(0); y++)
            {

                for (int x = 0; x < graph.GetLength(1); x++)
                {

                    Console.Write(graph[y, x]);

                    for (int i = 0; i < barSpacing; i++) Console.Write(' ');

                }

                Console.WriteLine();

            }

            foreach (KeyValuePair<char, double> pair in charProportionList)
            {

                Console.Write(pair.Key);
                for (int i = 0; i < barSpacing; i++) Console.Write(' ');

            }

            Console.WriteLine();

        }

        public static string DecipherTextByMapping(string text,
            Dictionary<char, char> mapping)
        {

            string outputText = "";

            foreach (char c in text)
            {

                if (mapping.ContainsKey(c))
                {
                    outputText += mapping[c];
                }
                else
                {
                    outputText += c;
                }

            }

            return outputText;

        }

        public static double GetKeyFrequencyDifference<T>(Dictionary<T, double> a, Dictionary<T, double> b)
        {

            double difference = 0;

            foreach (T key in a.Keys)
            {

                difference += Math.Abs(a[key] - b[key]);

            }

            return difference;

        }

        public static void PrintCharacterMapping(Dictionary<char, char> mapping)
        {

            foreach (char c in validCharacters)
            {

                Console.Write(c + " ");

            }
            Console.WriteLine();

            foreach (char c in validCharacters)
            {

                if (mapping.ContainsKey(c))
                {
                    Console.Write(mapping[c]);
                }
                else
                {
                    Console.Write(c);
                }

                Console.Write(" ");

            }

        }

    }
}
