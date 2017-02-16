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
        #region Fields
        public event PropertyChangedEventHandler PropertyChanged;
        private int scanProgress;
        private Movie selectedMovie;
        private string movieCountLabel;
        private IProgress<int> progressBar;
        private DataService dataService;
        private DialogService dialogService;
        private ObservableCollection<Movie> movies;
        private bool enableSidePanel;
        private string filter;
        private string sortOption;
        private SortOrder sortOrder;
        #endregion

        public MainViewModel()
        {
            LoadCommands();
            Movies = new ObservableCollection<Movie>();
            progressBar = new Progress<int>(OnProgressChanged);
            dataService = new DataService();
            dialogService = new DialogService();
            SortOption = "name";
            ProgressbarVisibility = Visibility.Collapsed;
        }

        #region Properties
        public ICommand ScanDirectoryCommand { get; set; }
        //TODO: GetPoster entfernen
        public ICommand GetPosterCommand { get; set; }
        public ICommand GetDetailFromTMDbCommand { get; set; }
        public ICommand GetMissingDetailFromTMDbCommand { get; set; }
        public ICommand GetAllDetailFromTMDbCommand { get; set; }
        public ICommand OpenDetailViewCommand { get; set; }

        public int ScanProgress
        {
            get { return scanProgress; }
            set
            {
                scanProgress = value;
                RaisePropertyChanged("ScanProgress");
            }
        }

        private Visibility progressbarVisibility;
        public Visibility ProgressbarVisibility
        {
            get { return progressbarVisibility; }
            set
            {
                progressbarVisibility = value;
                RaisePropertyChanged("ProgressbarVisibility");
            }
        }

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

        public string MovieCountLabel
        {
            get { return movieCountLabel; }
            set
            {
                movieCountLabel = value;
                RaisePropertyChanged("MovieCountLabel");
            }
        }

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

        public bool EnableSidePanel
        {
            get { return enableSidePanel; }
            set
            {
                enableSidePanel = value;
                RaisePropertyChanged("EnableSidePanel");
            }
        }

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

        #region Commands
        private void LoadCommands()
        {
            ScanDirectoryCommand = new CustomCommand(ScanDirectoryAsync);
            GetPosterCommand = new CustomCommand(GetPoster);
            GetDetailFromTMDbCommand = new CustomCommandAsync(GetDetailFromTMDbAsync);
            GetMissingDetailFromTMDbCommand = new CustomCommandAsync(GetMissingDetailFromTMDbAsync);
            GetAllDetailFromTMDbCommand = new CustomCommandAsync(GetAllDetailFromTMDbAsync);
            OpenDetailViewCommand = new CustomCommand(OpenDetailView);
        }


        private async void ScanDirectoryAsync(object obj)
        {
            try
            {
                var rootDirectory = dataService.GetNewRootDirectory();
                ProgressbarVisibility = Visibility.Visible;
                EnableSidePanel = false;
                await dataService.ScanAllMoviesAsync(progressBar, rootDirectory);
                RefreshMovieList();
                ProgressbarVisibility = Visibility.Collapsed;
                EnableSidePanel = true;
            }
            catch (DirectoryNotFoundException)
            {

            }
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
                //TODO: Scanbar im Bild anzeigen
                var movieFromTMDb = await dataService.GetDetailsFromTMDbAsync(movie as Movie);
                dataService.UpdateMovie(movie, movieFromTMDb);
                RefreshMovieList();
            }
        }

        private async Task GetMissingDetailFromTMDbAsync(object arg)
        {
            ScanProgress = 0;
            var movieCount = 0;
            ProgressbarVisibility = Visibility.Visible;
            var MoviesWithMissingDetails = Movies.Where(m => m.NfoFile == null || m.PosterFile == "NoImage.jpg" || m.PosterFile == null).ToList();
            foreach (var movie in MoviesWithMissingDetails)
            {
                var movieFromTMDb = await dataService.GetDetailsFromTMDbAsync(movie as Movie);
                dataService.UpdateMovie(movie, movieFromTMDb);
                ScanProgress = movieCount++ * 100 / MoviesWithMissingDetails.Count;
            }
            RefreshMovieList();
            ProgressbarVisibility = Visibility.Collapsed;
        }

        private async Task GetAllDetailFromTMDbAsync(object arg)
        {
            ScanProgress = 0;
            var movieCount = 0;
            ProgressbarVisibility = Visibility.Visible;
            foreach (var movie in Movies)
            {
                var movieFromTMDb = await dataService.GetDetailsFromTMDbAsync(movie as Movie);
                dataService.UpdateMovie(movie, movieFromTMDb);
                ScanProgress = movieCount++ * 100 / Movies.Count;
            }
            RefreshMovieList();
            ProgressbarVisibility = Visibility.Collapsed;
        }

        private void OpenDetailView(object obj)
        {
            Messenger.Default.Send<Movie>(SelectedMovie);
            dialogService.OpenDetailView();
        }
        #endregion

        private void OnProgressChanged(int progress)
        {
            ScanProgress = progress;
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
