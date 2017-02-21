using FilmInfo.Exceptions;
using FilmInfo.Properties;
using FilmInfo.Utility;
using FilmInfo.Utility.Enums;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
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
            if (index != -1)
            {
                var movieInDB = FilmDatabase[index];
                try
                {

                    var tmdbSearchResult = await tmdbWrapper.SearchMovieAsync(movie);
                    var fskTask = Altersfreigaben.getFskAsync(tmdbSearchResult.Id);
                    var tmdbDetails = await tmdbWrapper.GetMovieDetailsAsync(tmdbSearchResult.Id);

                    //TODO: Fill Details into movie
                    movieInDB.Poster = tmdbWrapper.GetPosterFromTMDb(tmdbDetails.PosterPath, "w500");
                    movieInDB.Description = tmdbDetails.Overview;
                    movieInDB.OriginalTitle = tmdbDetails.OriginalTitle;
                    movieInDB.Runtime = tmdbDetails.Runtime.Value;
                    movieInDB.Genres = new List<string>();
                    foreach (var genre in tmdbDetails.Genres)
                    {
                        movieInDB.Genres.Add(genre.Name);
                    }
                    movieInDB.Rating = tmdbDetails.VoteAverage;
                    movieInDB.RatingCount = tmdbDetails.VoteCount;

                    var releaseDateItem = tmdbDetails
                                                .ReleaseDates.Results
                                                .Where(r => r.Iso_3166_1 == "DE")
                                                .SelectMany(r => r.ReleaseDates)
                                                .FirstOrDefault();
                    if (releaseDateItem != null)
                    {
                        movie.ReleaseDate = releaseDateItem.ReleaseDate;
                    }
                    if (releaseDateItem != null && releaseDateItem.Certification != "" && int.TryParse(releaseDateItem.Certification, out int fsk))
                    {
                        movieInDB.Fsk = fsk;
                    }
                    else
                    {
                        movieInDB.Fsk = await fskTask;
                    }
                }
                catch (MovieNotFoundException ex)
                {
                    MessageBox.Show(ex.Message);
                }
            }
        }

        public List<Movie> GetProcessedMovies(string sortType, SortOrder sortOrder, string filter, int? fskMin, int? fskMax)
        {
            var processedMovieList = FilmDatabase;

            if (!string.IsNullOrEmpty(filter) || fskMin != null || fskMax != null)
            {
                processedMovieList = FilterMovies(processedMovieList, filter, fskMin, fskMax);
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

        private List<Movie> FilterMovies(List<Movie> movieList, string filter, int? fskMin, int? fskMax)
        {
            IEnumerable<Movie> filteredList;
            filteredList = movieList;

            if (!string.IsNullOrEmpty(filter))
                filteredList = filteredList.Where(m => m.Name.ToLower().Contains(filter.ToLower()));

            if (fskMin != null || fskMax != null)
            {
                fskMin = fskMin ?? 0;
                fskMax = fskMax ?? 18;
                filteredList = filteredList.Where(m => m.Fsk == -1 || (m.Fsk >= fskMin && m.Fsk <= fskMax));
            }
            return filteredList.ToList();
        }

        private List<Movie> SortMovies(List<Movie> movieList, string sortType, SortOrder sortOrder)
        {
            IEnumerable<Movie> orderedList = movieList;
            switch (sortType)
            {
                case "name":
                    if (sortOrder == SortOrder.Aufsteigend)
                        orderedList = orderedList.OrderBy(m => m.Name);
                    else
                        orderedList = orderedList.OrderByDescending(m => m.Name);
                    break;

                case "year":
                    if (sortOrder == SortOrder.Aufsteigend)
                        orderedList = orderedList.OrderBy(m => m.Year);
                    else
                        orderedList = orderedList.OrderByDescending(m => m.Year);
                    break;

                case "newest":
                    if (sortOrder == SortOrder.Aufsteigend)
                        orderedList = orderedList.OrderBy(m => m.MkvCreationTime);
                    else
                        orderedList = orderedList.OrderByDescending(m => m.MkvCreationTime);
                    break;

                case "rating":
                    if (sortOrder == SortOrder.Aufsteigend)
                        orderedList = orderedList.OrderBy(m => m.Rating);
                    else
                        orderedList = orderedList.OrderByDescending(m => m.Rating);
                    break;

                case "fsk":
                    if (sortOrder == SortOrder.Aufsteigend)
                        orderedList = orderedList.OrderBy(m => m.Fsk);
                    else
                        orderedList = orderedList.OrderByDescending(m => m.Fsk);
                    break;
            }
            return orderedList.ToList();
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
