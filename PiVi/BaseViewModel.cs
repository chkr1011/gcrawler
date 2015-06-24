using System;
using System.ComponentModel;
using System.Linq.Expressions;

namespace PiVi
{
    internal abstract class BaseViewModel : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        protected void OnPropertyChanged<T>(Expression<Func<T>> expression)
        {
            if (PropertyChanged == null)
            {
                return;
            }

            string memberName = ((MemberExpression)expression.Body).Member.Name;
            PropertyChanged(this, new PropertyChangedEventArgs(memberName));
        }
    }
}
