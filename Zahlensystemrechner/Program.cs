﻿#region usings

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text.RegularExpressions;

#endregion

namespace Zahlensystemrechner
{
    internal class Program
    {
        /// <summary>
        /// Matches anything that is inside braces except other braces
        /// </summary>
        private static readonly Regex BRACES_PATTERN = new Regex("\\(([^()]+)\\)");

        /// <summary>
        /// Matches + or -, followed by any character between a-f and 0-9 or h, x, k and o, then matches exactly one operator (*,\,/,:) and then matches any character as previously again
        /// </summary>
        private static readonly Regex POINTS_EXPRESSION_PATTERN = new Regex("(\\+|-)?[a-fhyko0-9]+(\\*|\\|\\/|:){1}(\\+|-)?[a-fhyko0-9]+");

        /// <summary>
        /// As above, it matches + or -, followed by any character as above, then matches exactly one operator (+,-) and then matches any character as previously again
        /// </summary>
        private static readonly Regex LINE_EXPRESSION_PATTERN = new Regex("(\\+|-)?[a-fhxko0-9]+(\\+|-){1}(\\+|-)?[a-fhxko0-9]+");

        private static readonly List<Tuple<string, string, string>> Calculations =
            new List<Tuple<string, string, string>>();

        private static bool prefixEnabled;

        private static void Main(string[] args)
        {
            var calcing = true;
            while (calcing)
            {
                var input = GetNextInput();

                if (input == "list")
                {
                    PrintList();
                }
                else if (input == "exit")
                {
                    calcing = false;
                }
                else if (input == "clear")
                {
                    Console.Clear();
                }
                else if (input == "prefix")
                {
                    prefixEnabled = true;
                    Console.WriteLine("Switched to Prefix!");
                }
                else if (input == "suffix")
                {
                    prefixEnabled = false;
                    Console.WriteLine("Switched to Suffix!");
                }
                else
                {
                    try
                    {
                        var sw = Stopwatch.StartNew();
                        var result = Calculate(input);
                        sw.Stop();
                        var time = (sw.ElapsedTicks / (TimeSpan.TicksPerMillisecond / 1000f) / 1000f / 1000f).ToString("F99").TrimEnd('0');

                        PrintResult(input, result, time + " Sekunden");

                        Calculations.Add(new Tuple<string, string, string>(input, result, time + " Sekunden"));
                    }
                    catch (Exception e)
                    {
                        Console.WriteLine(e.Message);
                    }
                }
            }
        }

        private static void PrintList()
        {
            Console.WriteLine();

            List<Tuple<string, string, string, string, string, string>> calcs =
                new List<Tuple<string, string, string, string, string, string>>();

            foreach (Tuple<string, string, string> calculation in Calculations)
            {
                calcs.Add(Tuple.Create(calculation.Item1,
                    calculation.Item2,
                    ConvertToZahlensystem(int.Parse(calculation.Item2), ZahlensystemeEnum.DUAL),
                    ConvertToZahlensystem(int.Parse(calculation.Item2), ZahlensystemeEnum.OKTAL),
                    ConvertToZahlensystem(int.Parse(calculation.Item2), ZahlensystemeEnum.HEXADEZIMAL),
                    calculation.Item3));
            }

            Console.WriteLine(calcs.ToStringTable(
                new[] {"Rechnung", "Dezimal", "Binär", "Oktal", "Hexadezimal", "Zeit"},
                tuple => tuple.Item1,
                tuple => tuple.Item2, tuple => tuple.Item3, tuple => tuple.Item4, tuple => tuple.Item5,
                tuple => tuple.Item6));
        }

        private static void PrintResult(string input, string result, string time)
        {
            Console.WriteLine();

            IEnumerable<Tuple<string, string, string, string, string, string>> results = new[]
            {
                Tuple.Create(input,
                    result,
                    ConvertToZahlensystem(int.Parse(result), ZahlensystemeEnum.DUAL),
                    ConvertToZahlensystem(int.Parse(result), ZahlensystemeEnum.OKTAL),
                    ConvertToZahlensystem(int.Parse(result), ZahlensystemeEnum.HEXADEZIMAL),
                    time)
            };

            Console.WriteLine(results.ToStringTable(
                new[] {"Rechnung", "Dezimal", "Binär", "Oktal", "Hexadezimal", "Zeit"},
                tuple => tuple.Item1,
                tuple => tuple.Item2, tuple => tuple.Item3, tuple => tuple.Item4, tuple => tuple.Item5,
                tuple => tuple.Item6));

            //Console.WriteLine("Das Ergebnis deiner Rechnung beträgt: ");
            //Console.WriteLine("Dezimal\t\tBinär\t\tOktal\t\tHexadezimal");
            //Console.WriteLine(result + "\t\t" +
            //                  ConvertToZahlensystem(int.Parse(result), Zahlensysteme.Dual) + "\t\t" +
            //                  ConvertToZahlensystem(int.Parse(result), Zahlensysteme.Oktal) + "\t\t" +
            //                  ConvertToZahlensystem(int.Parse(result), Zahlensysteme.Hexadezimal));
            Console.WriteLine();
        }

