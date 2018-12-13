#region usings

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using TableParser;

#endregion

namespace Quartett.Simple
{
	public class GameMaster
	{
		private readonly Dictionary<int, Karte> gespielteKarten = new Dictionary<int, Karte>();
		private string aktuelleEigenschaft;
		private int aktuellerSpieler;
		private int anzahlKIs;
		private int anzahlMenschen;
		private string file = "";
		private Spieler[] spieler = new Spieler[0];

		internal Kartenset kartenset { get; private set; }

		public void Configure()
		{
			ChooseKartenset();
			SetNumberOfPlayers();
		}

		public void Run()
		{
			Initialisiere();

			aktuellerSpieler = 0;

			// Solange noch mehr als ein Spieler Karten hat
			while (spieler.Length > 1)
			{
				// Clear last round
				Console.Clear();
				gespielteKarten.Clear();

				var info = new List<Tuple<string, int>>();

				foreach (Spieler player in spieler)
				{
					info.Add(new Tuple<string, int>("Spieler" + player.GetID(),
						player.Stapel.GetKarten(32).Length));
				}

				Console.WriteLine(info.ToStringTable(new[] {"Spieler", "Karten"}, tuple => tuple.Item1,
					tuple => tuple.Item2));
				Console.ReadKey(true);
				Console.Clear();

				// Get aktuelle Eigenschaft auf der Karte
				aktuelleEigenschaft = spieler[aktuellerSpieler].BenenneEigenschaft();

				// Spiele Karten der Reihe um
				for (int i = aktuellerSpieler; i <= spieler.Length; i++)
				{
					if (i == spieler.Length)
					{
						if (gespielteKarten.Keys.Count < spieler.Length)
						{
							i = 0;
						}
						else
						{
							break;
						}
					}

					gespielteKarten.Add(spieler[i].GetID(), spieler[i].SpieleKarte(aktuelleEigenschaft));
				}

				var info2 = new List<Tuple<string, string, string>>();

				foreach (KeyValuePair<int, Karte> pair in gespielteKarten)
				{
					info2.Add(new Tuple<string, string, string>("Spieler" + pair.Key,
						pair.Value.GetProperty("identifier").ToString(),
						pair.Value.GetProperty(aktuelleEigenschaft).ToString()));
				}

				Console.WriteLine(info2.ToStringTable(new[] {"Spieler", "Karte", aktuelleEigenschaft},
					tuple => tuple.Item1,
					tuple => tuple.Item2,
					tuple => tuple.Item3));

				// Bekomme die beste Karte
				var id = GetBesteKarte();

				if (id != -1)
				{
					Console.WriteLine("Spieler {0} hat die höchste Karte!", id);
				}
				else
				{
					Console.WriteLine("Unentschieden! Jeder Spieler bekommt seine Karten zurück!");
				}

				Console.ReadKey(true);

				if (id != -1)
				{
					// Füge die Karten dem Gewinner ins Deck
					var mvp = spieler.First(s => s.GetID() == id);

					foreach (Karte karte in gespielteKarten.Values)
					{
						mvp.Stapel.AddKarte(karte);
					}
				}
				else
				{
					foreach (KeyValuePair<int, Karte> pair in gespielteKarten)
					{
						spieler.First(s => s.GetID() == pair.Key).Stapel.AddKarte(pair.Value);
					}
				}

				Console.Clear();

				// Gehe durch alle Spieler durch und...
				foreach (var t in spieler)
				{
					// Gucke ob jemand keine Karten mehr hat
					if (!t.Stapel.HasKartenLeft())
					{
						Console.WriteLine("Spieler {0} hat keine Karten mehr!", t.GetID());

						if (!t.Lost(spieler.Length))
						{
							return;
						}
					}
				}

				// Lösche alle Loser
				spieler = spieler.Where(s => s.Stapel.HasKartenLeft()).ToArray();
			}

			Console.Clear();

			// Nur einer is übrig
			Console.WriteLine("Spieler {0} hat gewonnen! Glückwunsch!", spieler[0].GetID());
			spieler[0].Won();
		}

		// Initialisierung
		private void Initialisiere()
		{
			spieler = new Spieler[anzahlMenschen + anzahlKIs];

			for (int i = 0; i < anzahlMenschen; i++)
			{
				spieler[i] = new Mensch(i + 1);
			}

			for (int i = 0; i < anzahlKIs; i++)
			{
				spieler[anzahlMenschen + i] = new KI(anzahlMenschen + i + 1);
			}

			LadeKartenset();
			VerteileKarten();
		}

		// Initialisierung
		private void LadeKartenset()
		{
			Console.WriteLine("Lade {0}...", file.Split('\\').Last().Replace(".json", ""));
			kartenset = Kartenset.LoadFrom(file);
			Console.WriteLine("{0} geladen!", file.Split('\\').Last().Replace(".json", ""));
		}

		// Initialisierung
		private void VerteileKarten()
		{
			var perPlayer = 32 / spieler.Length;
			var random = new Random(DateTime.Now.GetHashCode());

			foreach (Spieler player in spieler)
			{
				for (int i = 0; i < perPlayer; i++)
				{
					var index = random.Next(0, kartenset.cards.Length);

					player.Stapel.AddKarte(kartenset.cards[index]);
					kartenset.RemoveCard(index);
				}
			}
		}

		// Konfiguration
		private void ChooseKartenset()
		{
			var files = Directory.GetFiles(AppDomain.CurrentDomain.BaseDirectory + "/data", "*.json",
				SearchOption.TopDirectoryOnly);

			Array.Sort(files, (s, s1) =>
			{
				for (int i = 0; i < s.Length; i++)
				{
					if (i == s1.Length)
					{
						return -1;
					}

					if (s[i] > s1[i])
					{
						return -1;
					}

					if (s1[i] > s[i])
					{
						return 1;
					}
				}

				if (s1.Length > s.Length)
				{
					return 1;
				}

				return 0;
			});

			file = files[
				Utility.ConsoleSelect("Kartenset",
					files.Select(f => f.Split('\\').Last().Replace(".json", "")).ToArray())];
		}

		// Konfiguration
		private void SetNumberOfPlayers()
		{
			var number = 5;

			while (number > 4 || number < 0)
			{
				Console.Write("Wie viele menschliche Spieler wollen spielen: ");
				number = int.Parse(Console.ReadLine());

				if (number > 4)
				{
					Console.WriteLine("Es können maximal 4 Spieler teilnehmen!");
					Console.WriteLine();
				}
			}

			var kiNumber = 5;

			while (kiNumber > (4 - number) || kiNumber < 0)
			{
				Console.Write("Wie viele KI Spieler sollen spielen: ");
				kiNumber = int.Parse(Console.ReadLine());

				if (anzahlMenschen + kiNumber > 4)
				{
					Console.WriteLine("Es können maximal 4 Spieler teilnehmen!");
					Console.WriteLine();
				}
			}

			anzahlKIs = kiNumber;
			anzahlMenschen = number;
		}

		private int GetBesteKarte()
		{
			var index = kartenset.GetBestCard(gespielteKarten.Values.ToArray(), aktuelleEigenschaft);

			if (index == -1)
			{
				return index;
			}

			return gespielteKarten.ElementAt(index).Key;
		}
	}
}