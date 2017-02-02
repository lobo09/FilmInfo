using FilmInfo.Services;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FilmInfo.Utility;
using FilmInfo.Model;
using System.IO;
using System.Windows;

namespace FilmInfo.ViewModels
{
    public class MainViewModel : INotifyPropertyChanged
    {
        public CustomCommand ScanDirectoryCommand { get; set; }
        public CustomCommand SortCommand { get; set; }
        public event PropertyChangedEventHandler PropertyChanged;
        private DataService dataService;


        private Movie selectedMovie;
        public Movie SelectedMovie
        {
            get
            {
                return selectedMovie;
            }
            set
            {
                selectedMovie = value;
                RaisePropertyChanged("SelectedMovie");
            }
        }

        private ObservableCollection<Movie> movies;
        public ObservableCollection<Movie> Movies
        {
            get
            {
                return movies;
            }
            set
            {
                movies = value;
                RaisePropertyChanged("Movies");
            }
        }

        public MainViewModel()
        {
            LoadCommands();
            dataService = new DataService();
        }

        private void LoadCommands()
        {
            ScanDirectoryCommand = new CustomCommand(ScanDirectory);
            SortCommand = new CustomCommand(SortMovies);
        }

        private void ScanDirectory(object obj)
        {
            dataService.ScanAllMovies();
            Movies = dataService.GetAllMovies().ToObservableCollection();
        }

        private void SortMovies(object obj)
        {
            string sortType = obj as string;

            if (sortType == "name")
            {
                MessageBox.Show("Name");
            }
            else if (sortType == "date")
            {
                MessageBox.Show("Datum");
            }
            else if (sortType == "newest")
            {
                MessageBox.Show("Neueste");
            }
        }

        

        private void RaisePropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
