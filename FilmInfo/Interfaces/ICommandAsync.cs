using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace FilmInfo.Interfaces
{
    public interface ICommandAsync : ICommand
    {
        Task ExecuteAsync(object parameter);
    }
}
