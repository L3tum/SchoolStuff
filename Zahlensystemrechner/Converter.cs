using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Zahlensystemrechner
{
    class Converter
    {
        internal static bool prefixEnabled = false;

        /// <summary>
        /// Converts a given string representation of a number in any Zahlensystem to dezimal
        /// </summary>
        /// <param name="input">The input.</param>
        /// <returns></returns>
        /// <exception cref="ArgumentException">Could not determine Zahlensystem!</exception>
        internal static int ConvertToDecizmal(string input)
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
                            output += (int)Math.Round(Math.Pow(2,
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
                            output += (int)Math.Round(Math.Pow(8,
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
                                zahl = (int)char.GetNumericValue(input[i]);
                            }

                            output += (int)Math.Round(Math.Pow(16,
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
        internal static string ConvertToZahlensystem(int dezimal, ZahlensystemeEnum zahlensystem)
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
}