        private static string GetNextInput()
        {
            Console.Write("Bitte gib deine Rechnung ein: ");
            return Console.ReadLine()?.ToLower().Replace(" ", "");
        }

        /// <summary>
        /// Calculates the specified input.
        /// </summary>
        /// <param name="input">The input.</param>
        /// <returns></returns>
        private static string Calculate(string input)
        {
            var braces = BRACES_PATTERN.Matches(input);

            while (braces.Count > 0)
            {
                foreach (Match brace in braces)
                {
                    // 1 is the captured group, 0 is the whole regex matched
                    var result = Calculate(brace.Groups[1].Value);

                    input = input.Replace(brace.Groups[0].Value, result);
                }

                braces = BRACES_PATTERN.Matches(input);
            }

            var points = POINTS_EXPRESSION_PATTERN.Matches(input);

            while (points.Count > 0)
            {
                foreach (Match point in points)
                {
                    var value = point.Groups[0].Value;
                    var operation = value.Contains("*") ? OperationsEnum.MULTIPLY : OperationsEnum.DIVIDE;
                    var parts = operation == OperationsEnum.MULTIPLY ? value.Split('*') : value.Split(':', '\\', '/');

                    var operandOne = parts[0];
                    var operandTwo = parts[1];
                    var numberOne = PrepareOperand(operandOne);
                    var numberTwo = PrepareOperand(operandTwo);
                    var result = operation == OperationsEnum.MULTIPLY ? numberOne * numberTwo : numberOne / numberTwo;

                    input = input.Replace(value, result.ToString());
                }

                points = POINTS_EXPRESSION_PATTERN.Matches(input);
            }

            var lines = LINE_EXPRESSION_PATTERN.Matches(input);

            while (lines.Count > 0)
            {
                foreach (Match point in lines)
                {
                    var value = point.Groups[0].Value;
                    var operation = value.Contains("+") ? OperationsEnum.PLUS : OperationsEnum.MINUS;
                    var parts = operation == OperationsEnum.PLUS ? value.Split('+') : value.Split('-');

                    // The split will remove the +/- of the operands as well, we need to fix that
                    if (parts.Length > 2)
                    {
                        // We can have a maximum number of 4 parts (realistically) if all operators were -
                        if (string.IsNullOrEmpty(parts[0]))
                        {
                            parts[0] = (operation == OperationsEnum.PLUS ? "+" : "-") + parts[1];

                            // We need to check offset by one since the first "operand" took two "spots" in the array
                            if (string.IsNullOrEmpty(parts[2]))
                            {
                                parts[1] = (operation == OperationsEnum.PLUS ? "+" : "-") + parts[3];
                            }
                        }
                        else if (string.IsNullOrEmpty(parts[1]))
                        {
                            parts[1] = (operation == OperationsEnum.PLUS ? "+" : "-") + parts[2];
                        }
                    }

                    var operandOne = parts[0];
                    var operandTwo = parts[1];
                    var numberOne = PrepareOperand(operandOne);
                    var numberTwo = PrepareOperand(operandTwo);

                    var result = operation == OperationsEnum.PLUS ? numberOne + numberTwo : numberOne - numberTwo;

                    input = input.Replace(value, result.ToString());
                }

                lines = LINE_EXPRESSION_PATTERN.Matches(input);
            }

            return input;
        }

        /// <summary>
        /// Prepares the operand to calculate it (strip off +/-, convert it to dezimal etc)
        /// </summary>
        /// <param name="operand">The operand.</param>
        /// <returns></returns>
        private static int PrepareOperand(string operand)
        {
            var prepositionOne = operand.StartsWith("+") ? "+" : operand.StartsWith("-") ? "-" : "";

            if (!string.IsNullOrEmpty(prepositionOne))
            {
                operand = operand.Replace("+", "").Replace("-", "");
            }

            var number = ConvertToDecizmal(operand);

            if (prepositionOne == "-")
            {
                // Convert number to negative by substracing two times the number (first to 0, then to negative)
                number = number - (number * 2);
            }

            return number;
        }

