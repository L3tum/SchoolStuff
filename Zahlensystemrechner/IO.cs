#region usings

using System;
using System.Collections.Generic;
using System.Diagnostics;

#endregion

namespace Zahlensystemrechner
{
	internal class IO
	{
		private static readonly List<Tuple<string, string, string>> Calculations =
			new List<Tuple<string, string, string>>();

		internal static void Run()
		{
			var input = "";
			while (input != "exit")
			{
				input = GetNextInput();

				switch (input)
				{
					case "list":
					{
						PrintList();
						break;
					}
					case "exit":
					{
						break;
					}
					case "clear":
					{
						Console.Clear();
						break;
					}
					case "clearlist":
					{
						Calculations.Clear();
						break;
					}
					case "prefix":
					{
						Converter.prefixEnabled = true;
						Console.WriteLine("Switched to Prefix!");
						break;
					}
					case "suffix":
					{
						Converter.prefixEnabled = false;
						Console.WriteLine("Switched to Suffix!");
						break;
					}
					default:
					{
						var sw = Stopwatch.StartNew();
						var result = Calculator.Run(input);

						sw.Stop();

						var time = (sw.ElapsedTicks / (TimeSpan.TicksPerMillisecond / 1000f) / 1000f / 1000f)
							.ToString("F99").TrimEnd('0');

						PrintResult(input, result, time + " Sekunden");

						Calculations.Add(new Tuple<string, string, string>(input, result, time + " Sekunden"));

						break;
					}
				}
			}
		}

		/// <summary>
		/// Prints the list.
		/// </summary>
		private static void PrintList()
		{
			Console.WriteLine();

			List<Tuple<string, string, string, string, string, string>> calcs =
				new List<Tuple<string, string, string, string, string, string>>();

			foreach (Tuple<string, string, string> calculation in Calculations)
			{
				calcs.Add(Tuple.Create(calculation.Item1,
					calculation.Item2,
					Converter.ConvertToZahlensystem(int.Parse(calculation.Item2), ZahlensystemeEnum.DUAL),
					Converter.ConvertToZahlensystem(int.Parse(calculation.Item2), ZahlensystemeEnum.OKTAL),
					Converter.ConvertToZahlensystem(int.Parse(calculation.Item2), ZahlensystemeEnum.HEXADEZIMAL),
					calculation.Item3));
			}

			Console.WriteLine(calcs.ToStringTable(
				new[] {"Rechnung", "Dezimal", "Binär", "Oktal", "Hexadezimal", "Zeit"},
				tuple => tuple.Item1,
				tuple => tuple.Item2, tuple => tuple.Item3, tuple => tuple.Item4, tuple => tuple.Item5,
				tuple => tuple.Item6));
		}

		/// <summary>
		/// Prints the result.
		/// </summary>
		/// <param name="input">The input.</param>
		/// <param name="result">The result.</param>
		/// <param name="time">The time.</param>
		private static void PrintResult(string input, string result, string time)
		{
			Console.WriteLine();

			IEnumerable<Tuple<string, string, string, string, string, string>> results = new[]
			{
				Tuple.Create(input,
					result,
					Converter.ConvertToZahlensystem(int.Parse(result), ZahlensystemeEnum.DUAL),
					Converter.ConvertToZahlensystem(int.Parse(result), ZahlensystemeEnum.OKTAL),
					Converter.ConvertToZahlensystem(int.Parse(result), ZahlensystemeEnum.HEXADEZIMAL),
					time)
			};

			Console.WriteLine(results.ToStringTable(
				new[] {"Rechnung", "Dezimal", "Binär", "Oktal", "Hexadezimal", "Zeit"},
				tuple => tuple.Item1,
				tuple => tuple.Item2, tuple => tuple.Item3, tuple => tuple.Item4, tuple => tuple.Item5,
				tuple => tuple.Item6));

			Console.WriteLine();
		}

		/// <summary>
		/// Gets the next input.
		/// </summary>
		/// <returns></returns>
		private static string GetNextInput()
		{
			Console.Write("Bitte gib deine Rechnung ein: ");
			return Console.ReadLine()?.ToLower().Replace(" ", "");
		}
	}
}