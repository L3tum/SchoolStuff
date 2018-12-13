#region usings

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

#endregion

namespace Quartett.GA
{
	internal class DNA
	{
		private readonly Dictionary<Chromosome, int> chromosomes;

		internal DNA()
		{
			chromosomes = new Dictionary<Chromosome, int>(100);

			var rand = new Random();

			for (int i = 0; i < 100; i++)
			{
				chromosomes.Add(new Chromosome(rand.NextDouble()), 0);
			}
		}

		internal int Generation { get; private set; } = 1;

		internal int Calculate(double[] cardsAvailable, double[] currentlyPlayed, bool reversed, string eigenschaft)
		{
			var i = 0;
			var results = new int[100];
			var ts = new List<Task>();

			while (true)
			{
				if (ts.Count == Environment.ProcessorCount)
				{
					Task.WaitAny(ts.ToArray());
				}

				var pair = chromosomes.ElementAt(i);

				int copy = i;

				ts.Add(Task.Run(() => pair.Key.Calculate(cardsAvailable, currentlyPlayed, reversed, eigenschaft))
					.ContinueWith(t => { results[copy] = t.Result; }));

				i++;

				if (i == 100)
				{
					Task.WaitAll(ts.ToArray());

					break;
				}
			}

			var sorted = new Dictionary<int, int>();

			foreach (int result in results)
			{
				if (sorted.ContainsKey(result))
				{
					sorted[result]++;
				}
				else
				{
					sorted.Add(result, 1);
				}
			}

			sorted = sorted.OrderBy(kvp => kvp.Value).ToDictionary(kvp => kvp.Key, kvp => kvp.Value);

			var best = sorted.First().Key;

			for (int j = 0; j < results.Length; j++)
			{
				if (results[j] == best)
				{
					chromosomes[chromosomes.ElementAt(j).Key]++;
				}
			}

			return best;
		}

		internal void Evolve(int place)
		{
			Generation++;

			var nextGeneration = new List<Chromosome>();
			var keepBest = 11 - place;
			var ordered = chromosomes.OrderBy(kvp => kvp.Value).ToDictionary(kvp => kvp.Key, kvp => kvp.Value);
			var kept = ordered.Keys.Take(keepBest).ToArray();
			var rand = new Random();

			for (int i = 0; i < keepBest * 3; i++)
			{
				var index1 = rand.Next(0, kept.Length);
				var index2 = rand.Next(0, kept.Length);

				nextGeneration.Add(kept[index1].Breed(kept[index2]));
			}

			chromosomes.Clear();

			for (int i = nextGeneration.Count; i < 100; i++)
			{
				nextGeneration.Add(new Chromosome(rand.NextDouble()));
			}

			foreach (Chromosome chromosome in nextGeneration)
			{
				chromosomes.Add(chromosome, 0);
			}

			Console.WriteLine("Evolved into {0} Generation on place {1}!", Generation, place);
			Console.ReadKey(true);
		}
	}
}