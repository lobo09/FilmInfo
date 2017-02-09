using FilmInfo.Model;
using FilmInfo.Utility;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FilmInfo.ViewModels
{
    public class DetailViewModel : INotifyPropertyChanged
    {
        private Movie selectedMovie;
        public Movie SelectedMovie
        {
            get { return selectedMovie; }
            set
            {
                selectedMovie = value;
                RaiseOnPropertyChanged("SelectedMovie");
            }
        }

        public DetailViewModel()
        {
            Messenger.Default.Register<Movie>(this, OnMovieRecieved);
        }

        private void OnMovieRecieved(Movie movie)
        {
            SelectedMovie = movie;
        }

        public event PropertyChangedEventHandler PropertyChanged;

        private void RaiseOnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
