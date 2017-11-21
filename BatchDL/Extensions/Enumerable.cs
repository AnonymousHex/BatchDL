using System;
using System.Collections.Generic;

namespace BatchDL.Extensions
{
	public static class Enumerable
	{
		/// <summary>
		/// 
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="enumerable"></param>
		/// <param name="action"></param>
		public static void ForEach<T>(this IEnumerable<T> enumerable, Action<T> action)
		{
			foreach (var element in enumerable)
				action.Invoke(element);
		}
	}
}
