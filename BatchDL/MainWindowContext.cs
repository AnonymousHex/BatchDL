using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Windows;
using System.Windows.Forms;
using BatchDL.Exceptions;
using BatchDL.Options;
using BatchDL.UI;
using MessageBox = System.Windows.MessageBox;
using WebBrowser = System.Windows.Forms.WebBrowser;

namespace BatchDL
{
	internal delegate string FormatTitle(string title);
	internal delegate void DocumentDownloadHandler(object sender, WebBrowserDocumentCompletedEventArgs args, Website site, WebsiteOptions options);

	/// <summary>
	/// 
	/// </summary>
	public class MainWindowContext : ObservableObject
	{
		private readonly Dictionary<Website, FormatTitle> _formatters = new Dictionary<Website, FormatTitle>
		{
			{Website.FourChan, Utilities.GetFormatted4ChanTitle},
			{Website.NHentai, Utilities.GetFormattedNHentaiTitle},
			{Website.EHentai, Utilities.GetFormattedEHentaiTitle},
		};

		private readonly List<KeyValuePair<Website, WebsiteOptions>> _options;
		private readonly Dictionary<Website, DocumentDownloadHandler> _downloaders;
		private Command _downloadCommand;
		private static readonly WebClient Client = new WebClient();
		private bool _isDownloading;

		private readonly FourChanOptionsContext _4ChanOptionsContext = new FourChanOptionsContext();
		private readonly NHentaiOptionsContext _nHentaiOptionsContext = new NHentaiOptionsContext();
		private readonly EHentaiOptionsContext _eHentaiOptionsContext = new EHentaiOptionsContext();

		public MainWindowContext()
		{
			_eHentaiOptionsContext.Folder = @"C:\Projects\test";
			_eHentaiOptionsContext.Url = @"https://e-hentai.org/s/dfd9b9243c/723937-29"; //todo remove

			_downloaders = new Dictionary<Website, DocumentDownloadHandler>
			{
				{Website.FourChan, FourChanBrowserOnDocumentCompleted},
				{Website.NHentai, NHentaiBrowserOnDocumentCompleted},
				{Website.EHentai, EHentaiBrowserOnDocumentCompleted},
			};

			_options = new List<KeyValuePair<Website, WebsiteOptions>>
			{
				new KeyValuePair<Website, WebsiteOptions>(Website.FourChan, _4ChanOptionsContext),
				new KeyValuePair<Website, WebsiteOptions>(Website.NHentai, _nHentaiOptionsContext),
				new KeyValuePair<Website, WebsiteOptions>(Website.EHentai, _eHentaiOptionsContext),
			};

			SelectedTab = 2; //todo remove
		}

		public EHentaiOptionsContext EHentaiOptions
		{
			get { return _eHentaiOptionsContext; }
		}

		public NHentaiOptionsContext NHentaiOptions
		{
			get { return _nHentaiOptionsContext; }
		}

		public FourChanOptionsContext FourChanOptions
		{
			get { return _4ChanOptionsContext; }
		}

		/// <summary>
		/// 
		/// </summary>
		public int SelectedTab { get; set; }
		
		public Command DownloadCommand
		{
			get { return _downloadCommand ?? (_downloadCommand = new Command(Download)); }
		}

		public bool CanDownload
		{
			get { return _isDownloading == false; }
			set
			{
				Set("CanDownload", ref _isDownloading, !value);
				OnPropertyChanged("IsDownloading");
			}
		}

		public bool IsDownloading
		{
			get { return _isDownloading; }
		}

