#region usings

using System;

#endregion

namespace Quartett.Simple
{
	public abstract class Spieler
	{
		private readonly int ID;

		public Stapel Stapel = new Stapel();

		protected Spieler(int ID)
		{
			this.ID = ID;
		}

		public int GetID()
		{
			return ID;
		}

		public virtual string BenenneEigenschaft()
		{
			throw new NotImplementedException();
		}

		public virtual Karte SpieleKarte(string eigenschaft)
		{
			throw new NotImplementedException();
		}

		public virtual bool Lost(int placement)
		{
			throw new NotImplementedException();
		}

		public virtual void Won()
		{
			throw new NotImplementedException();
		}
	}
}