namespace ISBNUtility
{
	internal class Converter
	{
		/// <summary>
		/// Converts the ISBN13 to ISBN10.
		/// </summary>
		/// <param name="isbn">The isbn.</param>
		/// <returns></returns>
		internal static string ConvertISBN13ToISBN10(string isbn)
		{
			// Remove the prefix
			isbn = isbn.Substring(3, isbn.Length - 3);

			// Switch the checksums
			var checksum = Spec.CalculateChecksum10(isbn);
			isbn = isbn.Substring(0, isbn.Length - 1);
			isbn += checksum;

			return isbn;
		}

		/// <summary>
		/// Converts the ISBN10 to ISBN13.
		/// </summary>
		/// <param name="isbn">The isbn.</param>
		/// <returns></returns>
		internal static string ConvertISBN10ToISBN13(string isbn)
		{
			// Add the prefix
			isbn = Spec.PREFIX + isbn;

			// Switch the checksums
			var checksum = Spec.CalculateChecksum13(isbn);
			isbn = isbn.Substring(0, isbn.Length - 1);
			isbn += checksum;

			return isbn;
		}
	}
}