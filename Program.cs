using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text.RegularExpressions;
using System.IO;
using HtmlAgilityPack;

namespace aid_core
{
	class Program
	{
		static string userAgent, acceptString;
		static bool downloadVideo, useCustomUserAgent, useCustomAcceptString, useTextFile, displayDebugInfo = false;
		static List<string> imageUris = new List<string>();
		static string productPageSource;
		static Regex linkRule = new Regex("https?://([\\w+?\\.\\w+])+([a-zA-Z0-9\\~\\!\\@\\#\\$\\%\\^\\&amp;\\*\\(\\)_\\-\\=\\+\\\\\\/\\?\\.\\:\\;\\'\\,]*)?", RegexOptions.IgnoreCase);
		static Regex colorImagesFromJsRule = new Regex("'colorImages'.*", RegexOptions.IgnoreCase);
		static Regex videoRule = new Regex("holderId.*", RegexOptions.IgnoreCase);

		static void Main(string[] args)
		{
			// Two basic settings: User Agent String, and Accept String (so Amazon won't provide us a CAPTCHA page).
			userAgent = "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/56.0.2924.87 Safari/537.36";
			acceptString = "text/html,application/xhtml+xml,application/xml;q=0.9,image/webp,*/*;q=0.8";

			Console.WriteLine("Amazon Image Downloader v1.1-aid-core");

			ReadFlags(args);
			DisplayFlags(args);

			if (!useTextFile)
			{
				Console.Write("Amazon product page> ");
				imageUris.Add(Console.ReadLine());
			}

			for (int i = 0; i < imageUris.Count(); i++)
			{
				try
				{
					WebClient webClient = new WebClient();

					GetProductPage(imageUris[i]);

					HtmlSource htmlDocument = new HtmlSource(productPageSource);

					Product product = new Product(
						htmlDocument.DocumentNode.Descendants("span")
							.Where(e => e.GetAttributeValue("id", "")
							.Equals("productTitle"))
							.First().InnerText.Trim(),
						htmlDocument.DocumentNode.Descendants("span")
							.Where(e => e.GetAttributeValue("id", "")
							.Equals("priceblock_ourprice"))
							.First().InnerText.Trim(),
						htmlDocument.DocumentNode.Descendants("div")
							.Where(e => e.GetAttributeValue("id", "")
							.Equals("productDescription"))
							.First().Descendants("p").First().InnerText.Trim()
					);

					// This regex first gets the colorImages block, then extracts links.
					MatchCollection images = linkRule.Matches(colorImagesFromJsRule.Match(productPageSource).Value);

					string downloadDirectory = "downloads/" + product.name.Replace(' ', '-').Replace('\'', '-').Replace('/', '-');
					

					if (!Directory.Exists(downloadDirectory))
					{
						Directory.CreateDirectory(downloadDirectory);
					}
					
					product.SaveNotes(downloadDirectory);

					if (!Directory.Exists(downloadDirectory + "/img"))
					{
						Directory.CreateDirectory(downloadDirectory + "/img");
					}

					if (downloadVideo)
					{
						if (!Directory.Exists(downloadDirectory + "/mp4"))
						{
							Directory.CreateDirectory(downloadDirectory + "/mp4");
						}

						int videoStartIndex = productPageSource.IndexOf("holderId");

						// A value of -1 means the regex did not found links.
						if (productPageSource.IndexOf("holderId") != -1)
						{
							MatchCollection videoUrls = linkRule.Matches(videoRule.Match(productPageSource).Value);
							foreach (Match item in videoUrls)
							{
								if (item.ToString().Contains("mp4"))
								{
									Console.Write("Attempting to download video... ");
									webClient.DownloadFile(item.ToString(), downloadDirectory + "/mp4/" + Path.GetFileName(item.ToString()));
									Console.WriteLine("Done.");
								}
							}
						}
						else
						{
							Console.WriteLine("No video link found, skipping...");
						}
					}

					Console.WriteLine("Attempting to download images:");

					float downloadedCount = 1;

					foreach (Match imageUri in images)
					{
						float downloadedPercentage = downloadedCount++ / images.Count() * 100;
						Console.Write("\rDownloading images " + Math.Floor(downloadedPercentage) + "%");
						webClient.DownloadFile(imageUri.ToString(), downloadDirectory + "/img/" + Path.GetFileName(imageUri.ToString()));
					}

					Console.Write("\n\n----RESULTS----\n" + downloadedCount + " file");
					if (downloadedCount > 1)
					{
						Console.Write("s");
					}
					Console.WriteLine(" were downloaded to " + downloadDirectory + "\n---------------");
				}
				catch (InvalidOperationException e)
				{
					// Thrown when the regex fails finding one or all of the searches.

					Console.WriteLine("FAILED.\n\nOne or all of the following elements were not found:\n- Product title\n- Product price\n- Product description.\n\nThis may not be an Amazon product page.\n\n----Technical details:----\n" + e.Message + "\n--------------------------");
				}
				catch (Exception e)
				{
					// Gotta catch 'em all.
					Console.WriteLine("FAILED.\n\n----Technical details:----\n" + e.Message + "\n--------------------------");
					if (displayDebugInfo)
					{
						Console.WriteLine(e);
					}
				}
			}
		}

