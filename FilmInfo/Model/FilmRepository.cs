using FilmInfo.Exceptions;
using FilmInfo.Properties;
using FilmInfo.Utility;
using FilmInfo.Utility.Enums;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media.Imaging;
using System.Windows.Threading;

namespace FilmInfo.Model
{
    public class FilmRepository
    {
        public List<Movie> FilmDatabase { get; set; }
        public string RootDirectory { get; private set; }
        private TMDbWrapper tmdbWrapper;

        public FilmRepository()
        {
            FilmDatabase = new List<Movie>();
            tmdbWrapper = new TMDbWrapper();
        }

        public async Task<bool> ScanAllMovies(string rootDirectory, IProgress<int> progress)
        {
            await Task.Run(() =>
                {
                    FilmDatabase.Clear();
                    RootDirectory = rootDirectory;
                    var directorys = new DirectoryInfo(RootDirectory).EnumerateDirectories();
                    int actualDir = 1;
                    int maxDir = directorys.Count();

                    foreach (var directory in directorys)
                    {
                        var movie = SetAllFilesInMovie(directory);
                        movie = SetAllFieldsInMovie(directory, movie);
                        FilmDatabase.Add(movie);
                        progress.Report(actualDir++ * 100 / maxDir);
                    }
                    FilmDatabase.RemoveAll(m => m.MkvFile == null);
                });

            return FilmDatabase.Count != 0 ? true : false;
        }

        public async Task GetDetailsFromTMDbAsync(Movie movie)
        {
            var index = FilmDatabase.IndexOf(movie);
            if(index != -1)
            {
                var movieInDB = FilmDatabase[index];
            try
                {
                    var tmdbSearchResult = await tmdbWrapper.SearchMovieAsync(movie);

                    //TODO: Fill Details into movie
                    movieInDB.Poster = tmdbWrapper.GetPosterFromTMDb(tmdbSearchResult.PosterPath, "w500");
                    movieInDB.Description = tmdbSearchResult.Overview;
                    movieInDB.ReleaseDate = tmdbSearchResult.ReleaseDate.Value;
                    movieInDB.OriginalTitle = tmdbSearchResult.OriginalTitle;
                    movieInDB.Runtime = tmdbSearchResult.Runtime.Value;
                    movieInDB.Genres = new List<string>();
                    foreach(var genre in tmdbSearchResult.Genres)
                    {
                        movieInDB.Genres.Add(genre.Name);
                    }
                    
                }
                catch (MovieNotFoundException ex)
                {
                    MessageBox.Show(ex.Message);
                }
            }
        }

        public List<Movie> GetProcessedMovies(string sortType, SortOrder sortOrder, string filter)
        {
            var processedMovieList = FilmDatabase;

            if (!string.IsNullOrEmpty(filter))
            {
                processedMovieList = FilterMovies(processedMovieList, filter);
            }

            if (!string.IsNullOrEmpty(sortType))
            {
                processedMovieList = SortMovies(processedMovieList, sortType, sortOrder);
            }
            return processedMovieList;
        }

        public void ToggleSelectionOnMovie(Movie movie)
        {
            var selectedMovie = FilmDatabase.Where(m => m == movie).First();

            if (selectedMovie.isSelected == Visibility.Collapsed)
            {
                selectedMovie.isSelected = Visibility.Visible;
            }
            else
            {
                selectedMovie.isSelected = Visibility.Collapsed;
            }
        }

        public void UpdateMovie(Movie movie, Movie newMovie)
        {
            var index = FilmDatabase.IndexOf(movie);
            if (index != -1)
            {
                FilmDatabase[index] = newMovie;
            }
        }

        private List<Movie> FilterMovies(List<Movie> movieList, string filter)
        {
            if (!string.IsNullOrEmpty(filter))
                movieList = movieList.Where(m => m.Name.ToLower().Contains(filter.ToLower())).ToList();

            return movieList;
        }

        private List<Movie> SortMovies(List<Movie> movieList, string sortType, SortOrder sortOrder)
        {
            switch (sortType)
            {
                case "name":
                    if (sortOrder == SortOrder.Aufsteigend)
                        movieList = movieList.OrderBy(m => m.Name).ToList();
                    else
                        movieList = movieList.OrderByDescending(m => m.Name).ToList();
                    break;

                case "year":
                    if (sortOrder == SortOrder.Aufsteigend)
                        movieList = movieList.OrderBy(m => m.Year).ToList();
                    else
                        movieList = movieList.OrderByDescending(m => m.Year).ToList();
                    break;

                case "newest":
                    if (sortOrder == SortOrder.Aufsteigend)
                        movieList = movieList.OrderBy(m => m.MkvCreationTime).ToList();
                    else
                        movieList = movieList.OrderByDescending(m => m.MkvCreationTime).ToList();
                    break;
            }
            return movieList;
        }

        private Movie SetAllFilesInMovie(DirectoryInfo directory)
        {
            var movie = new Movie();
            List<FileInfo> files = new List<FileInfo>(directory.EnumerateFiles().ToList());

            if (files.Any(f => f.Name.ToLower().Contains(".mkv")))
            {
                movie.MkvFile = files.Where(f => f.Name.ToLower().Contains(".mkv")).First().Name;
                movie.MkvFileFull = $@"{directory.FullName}\{movie.MkvFile}";
            }

            if (files.Any(f => f.Name.ToLower().Contains(".nfo")))
            {
                movie.NfoFile = files.Where(f => f.Name.ToLower().Contains(".nfo")).First().Name;
                movie.NfoFileFull = $@"{directory.FullName}\{movie.NfoFile}";
            }

            if (files.Any(f => f.Name.ToLower().Contains("poster.jpg") || f.Name.ToLower().Contains("folder.jpg")))
            {
                movie.PosterFile = files.Where(f => f.Name.ToLower().Contains("poster.jpg") || f.Name.ToLower().Contains("folder.jpg")).First().Name;
                movie.PosterFileFull = $@"{directory.FullName}\{movie.PosterFile}";
            }
            else
            {
                movie.PosterFile = "NoImage.jpg";
                movie.PosterFileFull = @"pack://application:,,,/Resources/Images/NoImage.jpg";
            }
            return movie;
        }

        private Movie SetAllFieldsInMovie(DirectoryInfo directory, Movie movie)
        {
            var Year = Regex.Match(directory.Name, @"\(\d{4}\)").Value;

            if (Year.Count() == 6)
            {
                movie.Name = directory.Name.Remove(directory.Name.Length - 7); ;
                movie.Year = int.Parse(Year.Remove(0, 1).Remove(4, 1));
            }
            else
            {
                movie.Name = directory.Name;
            }

            if (movie.MkvFileFull != null) movie.MkvCreationTime = File.GetCreationTime(movie.MkvFileFull);

            if (movie.PosterFileFull != null)
                movie.Poster = FileOperations.LoadBitmapImage(movie.PosterFileFull);

            movie.isSelected = Visibility.Collapsed;
            return movie;
        }


    }
}
