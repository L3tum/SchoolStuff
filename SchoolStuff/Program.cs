#region usings

using System;
using System.Linq;

#endregion

namespace SchoolStuff
{
    internal class Program
    {
        private static void Main(string[] args)
        {
            while (true)
            {
                Console.Write("Bitte Geben Sie eine obere Grenze ein: ");
                int n = 2;
                int oberGrenze = Convert.ToInt32(Console.ReadLine());
                int numbers = oberGrenze;

                // Second parameter is the number of elements, so max "index" - 1, otherwise we'd have one extra (e.g. 62 instead of 61)
                int[] zahlenArray = Enumerable.Range(2, oberGrenze - 1).ToArray();

                while (n <= Math.Sqrt(oberGrenze))
                {
                    int[] nextZahlenArray = new int[oberGrenze];
                    int c = 0;
                    int nextN = n;

                    for (int i = 0; i < zahlenArray.Length; i++)
                    {
                        // "Uncopied/Empty" entries in the array are 0. Since we start at 2 we can safely assume that any zero is not one of our numbers and shouldn't be copied
                        // Number MOD n != 0 ensures that number is not a multiple of n
                        // We do need to copy n however
                        if (zahlenArray[i] != 0 && (zahlenArray[i] % n != 0 || zahlenArray[i] == n))
                        {
                            nextZahlenArray[c] = zahlenArray[i];
                            c++;

                            if (nextN == n && zahlenArray[i] > n)
                            {
                                nextN = zahlenArray[i];
                            }
                        }
                    }

                    numbers = c;
                    n = nextN;
                    zahlenArray = nextZahlenArray;
                }

                Array.Resize(ref zahlenArray, numbers);

                Console.WriteLine("Die folgenden Primzahlen wurden zwischen 2 und {0} gefunden:", oberGrenze);
                Console.WriteLine(string.Join(", ", zahlenArray));

                // This is extra
                Console.WriteLine();
                Console.WriteLine("Nochmal? [y/n]");

                if (Console.ReadKey(true).Key == ConsoleKey.Y)
                {
                    Console.WriteLine();
                }
                else
                {
                    break;
                }
            }
        }
    }
}