		/// <summary>
		/// 
		/// </summary>
		private void Download()
		{
			try
			{
				CanDownload = false;
				var downloader = _options[SelectedTab].Value;
				var site = Utilities.GetWebsite(downloader.Url);
				var browser = new WebBrowser
				{
					ScriptErrorsSuppressed = true,
				};

				browser.Navigate(new Uri(downloader.Url));
				WebBrowserDocumentCompletedEventHandler handler = null;
				handler = (sender, args) =>
				{
					_downloaders[site].Invoke(sender, args, site, downloader);
					browser.DocumentCompleted -= handler;
				};

				browser.DocumentCompleted += handler;
			}
			catch (WebException)
			{
				MessageBox.Show("Error downloading", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
				CanDownload = true;
			}
			catch (InvalidUrlException ex)
			{
				MessageBox.Show(string.Format("{0}\n\nUrl: {1}", ex.Message, ex.Url), "Invalid Url", MessageBoxButton.OK, MessageBoxImage.Error);
				CanDownload = true;
			}
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="args"></param>
		/// <param name="site"></param>
		/// <param name="options"></param>
		private void NHentaiBrowserOnDocumentCompleted(object sender, WebBrowserDocumentCompletedEventArgs args, Website site, WebsiteOptions options)
		{
			var browser = (WebBrowser)sender;
			if (browser.Document?.Body == null)
				return;

			var title = _formatters[site].Invoke(browser.DocumentTitle);

			//create subfolder for thread
			Directory.CreateDirectory(Path.Combine(options.Folder, title));
			const string thumbStart = "t.nhentai.net/galleries/";
			string id = browser.Document.Body.OuterHtml
				.Substring(browser.Document.Body.OuterHtml.IndexOf(thumbStart, StringComparison.Ordinal) + thumbStart.Length)
				.TakeWhile(char.IsDigit).Aggregate("", (current, c) => current + c);
			
			int fails = 0;
			int image = 1;
			while (true)
			{
				try
				{
					var fileName = image + ".jpg";
					var url = string.Format("https://i.nhentai.net/galleries/{0}/{1}", id, fileName);
					Client.DownloadFile(url, Path.Combine(options.Folder, title, fileName));
					image++;
				}
				catch (WebException)
				{
					try
					{
						var fileName = image + ".png";
						var url = string.Format("https://i.nhentai.net/galleries/{0}/{1}", id, fileName);
						Client.DownloadFile(url, Path.Combine(options.Folder, title, fileName));
						image++;
					}
					catch (WebException ex)
					{
						Debug.WriteLine(ex);

						fails++;
						if (fails == 3)
						{
							browser.Dispose();

							CanDownload = true;
							return;
						}
					}
				}
			}
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="args"></param>
		/// <param name="site"></param>
		/// <param name="options"></param>
		private void EHentaiBrowserOnDocumentCompleted(object sender, WebBrowserDocumentCompletedEventArgs args, Website site, WebsiteOptions options)
		{
			//TODO make a list of all the pages that have HQ images, grab links, window.open them and replace as necessary
			var browser = (WebBrowser)sender;
			if (browser.Document?.Body == null)
				return;

			var title = _formatters[site].Invoke(browser.DocumentTitle);

			//create subfolder for thread
			Directory.CreateDirectory(Path.Combine(options.Folder, title));

			CanDownload = true;

			int fails = 0;
			int image = 1;
			while (true)
			{
				try
				{
					//todo get each page, onload save image, if there's a big link, cache url

					var fullSizeLink = browser.Document.GetElementById("i7");
					if (fullSizeLink != null)
					{
						var start = fullSizeLink.OuterHtml.IndexOf("href=", StringComparison.Ordinal) + 6;
						var end = fullSizeLink.OuterHtml.IndexOf(">", start, StringComparison.Ordinal) - 1;
						var url = fullSizeLink.OuterHtml.Substring(start, end - start);
					}
					else
					{

					}

					var fileName = image + ".jpg";
					//var url = string.Format("https://i.nhentai.net/galleries/{0}/{1}", id, fileName);
					//Client.DownloadFile(url, Path.Combine(options.Folder, title, fileName));
					image++;
				}
				catch (WebException)
				{
					try
					{
						var fileName = image + ".png";
						//var url = string.Format("https://i.nhentai.net/galleries/{0}/{1}", id, fileName);
						//Client.DownloadFile(url, Path.Combine(options.Folder, title, fileName));
						image++;
					}
					catch (WebException ex)
					{
						Debug.WriteLine(ex);

						fails++;
						if (fails == 3)
						{
							browser.Dispose();

							CanDownload = true;
							return;
						}
					}
				}
			}
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="args"></param>
		/// <param name="site"></param>
		/// <param name="options"></param>
		private void FourChanBrowserOnDocumentCompleted(object sender, WebBrowserDocumentCompletedEventArgs args, Website site, WebsiteOptions options)
		{
			if (options.Url.Contains("boards.4chan.org") == false || options.Url.Contains("/thread/") == false)
			{
				MessageBox.Show("Invalid web url.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
				CanDownload = true;
				return;
			}

			var browser = (WebBrowser)sender;
			if (browser.Document?.Body == null)
				return;

			int count = 0;
			int total = 0;
			var title = _formatters[site].Invoke(browser.DocumentTitle);

			//create subfolder for thread
			Directory.CreateDirectory(Path.Combine(options.Folder, title));

			foreach (var element in browser.Document.Body.GetElementsByTagName("div").OfType<HtmlElement>())
			{
				try
				{
					var child = element.Children.OfType<HtmlElement>().FirstOrDefault(e => e.GetAttribute("className") == "fileText");
					if (child == null)
						continue;

					total++;
					if (((FourChanOptionsContext)options).ParseInnerHtmlAndDownload(child.OuterHtml, title, Client))
						count++;
				}
				catch (Exception ex)
				{
					Debug.WriteLine(ex.Message);
				}
			}

			MessageBox.Show(
				string.Format("Successfully downloaded {0} images out of {2} to \"{1}\".", count, options.Folder, total),
				"Success",
				MessageBoxButton.OK,
				MessageBoxImage.Information);

			browser.Dispose();

			CanDownload = true;
		}
	}
}
