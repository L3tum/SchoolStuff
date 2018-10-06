#region usings

using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Threading.Tasks;
using System.Xml.Linq;
using ISBNUtility.Model;
using Newtonsoft.Json;

#endregion

namespace ISBNUtility
{
	internal class Recognizer
	{
		private static List<Group> ranges;

		internal static string SanitizeISBN(string isbn)
		{
			return isbn.Replace("-", "").Replace(" ", "").Replace(".", "").Replace("X", "x");
		}

		internal static VERSION RecognizeISBN(string isbn)
		{
			isbn = SanitizeISBN(isbn);
			if (isbn.Length == 13 && isbn.StartsWith(Spec.PREFIX) && Spec.IsISBN13SpecCompliant(isbn))
			{
				return VERSION.ISBN13;
			}

			if (isbn.Length == 10 && Spec.IsISBN10SpecCompliant(isbn))
			{
				return VERSION.ISBN10;
			}

			return VERSION.INVALID;
		}

		/// <summary>
		/// Hyphenates the isbn.
		/// It uses the ISBN Ranges list provided here https://www.isbn-international.org/range_file_generation
		/// and converted to JSON
		/// Inspiration is here: https://github.com/xlcnd/isbnlib/blob/dev/isbnlib/_msk.py
		/// Permalink: https://github.com/xlcnd/isbnlib/blob/114e4b2c0726a30d3ae50018d27409c004a68800/isbnlib/_msk.py
		/// </summary>
		/// <param name="isbn">The isbn.</param>
		/// <returns></returns>
		internal static string hyphenateISBN(string isbn)
		{
			isbn = SanitizeISBN(isbn);
			var version = RecognizeISBN(isbn);

			if (version == VERSION.INVALID)
			{
				return isbn;
			}

			var loader = LoadISBNRanges();
			var origISBN = isbn;

			// We need a ISBN13 for this
			// Reason is that we need both prefix and checksum so it's easier to just generate it normally
			if (version == VERSION.ISBN10)
			{
				isbn = Converter.ConvertISBN10ToISBN13(isbn);
			}

			// Prepare ISBN for usage with group finder
			// Since at least 2 digits need to be used for checksum and title
			// We need to remove them to properly find the actual range in the group this ISBN is in
			var preparedISBN = isbn.Substring(0, isbn.Length - 2);

			// ISBN13 Group (Country) string, starting with smallest possible
			var group = preparedISBN.Substring(0, 3) + "-" + preparedISBN.Substring(3, 1);
			var current = 3;
			var length = 0;

			loader.Wait();

			for (var i = 0; i < 6; i++)
			{
				var found = false;

				foreach (Group range in ranges)
				{
					// Check if the current group's (country) Prefix is in the ISBN
					if (range.Prefix == group)
					{
						// The original code this is derived from always takes current + 8 as length
						// Since the substring starts at index 4 at minimum a length of minimum 12 would end up way over string
						// So we just cut the whole string starting at current + 1
						var sevens = int.Parse(preparedISBN.Substring(current + 1).PadRight(7, '0'));

						// Check if the part without the group is in the rules of the group
						foreach (var rule in (Newtonsoft.Json.Linq.JArray) range.Rules.Rule)
						{
							var r = rule.ToObject<Rule>();
							var ra = r.Range.Split('-');

							if (int.Parse(ra[0]) <= sevens && sevens <= int.Parse(ra[1]))
							{
								length = int.Parse(r.Length);
								found = true;
								break;
							}
						}

						if (found)
						{
							break;
						}
					}
				}

				// Unfortunately we can't use LINQ here all that good so...
				if (found)
				{
					break;
				}

				// Else take more from the ISBN to identify the group
				current++;
				group += preparedISBN.Substring(current, 1);
			}

			if (length != 0)
			{
				// If it's ISBN10 we need to avoid the prefix and choose the original checksum
				if (version == VERSION.ISBN10)
				{
					isbn = string.Join("-", group.Substring(4), isbn.Substring(current + 1, length),
						isbn.Substring(current + 1 + length, isbn.Length - current - length - 2),
						origISBN[origISBN.Length - 1].ToString());
				}
				else
				{
					isbn = string.Join("-", group, isbn.Substring(current + 1, length),
						isbn.Substring(current + 1 + length, isbn.Length - current - length - 2),
						isbn[isbn.Length - 1]);
				}

				return isbn;
			}

			return origISBN;
		}

