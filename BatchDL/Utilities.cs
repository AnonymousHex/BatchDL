using System.IO;
using System.Linq;
using System.Net;
using BatchDL.Exceptions;

namespace BatchDL
{
	/// <summary>
	/// 
	/// </summary>
	internal static class Utilities
	{
		/// <summary>
		/// Gets the current website to download from given a url.
		/// </summary>
		/// <param name="url"></param>
		/// <returns></returns>
		public static Website GetWebsite(string url)
		{
			if (url.Contains("nhentai"))
				return Website.NHentai;

			if (url.Contains("4chan"))
				return Website.FourChan;

			if (url.Contains("e-hentai"))
				return Website.EHentai;

			throw new InvalidUrlException(url, "Unknown website.");
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="url"></param>
		/// <param name="folder"></param>
		/// <param name="containingFolder"></param>
		/// <returns></returns>
		public static string GetFilePath(string url, string folder, string containingFolder)
		{
			return Path.Combine(folder, containingFolder, url.Substring(url.LastIndexOf('/') + 1));
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="title"></param>
		/// <returns></returns>
		public static string GetFormatted4ChanTitle(string title)
		{
			return title.Split('-')[1].Trim().RemoveIllegalChars();
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="title"></param>
		/// <returns></returns>
		public static string GetFormattedNHentaiTitle(string title)
		{
			return title.Split('»')[0].Trim().RemoveIllegalChars();
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="title"></param>
		/// <returns></returns>
		public static string GetFormattedEHentaiTitle(string title)
		{
			return title;
		}

		/// <summary>
		/// Removes all illegal path characters from a string path.
		/// </summary>
		/// <param name="input"></param>
		/// <returns></returns>
		public static string RemoveIllegalChars(this string input)
		{
			for (int i = 0; i < input.Length; i++)
			{
				var c = input[i];
				if (c == '?' || Path.GetInvalidPathChars().Contains(c))
					input = input.Replace(c, '-');
			}

			return input;
		}
	}
}