		private static void GetProductPage(string cleanProductUri)
		{
			using (HttpClient httpClient = new HttpClient())
			{
				Console.Write("\nSetting accept string and user-agent... ");

				httpClient.DefaultRequestHeaders.TryAddWithoutValidation("Accept", acceptString);
				httpClient.DefaultRequestHeaders.TryAddWithoutValidation("User-Agent", userAgent);

				Console.Write("Done.\nDownloading page... ");

				using (HttpResponseMessage httpResponseMessage = httpClient.GetAsync(cleanProductUri).Result)
				{
					using (HttpContent httpContent = httpResponseMessage.Content)
					{
						productPageSource = httpContent.ReadAsStringAsync().Result;
						Console.WriteLine("Done.");
					}
				}
			}
		}

		private static void ReadFlags(string[] args)
		{
			if (args.Contains("-v")) { downloadVideo = true; }
			if (args.Contains("-u")) { useCustomUserAgent = true; }
			if (args.Contains("-a")) { useCustomAcceptString = true; }
			if (args.Contains("-t")) { useTextFile = true; }
			if (args.Contains("-d")) { displayDebugInfo = true; }
		}

		private static void DisplayFlags(string[] args)
		{
			if (args.Length > 0)
			{
				Console.WriteLine("\nThe following flags were set:");

				if (downloadVideo)
				{
					Console.WriteLine(" - Download product video");
				}

				if (useCustomUserAgent)
				{
					Console.WriteLine(" - Custom User Agent: \"" + args[Array.IndexOf(args, "-u") + 1] + "\"");
					userAgent = args[Array.IndexOf(args, "-u") + 1];
				}

				if (useCustomAcceptString)
				{
					Console.WriteLine(" - Custom Accept Agent: \"" + args[Array.IndexOf(args, "-a") + 1] + "\"");
					acceptString = args[Array.IndexOf(args, "-a") + 1];
				}

				if (useTextFile)
				{
					Console.WriteLine(" - Read from text file: " + args[Array.IndexOf(args, "-t") + 1]);
					imageUris = File.ReadAllLines(args[Array.IndexOf(args, "-t") + 1]).OfType<string>().ToList();
				}
			}
			else
			{
				Console.WriteLine("No flags were set.");
			}
			
			// An empty line a day keeps the doctor away.
			Console.WriteLine();
		}
	}

	// Custom implementation of an HtmlDocument function. So instead of:
	//     HtmlDocument doc = new HtmlDocument();
	//     doc.LoadHtml(sourceCode);
	//
	// We can do the same in the constructor:
	//     HtmlSource doc = new HtmlSource(sourceCode);
	public class HtmlSource : HtmlDocument
	{
		public HtmlSource(string source)
		{
			HtmlNode.ElementsFlags.Remove("option");
			this.LoadHtml(source);
		}
	}

	public class Product
	{
		public string name { get; set; }
		public double price { get; set; }
		public string description { get; set; }
		
		public Product(string name, string price, string description)
		{
			this.name = name;
			this.price = Convert.ToDouble(price.Substring(1, price.IndexOf('.') + 2).Replace('.', ','));
			this.description = description;
		}

		public override string ToString()
		{
			return "Name: " + name + ", Price: " + price + ", Description: " + description;
		}

		public void SaveNotes(string downloadDirectory)
		{
			StreamWriter sw = new StreamWriter(downloadDirectory + "/notes.txt");
			sw.WriteLine("Name\t" + name);
			sw.WriteLine("Price\t" + price);
			sw.WriteLine("Description\t" + description);
			sw.Close();
		}
	}
}
