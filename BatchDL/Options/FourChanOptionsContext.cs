using System;
using System.Diagnostics;
using System.Linq;
using System.Net;

namespace BatchDL.Options
{
	public class FourChanOptionsContext : WebsiteOptions
	{
		private bool _includeGifWebm;
		private int _minFileSize;
		private int _maxFileSize = 8192;
		private int _minWidth;
		private int _maxWidth = 10000;
		private int _minHeight;
		private int _maxHeight = 10000;

		public int MinWidth
		{
			get { return _minWidth; }
			set { Set("MinWidth", ref _minWidth, value); }
		}

		public int MaxWidth
		{
			get { return _maxWidth; }
			set { Set("MaxWidth", ref _maxWidth, value); }
		}

		public int MinHeight
		{
			get { return _minHeight; }
			set { Set("MinHeight", ref _minHeight, value); }
		}

		public int MaxHeight
		{
			get { return _maxHeight; }
			set { Set("MaxHeight", ref _maxHeight, value); }
		}

		public int MaxFileSize
		{
			get { return _maxFileSize; }
			set { Set("MaxFileSize", ref _maxFileSize, value); }
		}

		public int MinFileSize
		{
			get { return _minFileSize; }
			set { Set("MinFileSize", ref _minFileSize, value); }
		}

		public bool IncludeGifAndWebm
		{
			get { return _includeGifWebm; }
			set { Set("IncludeGifAndWebm", ref _includeGifWebm, value); }
		}

		/// <summary>
		/// Parse the div info, filter on sizes, and download the image if possible.
		/// </summary>
		/// <param name="line"></param>
		/// <param name="threadName"></param>
		/// <param name="client"></param>
		public bool ParseInnerHtmlAndDownload(string line, string threadName, WebClient client)
		{
			if (string.IsNullOrEmpty(line))
				return false;

			var start = line.IndexOf("href=", StringComparison.Ordinal) + 6;
			if (start < 0)
				return false;

			var len = line.IndexOf("\" target", StringComparison.Ordinal) - start;
			var url = "http:" + line.Substring(start, len);

			start = line.IndexOf("</A>", StringComparison.OrdinalIgnoreCase) + 6;
			len = line.IndexOf("</DIV>", StringComparison.OrdinalIgnoreCase) - start - 1;
			var inner = line.Substring(start, len);

			Debug.WriteLine(inner);
			var size = line.Substring(start, line.IndexOf(" ", start, StringComparison.Ordinal) - start);
			bool megabyte = size.Contains('.');
			var fileSize = Convert.ToDouble(size);
			if (megabyte)
				fileSize *= 1024;

			if (fileSize < _minFileSize || fileSize > _maxFileSize)
				return false;

			var dimensions = inner.Substring(inner.IndexOf(", ", StringComparison.OrdinalIgnoreCase) + 2).Split('x');
			var width = Convert.ToInt32(dimensions[0]);
			var height = Convert.ToInt32(dimensions[1]);
			if (width < _minWidth || width > _maxWidth || height < _minHeight || height > _maxHeight)
				return false;

			if (IncludeGifAndWebm == false)
			{
				var ext = url.Substring(url.LastIndexOf('.') + 1);
				if (ext == "gif" || ext == "webm")
					return false;
			}

			try
			{
				client.DownloadFile(new Uri(url), Utilities.GetFilePath(url, Folder, threadName));
			}
			catch (WebException)
			{
				return false;
			}

			return true;
		}
	}
}
