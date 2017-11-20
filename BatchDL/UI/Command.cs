using System;
using System.Windows.Input;

namespace BatchDL.UI
{
	/// <summary>
	/// Simple Command class for passing methods.
	/// </summary>
	public class Command : ICommand
	{
		private readonly Action _action;
		private readonly Func<bool> _canExecute;
		private bool _lastResult;

		public Command(Action execute, Func<bool> canExecute = null)
		{
			_action = execute;
			_canExecute = canExecute;
		}

		public bool CanExecute(object parameter)
		{
			if (_canExecute == null)
				return true;

			var result = _canExecute.Invoke();
			if (result != _lastResult)
			{
				_lastResult = result;
				OnCanExecuteChanged();
			}

			return result;
		}

		public void Execute(object parameter)
		{
			if (CanExecute(parameter))
				_action.Invoke();
		}

		public event EventHandler CanExecuteChanged;

		void OnCanExecuteChanged()
		{
			var handler = CanExecuteChanged;
			if (handler != null)
				handler.Invoke(this, EventArgs.Empty);
		}
	}
}
