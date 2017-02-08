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
using System.Windows.Threading;
using System.Threading;
using System.Windows.Controls;

namespace FilmInfo.ViewModels
{
    public class MainViewModel : INotifyPropertyChanged
    {
        public CustomCommand ScanDirectoryCommand { get; set; }
        public CustomCommand GetPosterCommand { get; set; }
        public CustomCommand OpenDetailViewCommand { get; set; }
        public event PropertyChangedEventHandler PropertyChanged;
        private DataService dataService;
        private DialogService dialogService;

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

        private bool scanActive;
        public bool ScanActive
        {
            get { return scanActive; }
            set
            {
                scanActive = value;
                RaisePropertyChanged("ScanActive");
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

        private bool moviesProcessed;
        public bool MoviesProcessed
        {
            get { return moviesProcessed; }
            set
            {
                moviesProcessed = value;
                RaisePropertyChanged("MoviesProcessed");
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

        private SortOrderEnum sortOrder;
        public SortOrderEnum SortOrder
        {
            get { return sortOrder; }
            set
            {
                sortOrder = value;
                RaisePropertyChanged("SortOrder");
                RefreshMovieList();
            }
        }

        public MainViewModel()
        {
            LoadCommands();
            dataService = new DataService();
            dialogService = new DialogService();
            var Movies = new ObservableCollection<Movie>();
            dataService.RegisterScanProgressStarted(OnScanProgressStarted);
            dataService.RegisterScanProgressChanged(OnScanProgressChanged);
            dataService.RegisterScanProgressCompleted(OnScanProgressCompleted);
            SortOption = "name";
        }

        private void LoadCommands()
        {
            ScanDirectoryCommand = new CustomCommand(ScanDirectoryAsync);
            GetPosterCommand = new CustomCommand(GetPoster);
            OpenDetailViewCommand = new CustomCommand(OpenDetailView);
        }
        
        private void OnScanProgressStarted(object o, ProgressEventArgs e)
        {
            MoviesProcessed = false;
            Movies.Clear();
            ScanActive = true;
        }

        private void OnScanProgressChanged(object o, ProgressEventArgs e)
        {
            ScanProgress = (int)e.PercentFinished;
        }

        private void OnScanProgressCompleted(object o, ProgressEventArgs e)
        {
            ScanActive = false;
            RefreshMovieList();
            MoviesProcessed = true;
        }

        private async void ScanDirectoryAsync(object obj)
        {
            await dataService.ScanAllMoviesAsync();
        }

        private void GetPoster(object obj)
        {
            var movie = obj as Movie;
            throw new NotImplementedException("Get Poster noch nicht implementiert!");
        }

        private void OpenDetailView(object obj)
        {
            dialogService.OpenDetailView();
        }

        private void RefreshMovieList()
        {
            Movies = dataService.GetProcessedMovies(SortOption, SortOrder, Filter);
        }



        private void RaisePropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
