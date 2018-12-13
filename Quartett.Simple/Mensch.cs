#region usings

using System;

#endregion

namespace Quartett.Simple
{
	public class Mensch : Spieler
	{
		public Mensch(int ID) : base(ID)
		{
		}

		public override string BenenneEigenschaft()
		{
			var props = Stapel.GetNumericalProperties();

			return props[Utility.ConsoleSelect($"Eigenschaft (Spieler{GetID()})", props)];
		}

		public override Karte SpieleKarte(string eigenschaft)
		{
			return Stapel.GetObersteKarte();
		}

		public override bool Lost(int placement)
		{
			Console.WriteLine("Du bist leider ausgeschieden!");

			Console.Write("Willst du das Spiel abbrechen? [y/N]");

			var yn = Console.ReadLine();

			if (yn.ToLower() == "y")
			{
				return true;
			}

			return false;
		}

		public override void Won()
		{
			Console.WriteLine("Hurray");
		}
	}
}