#region usings

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using ISBNUtility;
using TableParser;

#endregion

namespace ISBNRunner
{
	internal class Program
	{
		// ISBN10, ISBN13, time
		private static readonly List<Tuple<string, string, string>> history = new List<Tuple<string, string, string>>();
		private static readonly ISBN isbn = new ISBN();

		private static void Main(string[] args)
		{
			var input = "";

			while (input.ToLower() != "exit")
			{
				switch (input.ToLower())
				{
					case "help":
					{
						Console.WriteLine(
							"Verfügbare Commands: help, history, clearHistory, clear, author, publisher, info.");
						Console.WriteLine(
							"Oder eine ISBN in ISBN10 oder ISBN13 Format, welche in das jeweilig andere konvertiert werden soll.");
						Console.WriteLine("Oder einen Titel, für den die ISBN gefunden werden soll.");
						Console.WriteLine();
						break;
					}
					case "history":
					{
						Console.WriteLine(history.ToStringTable(new[] {"ISBN10", "ISBN13", "Time(Sek)"},
							tuple => tuple.Item1, tuple => tuple.Item2, tuple => tuple.Item3));
						Console.WriteLine();
						break;
					}
					case "clearHistory":
					{
						history.Clear();
						break;
					}
					case "clear":
					{
						Console.Clear();
						break;
					}
					case "info":
					{
						Console.Write("Bitte geben Sie ISBN oder Titel des Buches ein: ");
						var info = Console.ReadLine();
						Console.WriteLine();

						Console.Write($"Fetching information for \"{info}\"...");
						var sw = Stopwatch.StartNew();
						var task = isbn.GetInfoFromSearch(info);

						DisplayLoading(task);

						sw.Stop();
						var elapsedSeconds =
							(sw.ElapsedTicks / (TimeSpan.TicksPerMillisecond / 1000f) / 1000f / 1000f)
							.ToString("F99").TrimEnd('0');

						var doc = task.Result;

						if (doc.isbn.Count == 0)
						{
							PrintNotFound(info);
							Console.WriteLine();

							break;
						}

						var isbn10 = doc.isbn[0];
						var isbn13 = "";
						var version = isbn.GetISBNVersion(isbn10);

						if (version == VERSION.ISBN10)
						{
							isbn13 = isbn.ConvertISBN(isbn10).ToUpper();
							isbn10 = isbn.HyphenateISBN(isbn10);
						}
						else
						{
							isbn13 = isbn10;
							isbn10 = isbn.ConvertISBN(isbn13).ToUpper();
							isbn13 = isbn.HyphenateISBN(isbn13);
						}

						Console.WriteLine();
						Console.WriteLine($"Das Buch \"{doc.title}\" hat folgende Eigenschaften: ");
						Console.Write("ISBN10: ");
						PrintISBN10(isbn10);
						Console.WriteLine(",");
						Console.Write("ISBN13: ");
						PrintISBN13(isbn13);
						Console.WriteLine(",");
						Console.WriteLine($"Author: \"{string.Join(", ", doc.author_name)}\",");
						Console.WriteLine($"Publisher: \"{string.Join(", ", doc.publisher)}\",");
						Console.WriteLine(
							$"Es wurde zuerst in {doc.first_publish_year} und zuletzt in {doc.publish_year.Last()} veröffentlicht.");
						Console.WriteLine();

						history.Add(new Tuple<string, string, string>(isbn10, isbn13, elapsedSeconds));

						break;
					}
					case "author":
					{
						Console.Write("Bitte geben Sie ISBN oder Titel des Buches ein: ");
						var info = Console.ReadLine();
						Console.WriteLine();
						var version = isbn.GetISBNVersion(info);

						Console.Write($"Fetching author for \"{info}\"...");
						var task = isbn.GetAuthorFromSearch(info);

						if (version != VERSION.INVALID)
						{
							info = isbn.HyphenateISBN(info);
						}

						DisplayLoading(task);

						var authors = task.Result;

						if (authors.Length > 1)
						{
							Console.WriteLine(
								"Mehrere zutreffende Bücher gefunden! Versuche, präzisere Suchen zu machen.");
							Console.WriteLine();
						}
						else if (authors.Length == 0)
						{
							PrintNotFound(info);
							Console.WriteLine();

							break;
						}

						if (version == VERSION.INVALID)
						{
							Console.WriteLine($"Der Author des Buchs \"{info}\" ist \"{authors[0]}\".");
						}
						else if (version == VERSION.ISBN13)
						{
							Console.Write("Der Author des Buchs \"");
							PrintISBN13(info);
							Console.WriteLine($"\" ist \"{authors[0]}\".");
						}
						else if (version == VERSION.ISBN10)
						{
							Console.Write("Der Author des Buchs \"");
							PrintISBN10(info);
							Console.WriteLine($"\" ist \"{authors[0]}\".");
						}

						Console.WriteLine();

						WorkWithISBN(info);

						break;
					}
					case "publisher":
					{
						Console.Write("Bitte geben Sie ISBN oder Titel des Buches ein: ");
						var info = Console.ReadLine();
						Console.WriteLine();
						var version = isbn.GetISBNVersion(info);

						Console.Write($"Fetching publisher for \"{info}\"...");
						var task = isbn.GetPublisherFromSearch(info);

						if (version != VERSION.INVALID)
						{
							info = isbn.HyphenateISBN(info);
						}

						DisplayLoading(task);

						var publishers = task.Result;

						if (publishers.Length > 1)
						{
							Console.WriteLine(
								"Mehrere zutreffende Bücher gefunden! Versuche, präzisere Suchen zu machen.");
							Console.WriteLine();
						}
						else if (publishers.Length == 0)
						{
							PrintNotFound(info);
							Console.WriteLine();

							break;
						}

						if (version == VERSION.INVALID)
						{
							Console.WriteLine($"Der Publisher des Buchs \"{info}\" ist \"{publishers[0]}\".");
						}
						else if (version == VERSION.ISBN13)
						{
							Console.Write("Der Publisher des Buchs \"");
							PrintISBN13(info);
							Console.WriteLine($"\" ist \"{publishers[0]}\".");
						}
						else if (version == VERSION.ISBN10)
						{
							Console.Write("Der Publisher des Buchs \"");
							PrintISBN10(info);
							Console.WriteLine($"\" ist \"{publishers[0]}\".");
						}

						Console.WriteLine();

						WorkWithISBN(info);

						break;
					}

					default:
					{
						WorkWithISBN(input);

						break;
					}
				}

				Console.Write("Bitte geben Sie ein Command ein: ");
				input = Console.ReadLine();
				Console.WriteLine();
			}
		}

