#region usings

using System;

#endregion

namespace Quartett
{
	internal class Program
	{
		private static void Main(string[] args)
		{
			Console.WriteLine("Press CTRL+C to stahp");

			GameMaster master = GameMaster.GetInstance();
			var keep = false;

			while (true)
			{
				//try
				//{
					if (!keep)
					{
						master.Configure();
						keep = true;
					}
					master.Run();
				//}
				//catch (Exception e)
				//{
				//	Console.WriteLine(e.Message);
				//}

				if (!master.monitored)
				{
					Console.Write("Nochmal? [Y/n] ");
					var yn = Console.ReadLine();

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
}