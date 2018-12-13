#region usings

using System;
using System.Linq;

#endregion

namespace Quartett.GA
{
	internal class Chromosome
	{
		internal Chromosome(double weight)
		{
			this.weight = weight;
		}

		internal double weight { get; }

		internal int Calculate(double[] cardsAvailable, double[] currentlyPlayed, bool reversed, string eigenschaft)
		{
			var res = 0.0d;

			if (reversed)
			{
				foreach (double d in currentlyPlayed)
				{
					res += d * weight;
				}

				foreach (double d in cardsAvailable)
				{
					res -= d * weight;
				}
			}
			else
			{
				foreach (double d in cardsAvailable)
				{
					res += d * weight;
				}

				foreach (double d in currentlyPlayed)
				{
					res -= d * weight;
				}
			}

			res *= (eigenschaft.GetHashCode() * weight);

			double closest = cardsAvailable.ToList().Aggregate((x, y) => Math.Abs(x - res) < Math.Abs(y - res) ? x : y);

			return cardsAvailable.ToList().IndexOf(closest);
		}

		internal Chromosome Breed(Chromosome partner)
		{
			var rand = new Random();

			return new Chromosome((weight * rand.NextDouble()) * (partner.weight * rand.NextDouble()));
		}
	}
}