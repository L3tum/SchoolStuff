#region usings

using System;
using System.Data;
using System.IO;
using System.Linq;
using Newtonsoft.Json;

#endregion

namespace Quartett
{
	public class Kartenset
	{
		public Karte[] cards;
		public string[] reversals;

		public void Check()
		{
			if (cards.Length < 32)
			{
				throw new ConstraintException(
					$"Ein Kartenset muss mindestens 32 Karten beinhalten. Dieses beinhaltet jediglich {cards.Length}.");
			}

			var props = cards[0].GetProperties();
			var removals = new bool[cards.Length];

			for (int i = 0; i < cards.Length; i++)
			{
				if (!cards[i].HasProperties(props))
				{
					removals[i] = true;
					Console.WriteLine("Die Karte an der Stelle {0} ist invalid!", i + 1);
				}
			}

			cards = cards.Where((c, i) => !removals[i]).ToArray();
		}

		public void RemoveCard(int index)
		{
			cards = cards.Where((c, i) => i != index).ToArray();
		}

		public int GetBestCard(Karte[] karten, string eigenschaft)
		{
			var best = -1;
			var bestValue = 0.0d;
			bool reversed = reversals.Contains(eigenschaft);

			if (reversed)
			{
				bestValue = 9999d;
			}

			for (int i = 0; i < karten.Length; i++)
			{
				var val = double.Parse(karten[i].GetProperty(eigenschaft).ToString());

				if (Math.Abs(val - bestValue) < 0.01d)
				{
					best = -1;
				}

				if (reversed)
				{
					if (val < bestValue)
					{
						best = i;
						bestValue = val;
					}
				}
				else
				{
					if (val > bestValue)
					{
						best = i;
						bestValue = val;
					}
				}
			}

			return best;
		}

		public static Kartenset LoadFrom(string file)
		{
			var json = "";

			using (var stream = File.OpenRead(file))
			{
				using (var reader = new StreamReader(stream))
				{
					json = reader.ReadToEnd();
				}
			}

			Kartenset kartenset = JsonConvert.DeserializeObject<Kartenset>(json);

			kartenset.Check();

			return kartenset;
		}
	}
}