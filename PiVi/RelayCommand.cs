namespace PiVi
{
    using System;
    using System.Windows.Input;

    public class RelayCommand : ICommand
    {
        private readonly Action<object> _action;
        private readonly Predicate<object> _canExecute;

        public RelayCommand(Action action, Func<bool> canExecute = null)
        {
            if (action == null)
            {
                throw new ArgumentNullException("action");
            }

            this._action = _ => action();

            if (canExecute != null)
            {
                this._canExecute = _ => canExecute();
            }
        }

        public RelayCommand(Action<object> action, Predicate<object> canExecute = null)
        {
            if (action == null)
            {
                throw new ArgumentNullException("action");
            }

            this._action = action;
            this._canExecute = canExecute;
        }

        public event EventHandler CanExecuteChanged;

        public bool CanExecute(object parameter)
        {
            return this._canExecute == null || this._canExecute(parameter);
        }

        public void Execute(object parameter)
        {
            this._action(parameter);
        }
    }
}
