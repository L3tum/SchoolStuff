#region usings

using System;
using System.Linq;
using Quartett.GA;

#endregion

namespace Quartett
{
	public class SchwereKI : Spieler
	{
		private readonly DNA dna;

		public SchwereKI(int ID) : base(ID)
		{
			dna = Genome.GetDNA();
		}

		public override string BenenneEigenschaft()
		{
			var random = new Random();
			var props = Stapel.GetNumericalProperties();

			return props[random.Next(0, props.Length)];
		}

		public override Karte SpieleKarte(string eigenschaft)
		{
			var reversed = GameMaster.GetInstance().kartenset.reversals.Contains(eigenschaft);
			var cards = Stapel.GetKarten(32);
			var properties = cards.Where(c => double.TryParse(c.GetProperty(eigenschaft).ToString(), out var res))
				.Select(c => double.Parse(c.GetProperty(eigenschaft).ToString())).ToArray();
			var played = GameMaster.GetInstance().GetGespielteKarten()
				.Select(c => double.Parse(c.GetProperty(eigenschaft).ToString())).ToArray();
			var index = dna.Calculate(properties, played, reversed, eigenschaft);

			Stapel.LöscheKarte(index);

			return cards.ElementAt(index);
		}

		public override bool Lost(int placement)
		{
			Genome.returnDNA(dna, placement);

			return true;
		}

		public override void Won()
		{
			Genome.returnDNA(dna, 1);
		}
	}
}