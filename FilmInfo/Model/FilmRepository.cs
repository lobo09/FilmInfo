using FilmInfo.Properties;
using FilmInfo.Utility;
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
        public event EventHandler<ProgressEventArgs> ScanProgressStarted;
        public event EventHandler<ProgressEventArgs> ScanProgressChanged;
        public event EventHandler<ProgressEventArgs> ScanProgressCompleted;

        public FilmRepository()
        {
            FilmDatabase = new List<Movie>();
        }

        public async Task<bool> ScanAllMovies(string rootDirectory)
        {
            ScanProgressStarted(this, new ProgressEventArgs());

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
                        ScanProgressChanged(this, new ProgressEventArgs(actualDir++, maxDir));
                    }
                    FilmDatabase.RemoveAll(m => m.MkvFile == null);
                });

            ScanProgressCompleted(this, new ProgressEventArgs());

            return FilmDatabase.Count != 0 ? true : false;
        }

        public List<Movie> GetProcessedMovies(string sortType, SortOrderEnum sortOrder, string filter)
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

        private List<Movie> FilterMovies(List<Movie> movieList, string filter)
        {
            if (!string.IsNullOrEmpty(filter))
                movieList = movieList.Where(m => m.Name.ToLower().Contains(filter.ToLower())).ToList();

            return movieList;
        }

        private List<Movie> SortMovies(List<Movie> movieList, string sortType, SortOrderEnum sortOrder)
        {
            switch (sortType)
            {
                case "name":
                    if (sortOrder == SortOrderEnum.Aufsteigend)
                        movieList = movieList.OrderBy(m => m.Name).ToList();
                    else
                        movieList = movieList.OrderByDescending(m => m.Name).ToList();
                    break;

                case "year":
                    if (sortOrder == SortOrderEnum.Aufsteigend)
                        movieList = movieList.OrderBy(m => m.Year).ToList();
                    else
                        movieList = movieList.OrderByDescending(m => m.Year).ToList();
                    break;

                case "newest":
                    if (sortOrder == SortOrderEnum.Aufsteigend)
                        movieList = movieList.OrderByDescending(m => m.MkvCreationTime).ToList();
                    else
                        movieList = movieList.OrderBy(m => m.MkvCreationTime).ToList();
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
            movie.Name = directory.Name;

            string Year = Regex.Match(directory.Name, @"\(\d{4}\)").Value;
            if (Year.Count() == 6)
                movie.Year = int.Parse(Year.Remove(0, 1).Remove(4, 1));

            if (movie.MkvFileFull != null) movie.MkvCreationTime = File.GetCreationTime(movie.MkvFileFull);

            if (movie.PosterFileFull != null)
                movie.Poster = FileOperations.LoadBitmapImage(movie.PosterFileFull);
            return movie;
        }


    }
}
