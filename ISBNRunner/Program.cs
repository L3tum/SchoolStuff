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
							"Verfügbare Commands: help, history, clearHistory, clear, author, publisher, land, verlag, info, erstelleISBN.");
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
					case "clearhistory":
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

						Console.Write("Fetching information for \"{0}\"...", info);
						var sw = Stopwatch.StartNew();
						var task = isbn.GetInfoFromSearch(info);

						DisplayLoading(task);

						sw.Stop();
						var elapsedSeconds =
							(sw.ElapsedTicks / (TimeSpan.TicksPerMillisecond / 1000f) / 1000f / 1000f)
							.ToString("F99").TrimEnd('0');

						var doc = task.Result;

						if (string.IsNullOrWhiteSpace(doc.title_suggest))
						{
							PrintNotFound(info);
							Console.WriteLine();

							break;
						}

						var isbn10 = "None";
						var isbn13 = "None";
						if (doc.isbn != null && doc.isbn.Count > 0)
						{
							isbn10 = doc.isbn[0];
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
						}

						Console.WriteLine();
						WrapLine("Das Buch \"{0}\" hat folgende Eigenschaften: ", doc.title);
						PrintKey("ISBN10: ");
						if (isbn10 == "None")
						{
							Console.Write("Keine vergeben");
						}
						else
						{
							PrintISBN10(isbn10);
						}

						Console.WriteLine(",");
						PrintKey("ISBN13: ");
						if (isbn13 == "None")
						{
							Console.Write("Keine vergeben");
						}
						else
						{
							PrintISBN13(isbn13);
						}

						Console.WriteLine(",");
						if (doc.person != null && doc.person.Count > 0)
						{
							PrintKey("Originalauthor: ");
							WrapLine("\"{0}\",", string.Join("; ", doc.person));
						}

						PrintKey("Author: ");
						WrapLine("\"{0}\",", string.Join("; ", doc.author_name));

						if (doc.place != null && doc.place.Count > 0)
						{
							PrintKey("Geschrieben in: ");
							WrapLine("\"{0}\",", string.Join("; ", doc.place));
						}

						if (doc.contributor != null && doc.contributor.Count > 0)
						{
							PrintKey("Mitwirkende: ");
							WrapLine("\"{0}\",", string.Join("; ", doc.contributor));
						}

						PrintKey("Publisher: ");
						WrapLine("\"{0}\",", string.Join("; ", doc.publisher));

						if (doc.publish_place != null && doc.publish_place.Count > 0)
						{
							PrintKey("Publiziert in: ");
							WrapLine("\"{0}\",", string.Join("; ", doc.publish_place));
						}

						var oldColor = Console.ForegroundColor;
						Console.ForegroundColor = ConsoleColor.Cyan;
						WrapLine("Zuerst im Jahre {0} und zuletzt im Jahre {1} veröffentlicht.",
							doc.first_publish_year, doc.publish_year.Last());
						Console.ForegroundColor = oldColor;
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

						Console.Write("Fetching author for \"{0}\"...", info);
						var task = isbn.GetAuthorFromSearch(info);

						if (version != VERSION.INVALID)
						{
							info = isbn.HyphenateISBN(info);
						}

						DisplayLoading(task);

						var authors = task.Result;

						if (authors.Length > 1)
						{
							WrapLine(
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
							WrapLine("Der Author des Buchs \"{0}\" ist \"{1}\".", info, authors[0]);
						}
						else if (version == VERSION.ISBN13)
						{
							Console.Write("Der Author des Buchs \"");
							PrintISBN13(info);
							WrapLine("\" ist \"{0}\".", authors[0]);
						}
						else if (version == VERSION.ISBN10)
						{
							Console.Write("Der Author des Buchs \"");
							PrintISBN10(info);
							WrapLine("\" ist \"{0}\".", authors[0]);
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

						Console.Write("Fetching publisher for \"{0}\"...", info);
						var task = isbn.GetPublisherFromSearch(info);

						if (version != VERSION.INVALID)
						{
							info = isbn.HyphenateISBN(info);
						}

						DisplayLoading(task);

						var publishers = task.Result;

						if (publishers.Length > 1)
						{
							WrapLine(
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
							WrapLine("Der Publisher des Buchs \"{0}\" ist \"{1}\".", info, publishers[0]);
						}
						else if (version == VERSION.ISBN13)
						{
							Console.Write("Der Publisher des Buchs \"");
							PrintISBN13(info);
							WrapLine("\" ist \"{0}\".", publishers[0]);
						}
						else if (version == VERSION.ISBN10)
						{
							Console.Write("Der Publisher des Buchs \"");
							PrintISBN10(info);
							WrapLine("\" ist \"{0}\".", publishers[0]);
						}

						Console.WriteLine();

						WorkWithISBN(info);

						break;
					}

					case "land":
					{
						Console.Write("Bitte geben Sie die Gruppenkennung ein: ");
						var info = Console.ReadLine();
						Console.WriteLine();

						var task = isbn.GetCountryFromNumber(info);

						Console.Write("Fetching country for \"{0}\"...", info);
						DisplayLoading(task);

						var country = task.Result;

						if (string.IsNullOrEmpty(country))
						{
							WrapLine("Konnte kein Land für {0} finden!", info);
							Console.WriteLine();

							break;
						}

						WrapLine("Das Land mit der Nummer {0} ist {1}", info, country);
						Console.WriteLine();

						break;
					}

					case "verlag":
					{
						Console.Write("Bitte geben Sie die Verlagskennung ein: ");
						var info = Console.ReadLine();
						Console.WriteLine();

						var task = isbn.GetPublisherFromNumber(info);

						Console.Write("Fetching publisher for \"{0}\"...", info);
						DisplayLoading(task);

						var publishers = task.Result;

						if (publishers.Length == 0)
						{
							WrapLine("Konnte keinen Verlag für {0} finden!", info);
							Console.WriteLine();

							break;
						}

						if (publishers.Length > 1)
						{
							WrapLine("Es wurden mehrere Verläge gefunden! Versuche, präziser zu sein.");
							Console.WriteLine();
						}

						WrapLine(string.Format(
							"Der Verlag mit der Nummer \"{0}\" heißt \"{1}\", wurde bei der Agentur von \"{2}\" registriert und arbeitet in \"{3}\".",
							info, publishers.First().RegistrantName.Replace("\"", ""), publishers.First().AgencyName,
							publishers.First().Country));
						Console.WriteLine();

						break;
					}

					case "erstelleisbn":
					{
						Console.Write("Bitte geben Sie die Länderkennung ein: ");
						var country = Console.ReadLine();
						Console.WriteLine();
						Console.Write("Bitte geben Sie die Verlagskennung ein: ");
						var publisher = Console.ReadLine();
						Console.WriteLine();
						Console.Write("Bitte geben Sie die Titelkennung ein: ");
						var title = Console.ReadLine();
						Console.WriteLine();

						// Construct ISBN with placeholder checksum
						var constructed = country + "-" + publisher + "-" + title + "-" + 0;

						try
						{
							// "Convert" ISBN to get an actual proved-to-be-working ISBN
							constructed = isbn.ConvertISBN(constructed);
						}
						catch (FormatException e)
						{
							Console.WriteLine(e.Message);
							Console.WriteLine();
						}

						WorkWithISBN(constructed);

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

		private static void WrapLine(string line, params object[] arg)
		{
			if (arg.Length == 0)
			{
				WriteLines(GetWordWrappedForConsole(line));
			}
			else
			{
				WriteLines(GetWordWrappedForConsole(string.Format(line, arg)));
			}
		}

		private static IEnumerable<string> GetWordWrappedForConsole(string text)
		{
			var words = text.Split(' ');
			return words.Skip(1).Aggregate(words.Take(1).ToList(), (l, w) =>
			{
				if (l.Last().Length + w.Length >= (Console.WindowWidth - Console.CursorLeft))
					l.Add(w);
				else
					l[l.Count - 1] += " " + w;
				return l;
			});
		}

		private static void WriteLines(IEnumerable<string> lines)
		{
			foreach (string line in lines)
			{
				Console.WriteLine(line);
			}
		}

		private static void PrintKey(string key)
		{
			var normalColor = Console.ForegroundColor;
			Console.ForegroundColor = ConsoleColor.Red;
			Console.Write(key);
			Console.ForegroundColor = normalColor;
		}

		private static void PrintNotFound(string info)
		{
			var version = isbn.GetISBNVersion(info);

			if (version == VERSION.INVALID)
			{
				WrapLine("Keine Bücher gefunden, die mit \"{0}\" übereinstimmen!", info);
			}
			else if (version == VERSION.ISBN13)
			{
				Console.Write("Keine Bücher gefunden, die mit \"");
				PrintISBN13(info);
				WrapLine("\" übereinstimmen!");
			}
			else if (version == VERSION.ISBN10)
			{
				Console.Write("Keine Bücher gefunden, die mit \"");
				PrintISBN10(info);
				WrapLine("\" übereinstimmen!");
			}
		}

		private static void WorkWithISBN(string input)
		{
			isbn.OnMalformedISBN += IsbnOnOnMalformedIsbn;
			if (!string.IsNullOrWhiteSpace(input))
			{
				try
				{
					var version = isbn.GetISBNVersion(input);

					var sw = Stopwatch.StartNew();
					string converted = "";

					if (version == VERSION.INVALID)
					{
						Console.Write(
							"ISBN Version wurde nicht erkannt! Wollen Sie nach der ISBN für einen Buchtitel suchen? [J/n]");
						var i = Console.ReadLine();

						if (string.IsNullOrEmpty(i) || i.ToLower() == "j")
						{
							Console.Write("Fetching ISBN for \"{0}\"...", input);
							var task = isbn.GetISBNFromSearch(input);

							DisplayLoading(task);

							var isbns = task.Result;

							if (isbns.Length > 1)
							{
								WrapLine(
									"Mehrere zutreffende Bücher gefunden! Versuche, präzisere Suchen zu machen.");
								Console.WriteLine();
							}
							else if (isbns.Length == 0)
							{
								WrapLine("Keine Bücher gefunden, die mit \"{0}\" übereinstimmen!", input);
								Console.WriteLine();

								return;
							}

							input = isbns[0];

							version = isbn.GetISBNVersion(input);
							converted = isbn.ConvertISBN(input).ToUpper();
						}
					}
					else
					{
						converted = isbn.ConvertISBN(input).ToUpper();
					}

					sw.Stop();
					var elapsedSeconds =
						(sw.ElapsedTicks / (TimeSpan.TicksPerMillisecond / 1000f) / 1000f / 1000f)
						.ToString("F99").TrimEnd('0');

					if (converted != "")
					{
						input = isbn.HyphenateISBN(input);

						if (version == VERSION.ISBN10)
						{
							history.Insert(0,
								new Tuple<string, string, string>(input, converted, elapsedSeconds));
							Console.Write("Die ISBN10 ");
							PrintISBN10(input);
							Console.Write(" ist in ISBN13 ");
							PrintISBN13(converted);
							Console.WriteLine(" und wurde in {0} Sek. berechnet.", elapsedSeconds);
						}
						else
						{
							history.Insert(0,
								new Tuple<string, string, string>(converted, input, elapsedSeconds));
							Console.Write("Die ISBN13 ");
							PrintISBN13(input);
							Console.Write(" ist in ISBN10 ");
							PrintISBN10(converted);
							Console.WriteLine(" und wurde in {0} Sek. berechnet.", elapsedSeconds);
						}
					}

					Console.WriteLine();
				}
				catch (FormatException e)
				{
					Console.WriteLine(e.Message);
					Console.WriteLine();
				}
			}

			isbn.OnMalformedISBN -= IsbnOnOnMalformedIsbn;
		}

		private static void IsbnOnOnMalformedIsbn(string s, string message)
		{
			Console.WriteLine(message);
			Console.WriteLine();
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