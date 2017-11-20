using BatchDL.UI;

namespace BatchDL.Options
{
	public abstract class WebsiteOptions : ObservableObject
	{
		private string _url = "";
		private string _folder = "";

		public string Url
		{
			get { return _url; }
			set { Set("Url", ref _url, value); }
		}

		public string Folder
		{
			get { return _folder; }
			set { Set("Folder", ref _folder, value); }
		}
	}
}
