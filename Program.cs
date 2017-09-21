using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Drawing;
using System.IO;

namespace aid_core
{
	class Program
	{
		private static string userAgent, acceptString;
		private static bool downloadVideo = false;

		static void Main(string[] args)
		{

			if (args.Contains("-v") || args.Contains("--video"))
			{
				downloadVideo = true;
			}

			Console.WriteLine("Amazon Image Downloader v1.1-aid-core");

			if (args.Length > 0)
			{
				Console.WriteLine("\nThe following flags were set:");
				if (downloadVideo)
				{
					Console.WriteLine("- Download product video");
				}
			}

			if (!Directory.Exists("img"))
			{
				Console.WriteLine("  Making missing folder: img...");
				Directory.CreateDirectory("img");
			}
			if (!Directory.Exists("mp4") && downloadVideo)
			{
				Console.WriteLine("  Making missing folder: mp4...");
				Directory.CreateDirectory("mp4");
			}

			Console.Write("Amazon product page >");
			string uri = Console.ReadLine();

			userAgent = "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/56.0.2924.87 Safari/537.36";

			acceptString = "text/html,application/xhtml+xml,application/xml;q=0.9,image/webp,*/*;q=0.8";

			string htmlSourceCode;


			try
			{
				using (HttpClient client = new HttpClient())
				{
					Console.Write("\nSetting accept string and user-agent... ");

					client.DefaultRequestHeaders.TryAddWithoutValidation("Accept", acceptString);
					client.DefaultRequestHeaders.TryAddWithoutValidation("User-Agent", userAgent);

					Console.Write("Done.\nDownloading page... ");

					using (HttpResponseMessage response = client.GetAsync(uri).Result)
					{
						using (HttpContent content = response.Content)
						{
							htmlSourceCode = content.ReadAsStringAsync().Result;
							Console.WriteLine("Done.");
						}
					}
				}
				Console.Write("Selecting links... ");

				string beginning = htmlSourceCode.Substring(htmlSourceCode.IndexOf("'colorImages'"));
				string endString = beginning.Substring(0, beginning.IndexOf("</script>"));


				Console.WriteLine("Done.");

				Regex rule = new Regex("https?://([\\w+?\\.\\w+])+([a-zA-Z0-9\\~\\!\\@\\#\\$\\%\\^\\&amp;\\*\\(\\)_\\-\\=\\+\\\\\\/\\?\\.\\:\\;\\'\\,]*)?", RegexOptions.IgnoreCase);
				MatchCollection urls = rule.Matches(endString);
				WebClient webClient = new WebClient();

				if (downloadVideo)
				{
					string videoBeginning = htmlSourceCode.Substring(htmlSourceCode.IndexOf("holderId"));
					string videoEndString = videoBeginning.Substring(0, videoBeginning.IndexOf("offset"));
					MatchCollection videoUrls = rule.Matches(videoEndString);
					foreach (Match item in videoUrls)
					{
						if (item.ToString().Contains("mp4"))
						{
							Console.Write("Attempting to download video... ");
							webClient.DownloadFile(item.ToString(), "mp4/" + Path.GetFileName(item.ToString()));
							Console.WriteLine("Done.");
						}
					}
				}
				


				Console.WriteLine("Attempting to download images:");

				List<string> fileCount = new List<string>();

				float matchIndex = 1;

				foreach (Match item in urls)
				{
					float percent = matchIndex++ / urls.Count() * 100;
					Console.Write("\rDownloading images " + Math.Floor(percent) + "%");
					webClient.DownloadFile(item.ToString(), "img/" + Path.GetFileName(item.ToString()));
					fileCount.Add(Path.GetFileName(item.ToString()));
				}

				Console.WriteLine("\n" + fileCount.Count() + " file(s) were downloaded.");

			}
			catch (System.Exception e)
			{
				Console.WriteLine("Something went wrong.\n\nTechnical details:\n" + e.Message);
				Console.WriteLine(e);
			}
		}
	}
}
