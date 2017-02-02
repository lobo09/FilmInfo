using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace FilmInfo.Utility
{
    public class CustomCommand : ICommand
    {
        private Action<object> action;
        private Predicate<object> predicate;

        public CustomCommand(Action<object> action, Predicate<object> predicate)
        {
            this.action = action;
            this.predicate = predicate;
        }
        public CustomCommand(Action<object> action) : this(action, p => true)
        {
        }

        public event EventHandler CanExecuteChanged
        {
            add { CommandManager.RequerySuggested += value; }
            remove { CommandManager.RequerySuggested -= value; }
        }

        public bool CanExecute(object parameter)
        {
            return predicate != null ? predicate(parameter) : true;
        }

        public void Execute(object parameter)
        {
            action(parameter);
        }
    }
}