		private static void PrintNotFound(string info)
		{
			var version = isbn.GetISBNVersion(info);

			if (version == VERSION.INVALID)
			{
				Console.WriteLine($"Keine Bücher gefunden, die mit \"{info}\" übereinstimmen!");
			}
			else if (version == VERSION.ISBN13)
			{
				Console.Write("Keine Bücher gefunden, die mit \"");
				PrintISBN13(info);
				Console.WriteLine("\" übereinstimmen!");
			}
			else if (version == VERSION.ISBN10)
			{
				Console.Write("Keine Bücher gefunden, die mit \"");
				PrintISBN10(info);
				Console.WriteLine("\" übereinstimmen!");
			}
		}

		private static void WorkWithISBN(string input)
		{
			if (!string.IsNullOrWhiteSpace(input))
			{
				try
				{
					var version = isbn.GetISBNVersion(input);

					var sw = Stopwatch.StartNew();
					string converted = "";

					if (version == VERSION.INVALID)
					{
						Console.Write($"Fetching ISBN for \"{input}\"...");
						var task = isbn.GetISBNFromSearch(input);

						DisplayLoading(task);

						var isbns = task.Result;

						if (isbns.Length > 1)
						{
							Console.WriteLine(
								"Mehrere zutreffende Bücher gefunden! Versuche, präzisere Suchen zu machen.");
							Console.WriteLine();
						}
						else if (isbns.Length == 0)
						{
							Console.WriteLine(
								$"Keine Bücher gefunden, die mit \"{input}\" übereinstimmen!");
							Console.WriteLine();

							return;
						}

						input = isbns[0];

						version = isbn.GetISBNVersion(input);
						converted = isbn.ConvertISBN(input).ToUpper();
					}
					else
					{
						converted = isbn.ConvertISBN(input).ToUpper();
					}

					sw.Stop();
					var elapsedSeconds =
						(sw.ElapsedTicks / (TimeSpan.TicksPerMillisecond / 1000f) / 1000f / 1000f)
						.ToString("F99").TrimEnd('0');


					input = isbn.HyphenateISBN(input);

					if (version == VERSION.ISBN10)
					{
						history.Insert(0,
							new Tuple<string, string, string>(input, converted, elapsedSeconds));
						Console.Write("Die ISBN10 ");
						PrintISBN10(input);
						Console.Write(" ist in ISBN13 ");
						PrintISBN13(converted);
						Console.WriteLine($" und wurde in {elapsedSeconds} Sek. berechnet.");
					}
					else
					{
						history.Insert(0,
							new Tuple<string, string, string>(converted, input, elapsedSeconds));
						Console.Write("Die ISBN13 ");
						PrintISBN13(input);
						Console.Write(" ist in ISBN10 ");
						PrintISBN10(converted);
						Console.WriteLine($" und wurde in {elapsedSeconds} Sek. berechnet.");
					}

					Console.WriteLine();
				}
				catch (FormatException e)
				{
					Console.WriteLine(e.Message);
					Console.WriteLine();
				}
			}
		}

