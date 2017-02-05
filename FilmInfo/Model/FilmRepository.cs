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
        public event EventHandler<ProgressEventArgs> ScanProgressChanged;

        public FilmRepository()
        {
            FilmDatabase = new List<Movie>();
        }

        public bool ScanAllMovies(string rootDirectory)
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
                ScanProgressChanged(this, new ProgressEventArgs(actualDir++,maxDir));
            }
            FilmDatabase.RemoveAll(m => m.MkvFile == null);
            return FilmDatabase.Count != 0 ? true : false;
        }

        public List<Movie> GetProcessedMovies(string sortType, string filter)
        {
            var processedMovieList = FilmDatabase;

            if (!string.IsNullOrEmpty(filter))
            {
                processedMovieList = FilterMovies(processedMovieList, filter);
            }

            if (!string.IsNullOrEmpty(sortType))
            {
                processedMovieList = SortMovies(processedMovieList, sortType);
            }
            return processedMovieList;
        }

        private List<Movie> FilterMovies(List<Movie> movieList, string filter)
        {
            if (!string.IsNullOrEmpty(filter))
                movieList = movieList.Where(m => m.Name.ToLower().Contains(filter.ToLower())).ToList();

            return movieList;
        }

        private List<Movie> SortMovies(List<Movie> movieList, string sortType)
        {
            switch (sortType)
            {
                case "name":
                    movieList = movieList.OrderBy(m => m.Name).ToList();
                    break;

                case "year":
                    movieList = movieList.OrderByDescending(m => m.Year).ToList();
                    break;

                case "newest":
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
            movie.Name = directory.Name;

            string Year = Regex.Match(directory.Name, @"\(\d{4}\)").Value;
            if (Year.Count() == 6)
                movie.Year = int.Parse(Year.Remove(0, 1).Remove(4, 1));

            if (movie.MkvFileFull != null) movie.MkvCreationTime = File.GetCreationTime(movie.MkvFileFull);

            if (movie.PosterFileFull != null)
                movie.Poster = LoadBitmapImage(movie.PosterFileFull);
            return movie;
        }

        private BitmapImage LoadBitmapImage(string path)
        {
            BitmapImage bitmapImage = new BitmapImage();
            bitmapImage.BeginInit();
            bitmapImage.UriSource = new Uri(path, UriKind.RelativeOrAbsolute);
            bitmapImage.EndInit();
            bitmapImage.Freeze();
            return bitmapImage;
        }
    }
}
