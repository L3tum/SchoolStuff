#region usings

using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using ISBNUtility.Model;
using Newtonsoft.Json;

#endregion

namespace ISBNUtility
{
	public enum VERSION
	{
		INVALID,
		ISBN10,
		ISBN13
	}

	public class ISBN
	{
		/// <summary>
		/// Gets the isbn version.
		/// </summary>
		/// <param name="isbn">The isbn.</param>
		/// <returns></returns>
		public VERSION GetISBNVersion(string isbn)
		{
			// Sanitize input
			isbn = isbn.Replace("-", "").Replace(" ", "").Replace(".", "").Replace("X", "x");

			// Try to determine the ISBN Version used
			return Recognizer.RecognizeISBN(isbn);
		}

		/// <summary>
		/// Converts the isbn.
		/// </summary>
		/// <param name="input">The input.</param>
		/// <returns></returns>
		/// <exception cref="System.FormatException">
		/// </exception>
		public string ConvertISBN(string input)
		{
			// Sanitize input
			var sanitized = Recognizer.SanitizeISBN(input);

			// Try to determine the ISBN Version used
			VERSION version = Recognizer.RecognizeISBN(sanitized);

			// Throw if unable to determine (likely malformed string)
			if (version == VERSION.INVALID)
			{
				throw new FormatException($"Invalid ISBN Format for {input} detected!");
			}

			// Throw if the ISBN contains invalid characters
			if (!(version == VERSION.ISBN10
				? Spec.IsISBN10SpecCompliant(sanitized)
				: Spec.IsISBN13SpecCompliant(sanitized)))
			{
				throw new FormatException($"{input} is not spec compliant!");
			}

			// Throw if checksum calculated is not the one in the ISBN (malformed/incorrect ISBN)
			var checksum = (version == VERSION.ISBN10
				? Spec.CalculateChecksum10(sanitized)
				: Spec.CalculateChecksum13(sanitized));

			if (checksum != sanitized[sanitized.Length - 1])
			{
				throw new FormatException(
					$"Checksum mismatch for {input}! It should be {checksum.ToString().ToUpper()}!");
			}

			var isbn = (version == VERSION.ISBN10
				? Converter.ConvertISBN10ToISBN13(sanitized)
				: Converter.ConvertISBN13ToISBN10(sanitized));

			// Check if input already contains separation (and thus can simply copied)
			if (input.Contains("-") || input.Contains(" "))
			{
				// Remove the prefix and the first separator if it exists
				if (version == VERSION.ISBN13)
				{
					input = input.Substring(Spec.PREFIX.Length + 1);
				}

				var separator = "-";

				// Check if '-' or ' ' are used for separation
				if (input.Contains(" "))
				{
					separator = " ";
				}

				// Place the separators into the new string
				while (input.Contains(separator))
				{
					var index = input.IndexOf(separator);
					isbn = isbn.Insert((version == VERSION.ISBN10 ? Spec.PREFIX.Length : 0) + index, "-");
					input = input.Substring(0, index) + "+" + input.Substring(index + 1);
				}

				// We also need to hyphenate the prefix if it exists
				// This and the check above are inverted since we need to modify the target ISBN version
				if (version == VERSION.ISBN10)
				{
					isbn = isbn.Insert(Spec.PREFIX.Length, "-");
				}
			}
			else
			{
				isbn = HyphenateISBN(isbn);
			}

			return isbn;
		}

		/// <summary>
		/// Hyphenates the isbn.
		/// </summary>
		/// <param name="isbn">The isbn.</param>
		/// <returns></returns>
		public string HyphenateISBN(string isbn)
		{
			if (!isbn.Contains("-"))
			{
				if (isbn.Contains(" "))
				{
					return isbn.Replace(" ", "-");
				}

				return Recognizer.hyphenateISBN(isbn);
			}

			return isbn;
		}

		/// <summary>
		/// Gets the country with the country identifier
		/// </summary>
		/// <param name="number"></param>
		/// <returns></returns>
		public async Task<string> GetCountryFromNumber(string number)
		{
			return await Task.Run(() =>
			{
				// We need the prefix for this search
				// as the data is with a prefix
				if (!number.StartsWith(Spec.PREFIX))
				{
					number = Spec.PREFIX + "-" + number;
				}

				return Recognizer.GetCountryFromNumber(number);
			});
		}

		/// <summary>
		/// Gets the country.
		/// </summary>
		/// <param name="isbn">The isbn.</param>
		/// <returns></returns>
		public async Task<string> GetCountry(string isbn)
		{
			return await Task.Run(() =>
			{
				isbn = Recognizer.SanitizeISBN(isbn);

				return Recognizer.GetCountry(isbn);
			});
		}

