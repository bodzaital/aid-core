using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text.RegularExpressions;
using System.IO;
using System.IO.Compression;
using HtmlAgilityPack;
using System.Text;
using Newtonsoft.Json;

namespace aid_core2
{
	class Program
	{
		static void Main(string[] args)
		{
			Uri uri = new Uri("https://www.amazon.com/French-Connection-Whisper-Sleeveless-Strappy/dp/B07BFRVY11");

			Uri[] uris = GetImageLinks(uri);
			
			foreach (Uri link in uris)
			{
				Console.WriteLine(link);
			}

			Console.WriteLine("Done.");
			Console.ReadLine();
		}

		private static string GetSource(Uri uri)
		{
			using (WebClient client = new WebClient())
			{
				// Download source.
				Console.WriteLine("Downloading source.");
				client.Headers.Add("User-Agent", "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/60.0.3112.113 Safari/537.36");
				byte[] byteStream = client.DownloadData(uri);

				// Inflate with gzip.
				Console.WriteLine("Inflating.");
				GZipStream gzip = new GZipStream(new MemoryStream(byteStream), CompressionMode.Decompress);
				MemoryStream inflatedStream = new MemoryStream();
				gzip.CopyTo(inflatedStream);

				// Set encoding.
				Console.WriteLine("Decoding.");
				Encoding encoding = Encoding.Default;
				return encoding.GetString(inflatedStream.GetBuffer(), 0, (int)inflatedStream.Length);
			}
		}

		private static Uri[] GetImageLinks(Uri uri)
		{
			string source = GetSource(uri);

			const string START_DETECT = "P.when('A').register(\"ImageBlockATF\", function(A){";
			const string END_DETECT = "A.trigger('P.AboveTheFold'); // trigger ATF event.";
			const string REFINED_START_DETECT = "{";
			const string REFINED_END_DETECT = "};";

			// Initial substring to isolate the data.
			Console.WriteLine("Isolating (1).");
			int start = source.IndexOf(START_DETECT) + START_DETECT.Length;
			int end = source.IndexOf(END_DETECT, start);
			string substring = source.Substring(start, end - start);

			// Refined substring so only the json object remains.
			Console.WriteLine("Isolating (2).");
			start = substring.IndexOf(REFINED_START_DETECT);
			end = substring.IndexOf(REFINED_END_DETECT, start) + REFINED_END_DETECT.Length;
			substring = substring.Substring(start, end - start - 1);

			// Deserializing the JSON object to a dynamic variable.
			Console.WriteLine("Deserializing.");
			dynamic a = JsonConvert.DeserializeObject(substring);
			Uri[] uris = new Uri[a.colorImages.initial.Count];
			for (int i = 0; i < uris.Length; i++)
			{
				uris[i] = a.colorImages.initial[i].hiRes;
			}

			return uris;
		}

		private static void DownloadImages(Uri[] uris)
		{

		}
	}
}
