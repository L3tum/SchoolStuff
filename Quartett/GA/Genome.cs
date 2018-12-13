using System.Collections.Generic;
using System.Linq;

namespace Quartett.GA
{
	internal static class Genome
	{
		private static readonly List<DNA> dnas = new List<DNA>();

		internal static DNA GetDNA()
		{
			var dna = dnas.FirstOrDefault();

			if (dna == null)
			{
				return new DNA();
			}

			return dna;
		}

		internal static void returnDNA(DNA dna, int placement)
		{
			dna.Evolve(placement);

			dnas.Add(dna);
		}
	}
}