        /// <summary>
        /// Converts a given string representation of a number in any Zahlensystem to dezimal
        /// </summary>
        /// <param name="input">The input.</param>
        /// <returns></returns>
        private static int ConvertToDecizmal(string input)
        {
            var zahlensystem = GetZahlensystem(input);

            switch (zahlensystem)
            {
                case ZahlensystemeEnum.DEZIMAL:
                {
                    return int.Parse(input);
                }

                case ZahlensystemeEnum.DUAL:
                {
                    input = input.Replace(prefixEnabled ? "0d" : "d", "");

                    var output = 0;

                    for (int i = 0; i < input.Length; i++)
                    {
                        output += (int) Math.Round(Math.Pow(2,
                                                       (input.Length - 1 - i)) * char.GetNumericValue(input[i]));
                    }

                    return output;
                }

                case ZahlensystemeEnum.OKTAL:
                {
                    input = input.Replace(prefixEnabled ? "0k" : "o", "");

                    var output = 0;

                    for (int i = 0; i < input.Length; i++)
                    {
                        output += (int) Math.Round(Math.Pow(8,
                                                       (input.Length - 1 - i)) * char.GetNumericValue(input[i]));
                    }

                    return output;
                }

                case ZahlensystemeEnum.HEXADEZIMAL:
                {
                    input = input.Replace(prefixEnabled ? "0x" : "h", "");

                    var output = 0;

                    for (int i = 0; i < input.Length; i++)
                    {
                        var zahl = 0;

                        if (input[i] == 'a')
                        {
                            zahl = 10;
                        }
                        else if (input[i] == 'b')
                        {
                            zahl = 11;
                        }
                        else if (input[i] == 'c')
                        {
                            zahl = 12;
                        }
                        else if (input[i] == 'd')
                        {
                            zahl = 13;
                        }
                        else if (input[i] == 'e')
                        {
                            zahl = 14;
                        }
                        else if (input[i] == 'f')
                        {
                            zahl = 15;
                        }
                        else
                        {
                            zahl = (int) char.GetNumericValue(input[i]);
                        }

                        output += (int) Math.Round(Math.Pow(16,
                                                       (input.Length - 1 - i)) * zahl);
                    }

                    return output;
                }

                case ZahlensystemeEnum.INVALID:
                {
                    throw new ArgumentException("Could not determine Zahlensystem!");
                }
            }

            return 0;
        }

        /// <summary>
        /// Converts a dezimal value to the specified zahlensystem
        /// </summary>
        /// <param name="dezimal">The dezimal.</param>
        /// <param name="zahlensystem">The zahlensystem.</param>
        /// <returns></returns>
        private static string ConvertToZahlensystem(int dezimal, ZahlensystemeEnum zahlensystem)
        {
            switch (zahlensystem)
            {
                case ZahlensystemeEnum.DUAL:
                {
                    var output = "";

                    while (dezimal > 0)
                    {
                        output = (dezimal % 2) + output;
                        dezimal /= 2;
                    }

                    return prefixEnabled ? "0d" + output : output + "d";
                }

                case ZahlensystemeEnum.OKTAL:
                {
                    var output = "";

                    while (dezimal > 0)
                    {
                        output = (dezimal % 8) + output;
                        dezimal /= 8;
                    }

                    return prefixEnabled ? "0k" + output : output + "o";
                }

                case ZahlensystemeEnum.HEXADEZIMAL:
                {
                    var output = "";

                    while (dezimal > 0)
                    {
                        var rest = dezimal % 16;

                        if (rest == 10)
                        {
                            output = 'a' + output;
                        }
                        else if (rest == 11)
                        {
                            output = 'b' + output;
                        }
                        else if (rest == 12)
                        {
                            output = 'c' + output;
                        }
                        else if (rest == 13)
                        {
                            output = 'd' + output;
                        }
                        else if (rest == 14)
                        {
                            output = 'e' + output;
                        }
                        else if (rest == 15)
                        {
                            output = 'f' + output;
                        }
                        else
                        {
                            output = rest + output;
                        }

                        dezimal /= 16;
                    }

                    return prefixEnabled ? "0x" + output : output + "h";
                }
            }

            return dezimal.ToString();
        }

        /// <summary>
        /// Gets the zahlensystem of a string representation of a number
        /// </summary>
        /// <param name="input">The input.</param>
        /// <returns></returns>
        private static ZahlensystemeEnum GetZahlensystem(string input)
        {
            if (((prefixEnabled && input.StartsWith("0d")) || (!prefixEnabled && input.EndsWith("d"))) &&
                input.All(c => "01d".Contains(c)))
            {
                return ZahlensystemeEnum.DUAL;
            }

            if (((prefixEnabled && input.StartsWith("0k")) || (!prefixEnabled && input.EndsWith("o"))) &&
                input.All(c => "01234567ok".Contains(c)))
            {
                return ZahlensystemeEnum.OKTAL;
            }

            if (((prefixEnabled && input.StartsWith("0x")) || (!prefixEnabled && input.EndsWith("h"))) &&
                input.All(c => "0123456789abcdefhx".Contains(c)))
            {
                return ZahlensystemeEnum.HEXADEZIMAL;
            }

            if (input.All(c => "0123456789".Contains(c)))
            {
                return ZahlensystemeEnum.DEZIMAL;
            }

            return ZahlensystemeEnum.INVALID;
        }
    }

    internal enum ZahlensystemeEnum
    {
        DUAL,
        OKTAL,
        DEZIMAL,
        HEXADEZIMAL,
        INVALID
    }

    internal enum OperationsEnum
    {
        PLUS,
        MINUS,
        MULTIPLY,
        DIVIDE
    }
}