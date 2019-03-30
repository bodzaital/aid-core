using System;
using System.Collections.Generic;
using System.Text;
using HtmlAgilityPack;

namespace aid_core2
{
	class HtmlSource : HtmlDocument
	{
		public HtmlSource(string source)
		{
			HtmlNode.ElementsFlags.Remove("option");
			LoadHtml(source);
		}
	}
}
