using System;
using System.Windows.Input;

namespace PiVi
{
    internal sealed class RelayCommand : ICommand
    {
        private readonly Action<object> _action;
        private readonly Predicate<object> _canExecute;

        public RelayCommand(Action action, Func<bool> canExecute = null)
        {
            if (action == null)
            {
                throw new ArgumentNullException("action");
            }

            _action = _ => action();

            if (canExecute != null)
            {
                _canExecute = _ => canExecute();
            }
        }

        public event EventHandler CanExecuteChanged;

        public bool CanExecute(object parameter)
        {
            return _canExecute == null || _canExecute(parameter);
        }

        public void Execute(object parameter)
        {
            _action(parameter);
        }
    }
}
