using FilmInfo.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace FilmInfo.Utility
{
    public class CustomCommandAsync : ICommandAsync
    {
        private Func<object, Task> action;
        private Predicate<object> predicate;

        public CustomCommandAsync(Func<object, Task> execute, Predicate<object> canExecute) 
        {
            action = execute;
            predicate = canExecute;
        }
        public CustomCommandAsync(Func<object, Task> execute) : this(execute, null)
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

        public async void Execute(object parameter)
        {
            await ExecuteAsync(parameter);
        }

        public async Task ExecuteAsync(object parameter)
        {
            await action(parameter);
        }
    }
}
