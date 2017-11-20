using System;

namespace BatchDL.Exceptions
{
	class InvalidUrlException : Exception
	{
		public string Url { get; private set; }

		public InvalidUrlException(string url, string message)
			:base(message)
		{
			Url = url;
		}
	}
}
