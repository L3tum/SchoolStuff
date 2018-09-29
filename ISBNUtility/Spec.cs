using System.Text.RegularExpressions;

namespace ISBNUtility
{
	internal class Spec
	{
		// A conversion between prefix "979" and ISBN10 is not possible!
		internal const string PREFIX = "978";
		private static readonly Regex ISBN10_FORMAT = new Regex("([0-9]{10}|[0-9]{9}x)");
		private static readonly Regex ISBN13_FORMAT = new Regex("[0-9]{13}");

		/// <summary>
		/// Determines whether [is ISBN10 spec compliant] [the specified isbn].
		/// </summary>
		/// <param name="isbn">The isbn.</param>
		/// <returns>
		///   <c>true</c> if [is ISBN10 spec compliant] [the specified isbn]; otherwise, <c>false</c>.
		/// </returns>
		internal static bool IsISBN10SpecCompliant(string isbn)
		{
			return ISBN10_FORMAT.IsMatch(isbn);
		}

		/// <summary>
		/// Determines whether [is ISBN13 spec compliant] [the specified isbn].
		/// </summary>
		/// <param name="isbn">The isbn.</param>
		/// <returns>
		///   <c>true</c> if [is ISBN13 spec compliant] [the specified isbn]; otherwise, <c>false</c>.
		/// </returns>
		internal static bool IsISBN13SpecCompliant(string isbn)
		{
			return ISBN13_FORMAT.IsMatch(isbn);
		}

		/// <summary>
		/// Calculates the checksum for ISBN13.
		/// </summary>
		/// <param name="isbn">The isbn.</param>
		/// <returns></returns>
		internal static char CalculateChecksum13(string isbn)
		{
			// Add prefix if it doesn't exist
			// Smaller than 12 so checksum can be missing too
			if (isbn.Length < 12)
			{
				isbn = PREFIX + isbn;
			}

			// Remove current checksum if it exists
			if (isbn.Length == 13)
			{
				isbn = isbn.Substring(0, isbn.Length - 1);
			}

			if (isbn.Length != 12)
			{
				return '-';
			}

			return ("" + CalcChecksum13(isbn))[0];
		}

		/// <summary>
		/// Calculates the checksum for ISBN10.
		/// </summary>
		/// <param name="isbn">The isbn.</param>
		/// <returns></returns>
		internal static char CalculateChecksum10(string isbn)
		{
			if (isbn.Length == 10)
			{
				isbn = isbn.Substring(0, isbn.Length - 1);
			}

			if (isbn.Length != 9)
			{
				return '-';
			}

			var checksum = CalcChecksum10(isbn);

			if (checksum == 10)
			{
				return 'x';
			}

			return ("" + checksum)[0];
		}

		private static int CalcChecksum10(string isbn)
		{
			var checksum = 0;

			for (var i = 0; i < isbn.Length; i++)
			{
				checksum += (i + 1) * (int) char.GetNumericValue(isbn[i]);
			}

			checksum %= 11;

			return checksum;
		}

		private static int CalcChecksum13(string isbn)
		{
			var checksum = 0;

			for (var i = 0; i < isbn.Length; i++)
			{
				// Add value to checksum
				// Every uneven number is multiplied with 3
				checksum += (int) char.GetNumericValue(isbn[i]) * (i % 2 != 0 ? 3 : 1);
			}

			checksum %= 10;
			checksum = 10 - checksum;
			checksum %= 10;

			return checksum;
		}
	}
}