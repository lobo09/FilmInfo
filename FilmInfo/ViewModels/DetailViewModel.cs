﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FilmInfo.ViewModels
{
    public class DetailViewModel : INotifyPropertyChanged
    {



        public event PropertyChangedEventHandler PropertyChanged;

        private void RaiseOnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
