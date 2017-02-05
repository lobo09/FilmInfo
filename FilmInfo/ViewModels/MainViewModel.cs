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
using System.Windows.Data;

namespace FilmInfo.ViewModels
{
    public class MainViewModel : INotifyPropertyChanged
    {
        public CustomCommand ScanDirectoryCommand { get; set; }
        public CustomCommand GetPosterCommand { get; set; }
        public event PropertyChangedEventHandler PropertyChanged;
        private DataService dataService;

        private int scanProgress;
        public int ScanProgress
        {
            get { return scanProgress; }
            set
            {
                scanProgress = value;
                RaisePropertyChanged("ScanProgress");
            }
        }


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

        private string filter;
        public string Filter
        {
            get { return filter; }
            set
            {
                filter = value;
                RaisePropertyChanged("Filter");
                RefreshMovieList();
            }
        }

        private string sortOption;
        public string SortOption
        {
            get { return sortOption; }
            set
            {
                sortOption = value;
                RaisePropertyChanged("SortOption");
                RefreshMovieList();
            }
        }


        public MainViewModel()
        {
            LoadCommands();
            dataService = new DataService();
            dataService.RegisterProgressChanged(OnProgressChanged);
            SortOption = "name";
        }

        private void LoadCommands()
        {
            ScanDirectoryCommand = new CustomCommand(ScanDirectoryAsync);
            GetPosterCommand = new CustomCommand(GetPoster);
        }

        private void OnProgressChanged(object o, ProgressEventArgs e)
        {
            ScanProgress = (int)e.PercentFinished;
        }

        private async void ScanDirectoryAsync(object obj)
        {
            await dataService.ScanAllMoviesAsync();
            RefreshMovieList();
        }

        private void GetPoster(object obj)
        {
            var movie = obj as Movie;
            throw new NotImplementedException("Get Poster noch nicht implementiert!");
        }

        private void RefreshMovieList()
        {
            Movies = dataService.GetProcessedMovies(SortOption, Filter);
        }



        private void RaisePropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