		/// <summary>
		/// Gets the country from number.
		/// </summary>
		/// <param name="number">The number.</param>
		/// <returns></returns>
		internal static string GetCountryFromNumber(string number)
		{
			var loader = LoadISBNRanges();
			var country = "";

			loader.Wait();

			// Since we expect the "fully qualifying" identifier for this country/group
			// we can just iterate over all the groups we got
			// and don't have to check for any "is the publisher valid for this country" or so
			foreach (Group range in ranges)
			{
				// Check if the current group's (country) Prefix is the number
				if (range.Prefix == number)
				{
					country = range.Agency;
					break;
				}
			}

			return country;
		}

		/// <summary>
		/// Gets the country this ISBN was registered in
		/// </summary>
		/// <param name="isbn"></param>
		/// <returns></returns>
		internal static string GetCountry(string isbn)
		{
			isbn = SanitizeISBN(isbn);
			var version = RecognizeISBN(isbn);

			if (version == VERSION.INVALID)
			{
				return isbn;
			}

			var loader = LoadISBNRanges();

			// We need a ISBN13 for this
			// Reason is that we need both prefix and checksum so it's easier to just generate it normally
			if (version == VERSION.ISBN10)
			{
				isbn = Converter.ConvertISBN10ToISBN13(isbn);
			}

			// Prepare ISBN for usage with group finder
			// Since at least 2 digits need to be used for checksum and title
			// We need to remove them to properly find the actual range in the group this ISBN is in
			var preparedISBN = isbn.Substring(0, isbn.Length - 2);

			// ISBN13 Group (Country) string, starting with smallest possible
			var group = preparedISBN.Substring(0, 3) + "-" + preparedISBN.Substring(3, 1);
			var current = 3;
			var country = "";

			loader.Wait();

			for (var i = 0; i < 6; i++)
			{
				var found = false;

				foreach (Group range in ranges)
				{
					// Check if the current group's (country) Prefix is in the ISBN
					if (range.Prefix == group)
					{
						// The original code this is derived from always takes current + 8 as length
						// Since the substring starts at index 4 at minimum a length of minimum 12 would end up way over string
						// So we just cut the whole string starting at current + 1
						var sevens = int.Parse(preparedISBN.Substring(current + 1).PadRight(7, '0'));

						// Check if the part without the group is in the rules of the group
						foreach (var rule in (Newtonsoft.Json.Linq.JArray) range.Rules.Rule)
						{
							var r = rule.ToObject<Rule>();
							var ra = r.Range.Split('-');

							if (int.Parse(ra[0]) <= sevens && sevens <= int.Parse(ra[1]))
							{
								country = range.Agency;
								found = true;
								break;
							}
						}

						if (found)
						{
							break;
						}
					}
				}

				// Unfortunately we can't use LINQ here all that good so...
				if (found)
				{
					break;
				}

				// Else take more from the ISBN to identify the group
				current++;
				group += preparedISBN.Substring(current, 1);
			}

			return country;
		}

		private static async Task<bool> LoadISBNRanges()
		{
			// Get the ISBN Ranges
			if (ranges == null)
			{
				using (HttpClient client = new HttpClient())
				{
					var response =
						await client.GetAsync("https://www.isbn-international.org/export_rangemessage.xml");

					if (response.IsSuccessStatusCode)
					{
						var content = await response.Content.ReadAsStreamAsync();
						XDocument doc = XDocument.Load(content);
						ranges = JsonConvert.DeserializeObject<ISBNRanges>(JsonConvert.SerializeXNode(doc.Root))
							.ISBNRangeMessage.RegistrationGroups.Group;

						return true;
					}
				}

				using (var stream = File.OpenRead(AppDomain.CurrentDomain.BaseDirectory + "/ISBNRanges.json"))
				{
					using (StreamReader sr = new StreamReader(stream))
					{
						ranges = JsonConvert.DeserializeObject<ISBNRanges>(sr.ReadToEnd()).ISBNRangeMessage
							.RegistrationGroups.Group;
					}
				}
			}

			return true;
		}
	}
}