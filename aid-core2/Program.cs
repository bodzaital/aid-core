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
			string uriArgument = args[Array.IndexOf(args, "-u") + 1];

			Uri uri = new Uri(uriArgument);
			bool detectAlreadyExist = !args.Contains("-!");

			string product = GetProductName(uri);
			if (detectAlreadyExist && Directory.Exists(product))
			{
				Console.WriteLine($"Images of this product are already present.\nRemove or rename directory \"{product}\" and restart to download them\nor use -! flag to override this setting.");
			}
			else
			{
				string source;
				try
				{
					source = GetSource(uri);
				}
				catch (Exception)
				{
					Console.WriteLine("503: Service Unavailable.\nThe server refused the connection. Try again later.");
					return;
				}
				Uri[] imageUris = GetImageLinks(source);
				Uri[] videoUris = GetVideoLinks(source);
				DownloadImages(uri, imageUris);
				DownloadVideos(uri, videoUris);
				Console.WriteLine("Done.");
			}
		}

		/// <summary>
		/// Gets the page source from the given URI.
		/// </summary>
		/// <param name="uri">The URI of the product page.</param>
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

		/// <summary>
		/// Extracts high resolution image links from the page source.
		/// </summary>
		/// <param name="uri">The URI of the product page.</param>
		private static Uri[] GetImageLinks(string source)
		{
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

		/// <summary>
		/// Downloads the high resolution images.
		/// </summary>
		/// <param name="imageUris">An array of URIs of images.</param>
		private static void DownloadImages(Uri productUri, Uri[] imageUris)
		{
			string product = GetProductName(productUri);
			Directory.CreateDirectory(product);
			using (WebClient client = new WebClient())
			{
				for (int i = 0; i < imageUris.Length; i++)
				{
					client.DownloadFile(imageUris[i].ToString(), $"{product}/image_{i}.jpg");
					Console.WriteLine($"Downloaded to {product}/image_{i}.jpg");
				}
			}
		}

		/// <summary>
		/// Extracts the product name from the product URI.
		/// </summary>
		/// <param name="uri">The URI of the product page.</param>
		private static string GetProductName(Uri uri)
		{
			const string START_DETECT = ".com/";
			const string END_DETECT = "/dp/";

			int start = uri.ToString().IndexOf(START_DETECT) + START_DETECT.Length;
			int end = uri.ToString().IndexOf(END_DETECT, start);
			string name;

			try
			{
				name = uri.ToString().Substring(start, end - start);
			}
			catch (Exception)
			{
				name = uri.ToString().Replace('/', '-').Replace(':', '-');
			}

			return name;
		}

		private static Uri[] GetVideoLinks(string source)
		{
			const string START_DETECT = "P.when('A','jQuery').execute('triggerVideoAjax'";
			const string END_DETECT = "A.trigger('triggerVideoAjax',obj.videos);";

			int start = source.IndexOf(START_DETECT) + START_DETECT.Length;
			int end = source.IndexOf(END_DETECT, start);

			string substring = source.Substring(start, end - start);

			const string REFINED_START_DETECT = "jQuery.parseJSON('";
			const string REFINED_END_DETECT = "');";

			start = substring.IndexOf(REFINED_START_DETECT) + REFINED_START_DETECT.Length;
			end = substring.IndexOf(REFINED_END_DETECT, start);

			substring = substring.Substring(start, end - start);

			// Deserializing the JSON object to a dynamic variable.
			Console.WriteLine("Deserializing.");
			dynamic a = JsonConvert.DeserializeObject(substring);
			Uri[] uris = new Uri[a.videos.Count];
			for (int i = 0; i < uris.Length; i++)
			{
				uris[i] = a.videos[i].url;
			}

			return uris;
		}

		private static void DownloadVideos(Uri productUri, Uri[] videoUris)
		{
			string product = GetProductName(productUri);
			Directory.CreateDirectory(product);
			using (WebClient client = new WebClient())
			{
				for (int i = 0; i < videoUris.Length; i++)
				{
					client.DownloadFile(videoUris[i].ToString(), $"{product}/video_{i}.mp4");
					Console.WriteLine($"Downloaded to {product}/video_{i}.mp4");
				}
			}
		}
	}
}
