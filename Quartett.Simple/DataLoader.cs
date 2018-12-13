#region usings

using System;
using System.Dynamic;
using System.IO;
using System.Net.Http;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Quartett.Simple.Data;

#endregion

namespace Quartett.Simple
{
	public static class DataLoader
	{
		private const string alphabet = "abcdefghijklmnopqrstuvwxyz";
		private static readonly string[] apis = {"carqueryapi"};
		private static dynamic set;

		public static void Load()
		{
			var index = Utility.ConsoleSelect("API", apis);
			var selected = apis[index];
			var cardIndex = 0;
			var cardCounter = 1;
			var cardLoaded = 0;

			switch (selected)
			{
				case "carqueryapi":
				{
					HttpClient client = new HttpClient();
					var s = Kartenset.LoadFrom(AppDomain.CurrentDomain.BaseDirectory + "/Data/Autoquartett.json");

					set = new ExpandoObject();
					set.reversals = s.reversals;
					set.cards = new ExpandoObject[64];

					MakeList makes = null;
					var rand = new Random();

					Console.WriteLine("Loading Data...");
					makes = JsonConvert.DeserializeObject<MakeList>(client
						.GetStringAsync("https://www.carqueryapi.com/api/0.3/?cmd=getMakes").Result);

					Console.WriteLine("Selecting cars...");

					var ts = new Task[Environment.ProcessorCount];

					for (int j = 0; j < Environment.ProcessorCount; j++)
					{
						ts[j] = Task.Run(() =>
						{
							while (cardLoaded < 64)
							{
								var make = makes.Makes[rand.Next(0, makes.Makes.Count)];

								TrimList trims = null;

								trims = JsonConvert.DeserializeObject<TrimList>(client
									.GetStringAsync(
										"https://www.carqueryapi.com/api/0.3/?cmd=getTrims&make=" + make.make_id)
									.Result);

								if (trims == null)
								{
									Console.WriteLine("Trims are null for " + make.make_id);
									continue;
								}

								var i = 0;
								Trim trim = null;

								while (trim == null && trims.Trims.Count > i + 1)
								{
									trim = trims.Trims[i];

									if (string.IsNullOrEmpty(trim.model_0_to_100_kph) ||
									    string.IsNullOrEmpty(trim.model_make_display) ||
									    string.IsNullOrEmpty(trim.model_trim) ||
									    string.IsNullOrEmpty(trim.model_engine_cc) ||
									    string.IsNullOrEmpty(trim.model_engine_power_ps) ||
									    string.IsNullOrEmpty(trim.model_top_speed_kph))
									{
										trim = null;
										i++;
									}
								}

								if (trim != null)
								{
									dynamic card = new ExpandoObject();

									card.card_id = alphabet[cardIndex].ToString().ToUpper() + cardCounter;
									card.identifier = (trim.model_make_display.Trim() + " " + trim.model_trim.Trim());
									card.zero_to_hundred = trim.model_0_to_100_kph;
									card.ccm = trim.model_engine_cc;
									card.hp = trim.model_engine_power_ps;
									card.kmh = trim.model_top_speed_kph;

									lock (set)
									{
										if (cardLoaded < 64)
										{
											set.cards[cardLoaded] = card;

											cardLoaded++;
											cardCounter++;

											if (cardCounter > 4)
											{
												cardIndex++;
												cardCounter = 1;
											}
										}
									}
								}
							}
						});
					}

					Task.WaitAll(ts);

					Console.WriteLine("Saving...");

					using (StreamWriter sw =
						new StreamWriter(AppDomain.CurrentDomain.BaseDirectory + "/Data/Autoquartett-New.json"))
					{
						sw.Write(JsonConvert.SerializeObject(set));
						sw.Flush();
					}

					set = null;
					makes = null;
					client.Dispose();
					GC.Collect();

					Console.WriteLine("Done!");

					break;
				}
			}
		}
	}
}