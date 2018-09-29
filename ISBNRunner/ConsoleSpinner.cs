#region usings

using System;

#endregion

namespace ISBNRunner
{
	internal class ConsoleSpinner
	{
		private int counter;

		internal ConsoleSpinner()
		{
			counter = 0;
		}

		internal void Turn()
		{
			counter++;
			switch (counter % 4)
			{
				case 0:
					Console.Write("/");
					break;
				case 1:
					Console.Write("-");
					break;
				case 2:
					Console.Write("\\");
					break;
				case 3:
					Console.Write("|");
					break;
			}

			Console.SetCursorPosition(Console.CursorLeft - 1, Console.CursorTop);
		}
	}
}