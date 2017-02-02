using System;
using System.Collections.Generic;
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
        public Dictionary<string, Movie> FilmDatabase;
        private DirectoryInfo directoryInfo;

        public FilmRepository()
        {
            FilmDatabase = new Dictionary<string, Movie>();

            AddDummys();
        }

        public List<Movie> GetAllMovies(string rootDirectory)
        {
            directoryInfo = new DirectoryInfo(rootDirectory);
            var directorys = directoryInfo.EnumerateDirectories();
            var list = new List<Movie>();

            foreach (var directory in directorys)
            {
                var movie = SetFilesInMovie(directory);
                movie = SetFieldsInMovie(directory, movie);
                list.Add(movie);
            }
            list.RemoveAll(m => m.MkvFile == null);
            list = list.OrderBy(m => m.Name).ToList();

            return list;
        }

        private Movie SetFilesInMovie(DirectoryInfo directory)
        {
            var movie = new Movie();
            List<FileInfo> files = new List<FileInfo>(directory.EnumerateFiles().ToList());

            if (files.Any(f => f.Name.Contains(".mkv")))
            {
                movie.MkvFile = files.Where(f => f.Name.Contains(".mkv")).First().Name;
                movie.MkvFileFull = $@"{directory.FullName}\{movie.MkvFile}";
            }

            if (files.Any(f => f.Name.Contains(".nfo")))
            {
                movie.NfoFile = files.Where(f => f.Name.Contains(".nfo")).First().Name;
                movie.NfoFileFull = $@"{directory.FullName}\{movie.NfoFile}";
            }

            if (files.Any(f => f.Name.Contains("poster.jpg") || f.Name.Contains("folder.jpg")))
            {
                movie.PosterFile = files.Where(f => f.Name.Contains("poster.jpg") || f.Name.Contains("folder.jpg")).First().Name;
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

            return movie;
        }

        private void AddDummys()
        {
            FilmDatabase.Add("Terminator 3", new Movie() { Name = "Terminator 3", Year = 2001 });
            FilmDatabase.Add("Snatch", new Movie() { Name = "Snatch", Year = 1998 });
            FilmDatabase.Add("Suicide Squad", new Movie() { Name = "Suicide Squad", Year = 2016 });
        }
    }
}
