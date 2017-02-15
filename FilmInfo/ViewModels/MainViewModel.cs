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
using System.Windows.Input;
using FilmInfo.Utility.Enums;

namespace FilmInfo.ViewModels
{
    public class MainViewModel : INotifyPropertyChanged
    {
        public ICommand ScanDirectoryCommand { get; set; }
        public ICommand GetPosterCommand { get; set; }
        public ICommand GetDetailFromTMDbCommand { get; set; }
        public ICommand GetMissingDetailFromTMDbCommand { get; set; }
        public ICommand GetAllDetailFromTMDbCommand { get; set; }
        public ICommand OpenDetailViewCommand { get; set; }
        public event PropertyChangedEventHandler PropertyChanged;
        private DataService dataService;
        private DialogService dialogService;

        #region Properties
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

        private string movieCountLabel;
        public string MovieCountLabel
        {
            get { return movieCountLabel; }
            set
            {
                movieCountLabel = value;
                RaisePropertyChanged("MovieCountLabel");
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
                MovieCountLabel = $"{movies.Count} Filme";
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

        private SortOrder sortOrder;
        public SortOrder SortOrder
        {
            get { return sortOrder; }
            set
            {
                sortOrder = value;
                RaisePropertyChanged("SortOrder");
                RefreshMovieList();
            }
        }
        #endregion

        public MainViewModel()
        {
            Movies = new ObservableCollection<Movie>();
            LoadCommands();
            dataService = new DataService();
            dialogService = new DialogService();
            dataService.RegisterScanProgressStarted(OnScanProgressStarted);
            dataService.RegisterScanProgressChanged(OnScanProgressChanged);
            dataService.RegisterScanProgressCompleted(OnScanProgressCompleted);
            SortOption = "name";
        }

        #region Eventhandler
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
        #endregion

        #region Commands
        private void LoadCommands()
        {
            ScanDirectoryCommand = new CustomCommand(ScanDirectoryAsync);
            GetPosterCommand = new CustomCommand(GetPoster);
            GetDetailFromTMDbCommand = new CustomCommandAsync(GetDetailFromTMDbAsync);
            GetMissingDetailFromTMDbCommand = new CustomCommandAsync(GetAllDetailFromTMDbAsync);
            GetAllDetailFromTMDbCommand = new CustomCommandAsync(GetAllDetailFromTMDbAsync);
            OpenDetailViewCommand = new CustomCommand(OpenDetailView);
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

        private async Task GetDetailFromTMDbAsync(object obj)
        {
            var movie = obj as Movie;
            if (movie != null)
            {
                var movieFromTMDb = await dataService.GetDetailsFromTMDbAsync(movie as Movie);
                dataService.UpdateMovie(movie, movieFromTMDb);
                RefreshMovieList();
            }
        }

        private async Task GetMissingDetailFromTMDbAsync(object arg)
        {
            throw new NotImplementedException();
        }

        private async Task GetAllDetailFromTMDbAsync(object arg)
        {
            foreach (var movie in Movies)
            {
                var movieFromTMDb = await dataService.GetDetailsFromTMDbAsync(movie as Movie);
                dataService.UpdateMovie(movie, movieFromTMDb);
            }
            RefreshMovieList();
        }

        private void OpenDetailView(object obj)
        {
            Messenger.Default.Send<Movie>(SelectedMovie);
            dialogService.OpenDetailView();
        }
        #endregion

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
