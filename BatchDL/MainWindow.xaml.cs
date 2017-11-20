using System.Windows;
using System.Windows.Input;

namespace BatchDL
{
	/// <summary>
	/// Interaction logic for MainWindow.xaml
	/// </summary>
	public partial class MainWindow : Window
	{
		public MainWindow()
		{
			InitializeComponent();
			DataContext = new MainWindowContext();
			KeyDown += OnKeyDown;
		}

		private void OnKeyDown(object sender, KeyEventArgs keyEventArgs)
		{
			if (keyEventArgs.Key == Key.Escape)
				Close();
		}
	}
}
