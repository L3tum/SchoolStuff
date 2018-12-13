#region usings

using System.Linq;

#endregion

namespace Quartett
{
	public class Stapel
	{
		private Karte[] karten;

		public Stapel()
		{
			karten = new Karte[0];
		}

		public Karte GetObersteKarte()
		{
			var oberste = karten.First();

			karten = karten.Where((k, i) => i != 0).ToArray();

			return oberste;
		}

		public Karte[] GetKarten(int menge)
		{
			return karten.Take(menge).ToArray();
		}

		public void AddKarten(Karte[] kart)
		{
			karten = karten.Concat(kart).ToArray();
		}

		public void AddKarte(Karte karte)
		{
			AddKarten(new[] {karte});
		}

		public bool HasKartenLeft()
		{
			return karten.Length > 0;
		}

		public void LöscheKarte(int index)
		{
			karten = karten.Where((k, i) => i != index).ToArray();
		}

		public string[] GetNumericalProperties()
		{
			return karten[0].GetProperties()
				.Where(p => double.TryParse(karten[0].GetProperty(p).ToString(), out var result)).ToArray();
		}
	}
}