		private static void PrintISBN13(string isbn)
		{
			// Print prefix and then just print it as a ISBN10
			var normalColor = Console.ForegroundColor;
			Console.ForegroundColor = ConsoleColor.Yellow;
			Console.Write(isbn.Substring(0, 3));
			Console.ForegroundColor = normalColor;
			Console.Write("-");

			// Remove both prefix and '-'
			isbn = isbn.Substring(4);
			PrintISBN10(isbn);
		}

		private static void PrintISBN10(string isbn)
		{
			var parts = isbn.Split("-");
			var normalColor = Console.ForegroundColor;

			for (var i = 0; i < parts.Length; i++)
			{
				switch (i)
				{
					case 0:
					{
						// Group (Red)
						Console.ForegroundColor = ConsoleColor.Red;
						Console.Write(parts[i]);
						Console.ForegroundColor = normalColor;
						Console.Write("-");
						break;
					}
					case 1:
					{
						// Publisher (Green)
						Console.ForegroundColor = ConsoleColor.Green;
						Console.Write(parts[i]);
						Console.ForegroundColor = normalColor;
						Console.Write("-");
						break;
					}
					case 2:
					{
						// Title (Blue)
						Console.ForegroundColor = ConsoleColor.Cyan;
						Console.Write(parts[i]);
						Console.ForegroundColor = normalColor;
						Console.Write("-");
						break;
					}
					case 3:
					{
						// Checksum (Purple)
						Console.ForegroundColor = ConsoleColor.Magenta;
						Console.Write(parts[i]);
						Console.ForegroundColor = normalColor;
						break;
					}
				}
			}
		}

		private static void DisplayLoading(Task task)
		{
			var spinner = new ConsoleSpinner();

			Console.CursorVisible = false;
			while (!task.IsCompleted)
			{
				spinner.Turn();
				Thread.Sleep(100);
			}

			Console.Write(" ");
			Console.CursorVisible = true;
			Console.WriteLine();
		}
	}
}