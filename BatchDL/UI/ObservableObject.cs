using System.ComponentModel;

namespace BatchDL.UI
{
	/// <summary>
	/// Bindable base class
	/// </summary>
	public class ObservableObject : INotifyPropertyChanged
	{
		public event PropertyChangedEventHandler PropertyChanged;

		protected virtual void OnPropertyChanged(string propertyName = null)
		{
			var handler = PropertyChanged;
			if (handler != null)
				handler(this, new PropertyChangedEventArgs(propertyName));
		}

		// ReSharper disable once RedundantAssignment
		protected void Set<T>(string propertyName, ref T field, T value)
		{
			field = value;
			OnPropertyChanged(propertyName);
		}
	}
}
