namespace PiVi
{
    using System;
    using System.ComponentModel;
    using System.Linq.Expressions;
    using System.Windows;
    using System.Windows.Threading;

    internal abstract class BaseViewModel : INotifyPropertyChanged
    {
        #region Implementation of INotifyPropertyChanged

        public event PropertyChangedEventHandler PropertyChanged;

        #endregion

        protected void OnPropertyChanged<T>(Expression<Func<T>> expression)
        {
            if (this.PropertyChanged == null)
            {
                return;
            }

            string memberName = ((MemberExpression)expression.Body).Member.Name;
            this.PropertyChanged(this, new PropertyChangedEventArgs(memberName));
        }

        protected object Dispatch(Action action)
        {
            Dispatcher dispatcher = Application.Current.Dispatcher;
            if (!dispatcher.CheckAccess())
            {
                return dispatcher.Invoke(action);
            }

            return dispatcher.Invoke(action);
        }
    }
}
