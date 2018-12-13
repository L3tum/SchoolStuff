#region usings

using System;

#endregion

namespace Quartett
{
	public class KI : Spieler
	{
		public KI(int ID) : base(ID)
		{
		}

		public override string BenenneEigenschaft()
		{
			var random = new Random();
			var props = Stapel.GetNumericalProperties();

			return props[random.Next(0, props.Length)];
		}

		public override Karte SpieleKarte(string eigenschaft)
		{
			return Stapel.GetObersteKarte();
		}

		public override bool Lost(int placement)
		{
			Console.WriteLine("This is unacceptable.");
			return true;
		}

		public override void Won()
		{
			Console.WriteLine("Muhahaha");
		}
	}
}