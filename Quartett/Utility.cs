#region usings

using System;

#endregion

namespace Quartett
{
	internal static class Utility
	{
		internal static int ConsoleSelect(string selector, string[] options)
		{
			var selecting = true;
			var selected = 0;

			Console.CursorVisible = false;

			while (selecting)
			{
				Console.Clear();
				Console.WriteLine(new string('*', Console.WindowWidth));
				var title = " Quartett ";
				Console.WriteLine(new string('*', (Console.WindowWidth - title.Length) / 2) + title +
				                  new string('*', (Console.WindowWidth - title.Length) / 2));
				Console.WriteLine(new string('*', Console.WindowWidth));
				Console.WriteLine();
				Console.WriteLine(
					"Bitte wähle ein/eine {0} aus (Up/Down, Bestätigen mit Enter):", selector);
				Console.WriteLine();

				for (int i = 0; i < options.Length; i++)
				{
					if (i == selected)
					{
						Console.Write("> ");
					}

					Console.WriteLine(options[i]);
				}

				var key = Console.ReadKey(true);

				if (key.Key == ConsoleKey.UpArrow)
				{
					selected--;

					if (selected < 0)
					{
						selected = options.Length - 1;
					}
				}
				else if (key.Key == ConsoleKey.DownArrow)
				{
					selected++;

					if (selected >= options.Length)
					{
						selected = 0;
					}
				}
				else if (key.Key == ConsoleKey.Enter)
				{
					selecting = false;
				}
			}

			Console.Clear();
			Console.CursorVisible = true;

			return selected;
		}
	}
}