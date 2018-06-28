#region usings

using System;
using System.Linq;
using System.Text.RegularExpressions;

#endregion

namespace Zahlensystemrechner
{
    internal class Calculator
    {
        /// <summary>
        /// Matches anything that is inside braces except other braces
        ///                                                             G1
        /// </summary>
        private static readonly Regex BRACES_PATTERN = new Regex("\\(([^()]+)\\)");

        /// <summary>
        /// Matches + or -, followed by any character between a-f and 0-9 or h, x, k and o, then matches exactly one operator (*,\,/,:) and then matches any character as previously again
        /// 5*5;5;*;5
        /// </summary>
        private static readonly Regex POINTS_EXPRESSION_PATTERN =
            new Regex("((?:\\+|-)?[a-fhyko0-9]+)(\\*|\\|\\/|:){1}((?:\\+|-)?[a-fhyko0-9]+)");

        /// <summary>
        /// As above, it matches + or -, followed by any character as above, then matches exactly one operator (+,-) and then matches any character as previously again
        /// 5+5;5;+;5
        /// </summary>
        private static readonly Regex LINE_EXPRESSION_PATTERN =
            new Regex("((?:\\+|-)?[a-fhxko0-9]+)(\\+|-){1}((?:\\+|-)?[a-fhxko0-9]+)");

        internal static string Run(string input)
        {
            try
            {
                var result = Calculate(input);

                return result;
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
            }

            return input;
        }

        /// <summary>
        /// Calculates the specified input.
        /// </summary>
        /// <param name="input">The input.</param>
        /// <returns></returns>
        private static string Calculate(string input)
        {
            return CalculateLines(CalculatePoints(CalculateBraces(input)));
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

            var number = Converter.ConvertToDecizmal(operand);

            if (prepositionOne == "-")
            {
                // Convert number to negative by substracing two times the number (first to 0, then to negative)
                number = number - (number * 2);
            }

            return number;
        }

        /// <summary>
        /// Calculates the lines.
        /// </summary>
        /// <param name="input">The input.</param>
        /// <returns></returns>
        private static string CalculateLines(string input)
        {
            var lines = LINE_EXPRESSION_PATTERN.Matches(input);

            while (lines.Count > 0)
            {
                foreach (Match point in lines)
                {
                    var operation = point.Groups[2].Value == "+" ? OperationsEnum.PLUS : OperationsEnum.MINUS;
                    var numberOne = PrepareOperand(point.Groups[1].Value);
                    var numberTwo = PrepareOperand(point.Groups[3].Value);

                    var result = operation == OperationsEnum.PLUS ? numberOne + numberTwo : numberOne - numberTwo;

                    input = input.Replace(point.Value, result.ToString());
                }

                lines = LINE_EXPRESSION_PATTERN.Matches(input);
            }

            return input;
        }

        /// <summary>
        /// Calculates the points.
        /// </summary>
        /// <param name="input">The input.</param>
        /// <returns></returns>
        private static string CalculatePoints(string input)
        {
            var points = POINTS_EXPRESSION_PATTERN.Matches(input);

            while (points.Count > 0)
            {
                foreach (Match point in points)
                {
                    var operation = point.Groups[2].Value == "*" ? OperationsEnum.MULTIPLY : OperationsEnum.DIVIDE;
                    var numberOne = PrepareOperand(point.Groups[1].Value);
                    var numberTwo = PrepareOperand(point.Groups[3].Value);
                    var result = operation == OperationsEnum.MULTIPLY ? numberOne * numberTwo : numberOne / numberTwo;

                    input = input.Replace(point.Value, result.ToString());
                }

                points = POINTS_EXPRESSION_PATTERN.Matches(input);
            }

            return input;
        }

        /// <summary>
        /// Calculates the braces.
        /// </summary>
        /// <param name="input">The input.</param>
        /// <returns></returns>
        private static string CalculateBraces(string input)
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

            return input;
        }
    }

    internal enum OperationsEnum
    {
        PLUS,
        MINUS,
        MULTIPLY,
        DIVIDE
    }
}