		/// <summary>
		/// Gets the publisher name from its number
		/// </summary>
		/// <param name="number"></param>
		/// <returns></returns>
		public async Task<PublisherDoc[]> GetPublisherFromNumber(string number)
		{
			// We need the prefix again
			if (!number.StartsWith(Spec.PREFIX))
			{
				number = Spec.PREFIX + "-" + number;
			}

			string request = string.Format(
				"https://grp.isbn-international.org/sites/all/modules/piid_cineca_solr/piid_cineca_solr.ajax.php?q={0}&wt=json",
				number);

			using (HttpClient client = new HttpClient())
			{
				var response = await client.GetAsync(request);

				if (response.IsSuccessStatusCode)
				{
					PublisherRegister register =
						JsonConvert.DeserializeObject<PublisherRegister>(await response.Content.ReadAsStringAsync()
							.ConfigureAwait(false));

					var publishers = new List<PublisherDoc>();

					foreach (PublisherDoc publisherDoc in register.response.docs)
					{
						publishers.Add(publisherDoc);
					}

					return publishers.ToArray();
				}
			}

			return Array.Empty<PublisherDoc>();
		}

		/// <summary>
		/// Gets the isbn from search query.
		/// </summary>
		/// <param name="search">The search.</param>
		/// <returns></returns>
		public async Task<string[]> GetISBNFromSearch(string search)
		{
			string request = "http://openlibrary.org/search.json?q=" + Uri.EscapeUriString(search);

			using (HttpClient client = new HttpClient())
			{
				var response = await client.GetAsync(request).ConfigureAwait(false);

				if (response.IsSuccessStatusCode)
				{
					OpenLibrary data =
						JsonConvert.DeserializeObject<OpenLibrary>(await response.Content.ReadAsStringAsync()
							.ConfigureAwait(false));

					return data.docs.Select(doc => doc.isbn != null && doc.isbn.Count > 0 ? doc.isbn[0] : "")
						.Where(s => !string.IsNullOrWhiteSpace(s)).ToArray();
				}
			}

			return Array.Empty<string>();
		}

		/// <summary>
		/// Gets the author from search.
		/// </summary>
		/// <param name="search">The search.</param>
		/// <returns></returns>
		public async Task<string[]> GetAuthorFromSearch(string search)
		{
			string request = "";
			var version = GetISBNVersion(search);

			if (version == VERSION.INVALID)
			{
				request = "http://openlibrary.org/search.json?title=" + Uri.EscapeUriString(search);
			}
			else
			{
				request = "http://openlibrary.org/search.json?isbn=" + Uri.EscapeUriString(search);
			}

			using (HttpClient client = new HttpClient())
			{
				var response = await client.GetAsync(request).ConfigureAwait(false);

				if (response.IsSuccessStatusCode)
				{
					OpenLibrary data =
						JsonConvert.DeserializeObject<OpenLibrary>(await response.Content.ReadAsStringAsync()
							.ConfigureAwait(false));

					return data.docs
						.Select(doc =>
							doc.author_name != null && doc.author_name.Count > 0
								? string.Join(", ", doc.author_name)
								: "").Where(s => !string.IsNullOrWhiteSpace(s)).ToArray();
				}
			}

			return Array.Empty<string>();
		}

		/// <summary>
		/// Gets the publisher from search.
		/// </summary>
		/// <param name="search">The search.</param>
		/// <returns></returns>
		public async Task<string[]> GetPublisherFromSearch(string search)
		{
			string request = "";
			var version = GetISBNVersion(search);

			if (version == VERSION.INVALID)
			{
				request = "http://openlibrary.org/search.json?title=" + Uri.EscapeUriString(search);
			}
			else
			{
				request = "http://openlibrary.org/search.json?isbn=" + Uri.EscapeUriString(search);
			}

			using (HttpClient client = new HttpClient())
			{
				var response = await client.GetAsync(request).ConfigureAwait(false);

				if (response.IsSuccessStatusCode)
				{
					OpenLibrary data =
						JsonConvert.DeserializeObject<OpenLibrary>(await response.Content.ReadAsStringAsync()
							.ConfigureAwait(false));

					return data.docs
						.Select(doc =>
							doc.publisher != null && doc.publisher.Count > 0 ? string.Join(", ", doc.publisher) : "")
						.Where(s => !string.IsNullOrWhiteSpace(s)).ToArray();
				}
			}

			return Array.Empty<string>();
		}

		/// <summary>
		/// Gets the information from search.
		/// </summary>
		/// <param name="search">The search.</param>
		/// <returns></returns>
		public async Task<Doc> GetInfoFromSearch(string search)
		{
			string request = "";
			var version = GetISBNVersion(search);

			if (version == VERSION.INVALID)
			{
				request = "http://openlibrary.org/search.json?title=" + Uri.EscapeUriString(search);
			}
			else
			{
				request = "http://openlibrary.org/search.json?isbn=" + Uri.EscapeUriString(search);
			}

			using (HttpClient client = new HttpClient())
			{
				var response = await client.GetAsync(request).ConfigureAwait(false);

				if (response.IsSuccessStatusCode)
				{
					OpenLibrary data =
						JsonConvert.DeserializeObject<OpenLibrary>(await response.Content.ReadAsStringAsync()
							.ConfigureAwait(false));

					var doc = data.docs.Count > 0 ? data.docs[0] : new Doc();

					if (doc.isbn != null && doc.isbn.Count > 0)
					{
						if (doc.place == null)
						{
							doc.place = new List<string>();
						}

						if (doc.place.Count == 0)
						{
							doc.place.Add(await GetCountry(doc.isbn[0]).ConfigureAwait(false));
						}
					}

					return doc;
				}
			}

			return new Doc();
		}
	}
}