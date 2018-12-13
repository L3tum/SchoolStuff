#region usings

using System;

#endregion

namespace Quartett.Simple
{
	internal class Program
	{
		private static void Main(string[] args)
		{
			Console.WriteLine("Press CTRL+C to stahp");

			Console.Write("Wollen Sie ihre Karten updaten (Kann lange dauern)? [y/N] ");
			var yn = Console.ReadLine();

			if (yn == "y")
			{
				DataLoader.Load();
			}

			GameMaster master = new GameMaster();
			var keep = false;

			while (true)
			{
				try
				{
					if (!keep)
					{
						master.Configure();
						keep = true;
					}

					master.Run();
				}
				catch (Exception e)
				{
					Console.WriteLine(e.Message);
				}

				Console.Write("Nochmal? [Y/n] ");

				yn = Console.ReadLine();

				if (yn.ToLower() == "n")
				{
					break;
				}

				Console.Write("Wollen Sie die Konfiguration beibehalten? [Y/n] ");
				yn = Console.ReadLine();

				if (yn.ToLower() == "n")
				{
					keep = false;
				}
			}
		}
	}
}