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

namespace aid_core2
{
	class Program
	{
		static void Main(string[] args)
		{
			string url = "https://www.amazon.com/French-Connection-Whisper-Sleeveless-Strappy/dp/B07BFRVY11";

			string source = GetSource(url);

			Console.WriteLine(source);
		}

		private static string GetSource(string url)
		{
			using (WebClient web = new WebClient())
			{
				web.Headers.Add("User-Agent", "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/60.0.3112.113 Safari/537.36");
				Encoding encoding = Encoding.Default;
				byte[] data = web.DownloadData(url);
				GZipStream gzip = new GZipStream(new MemoryStream(data), CompressionMode.Decompress);
				MemoryStream decompressed = new MemoryStream();
				gzip.CopyTo(decompressed);
				return encoding.GetString(decompressed.GetBuffer(), 0, (int)decompressed.Length);
			}
		}
	}
}
