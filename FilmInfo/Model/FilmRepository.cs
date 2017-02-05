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

namespace FilmInfo.Model
{
    public class FilmRepository
    {
        public List<Movie> FilmDatabase { get; set; }
        public string RootDirectory { get; private set; }

        public FilmRepository()
        {
            FilmDatabase = new List<Movie>();
        }

        public bool ScanAllMovies(string rootDirectory)
        {
            FilmDatabase.Clear();
            RootDirectory = rootDirectory;
                var directorys = new DirectoryInfo(rootDirectory).EnumerateDirectories();
                foreach (var directory in directorys)
                {
                    var movie = SetFilesInMovie(directory);
                    movie = SetFieldsInMovie(directory, movie);
                    FilmDatabase.Add(movie);
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

        private Movie SetFilesInMovie(DirectoryInfo directory)
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
                movie.PosterFileFull = @"\Resources\Images\NoImage.jpg";
            }
            return movie;
        }

        private Movie SetFieldsInMovie(DirectoryInfo directory, Movie movie)
        {
            movie.Name = directory.Name;

            string Year = Regex.Match(directory.Name, @"\(\d{4}\)").Value;
            if (Year.Count() == 6)
                movie.Year = int.Parse(Year.Remove(0, 1).Remove(4, 1));

            if (movie.MkvFile != null) movie.MkvCreationTime = File.GetCreationTime(movie.MkvFileFull);

            return movie;
        }
    }
}
