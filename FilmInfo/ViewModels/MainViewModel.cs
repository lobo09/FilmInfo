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
using System.Diagnostics;

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
        private string sortOption;
        private SortOrder sortOrder;
        private string filter;
        private int? filterFskMin;
        private int? filterFskMax;
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
        public ICommand GetDetailFromTMDbCommand { get; set; }
        public ICommand GetMissingDetailFromTMDbCommand { get; set; }
        public ICommand GetAllDetailFromTMDbCommand { get; set; }
        public ICommand OpenDetailViewCommand { get; set; }
        public ICommand SelectCommand { get; set; }

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
                if (value != null && value != selectedMovie)
                {
                    selectedMovie = value;
                    RaisePropertyChanged("SelectedMovie");
                    Debug.WriteLine($"SelectedMovie: {value.Name}");
                }
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

        public string SortOption
        {
            get { return sortOption; }
            set
            {
                sortOption = value;
                RaisePropertyChanged("SortOption");
                SyncMovieList();
            }
        }

        public SortOrder SortOrder
        {
            get { return sortOrder; }
            set
            {
                sortOrder = value;
                RaisePropertyChanged("SortOrder");
                SyncMovieList();
            }
        }

        public string Filter
        {
            get { return filter; }
            set
            {
                filter = value;
                RaisePropertyChanged("Filter");
                SyncMovieList();
            }
        }


        public int? FilterFskMin
        {
            get { return filterFskMin; }
            set
            {
                filterFskMin = value;
                RaisePropertyChanged("FilterFskMin");
                SyncMovieList();
            }
        }

        public int? FilterFskMax
        {
            get { return filterFskMax; }
            set
            {
                filterFskMax = value;
                RaisePropertyChanged("FilterFskMax");
                SyncMovieList();
            }
        }
        #endregion

        #region Commands
        private void LoadCommands()
        {
            ScanDirectoryCommand = new CustomCommand(ScanDirectoryAsync);
            GetDetailFromTMDbCommand = new CustomCommandAsync(GetDetailFromTMDbAsync);
            GetMissingDetailFromTMDbCommand = new CustomCommandAsync(GetMissingDetailFromTMDbAsync);
            GetAllDetailFromTMDbCommand = new CustomCommandAsync(GetAllDetailFromTMDbAsync);
            OpenDetailViewCommand = new CustomCommand(OpenDetailView);
            SelectCommand = new CustomCommand(OnSelect);
        }


        private async void ScanDirectoryAsync(object obj)
        {
            var rootDirectory = dataService.GetNewRootDirectory();
            if (rootDirectory != "")
            {
                ProgressbarVisibility = Visibility.Visible;
                EnableSidePanel = false;
                await dataService.ScanAllMoviesAsync(progressBar, rootDirectory);
                SyncMovieList();
                ProgressbarVisibility = Visibility.Collapsed;
                EnableSidePanel = true;
            }
        }

        private async Task GetDetailFromTMDbAsync(object obj)
        {
            var movie = obj as Movie;
            if (movie != null)
            {
                //TODO: Scanbar im Bild anzeigen
                await dataService.GetDetailsFromTMDbAsync(movie as Movie);
                SyncMovieList();
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
                await dataService.GetDetailsFromTMDbAsync(movie as Movie);
                ScanProgress = movieCount++ * 100 / MoviesWithMissingDetails.Count;
            }
            SyncMovieList();
            ProgressbarVisibility = Visibility.Collapsed;
        }

        private async Task GetAllDetailFromTMDbAsync(object arg)
        {
            ScanProgress = 0;
            var movieCount = 0;
            ProgressbarVisibility = Visibility.Visible;
            foreach (var movie in Movies)
            {
                await dataService.GetDetailsFromTMDbAsync(movie as Movie);
                ScanProgress = movieCount++ * 100 / Movies.Count;
            }
            SyncMovieList();
            ProgressbarVisibility = Visibility.Collapsed;
        }

        private void OpenDetailView(object obj)
        {
            Messenger.Default.Send<Movie>(obj as Movie);
            Debug.WriteLine($"OpenDetailsView: {SelectedMovie.Name}");
            dialogService.OpenDetailView();
        }

        private void OnSelect(object obj)
        {
            var movie = obj as Movie;
            dataService.ChangeSelectionOnMovie(movie);
            SyncMovieList();
        }
        #endregion

        private void OnProgressChanged(int progress)
        {
            ScanProgress = progress;
        }

        private void SyncMovieList()
        {
            Movies = dataService.GetProcessedMovies(SortOption, SortOrder, Filter, FilterFskMin, FilterFskMax);
        }

        private void RaisePropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
