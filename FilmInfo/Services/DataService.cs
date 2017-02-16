using FilmInfo.Exceptions;
using FilmInfo.Model;
using FilmInfo.Utility;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using TMDbLib.Objects.Search;
using System.Windows.Media.Imaging;
using FilmInfo.Utility.Enums;
using FilmInfo.Extensions;
using System.IO;

namespace FilmInfo.Services
{
    public class DataService
    {
        private FilmRepository filmRepository;
        private TMDbWrapper tmdbWrapper;

        public DataService()
        {
            filmRepository = new FilmRepository();
            tmdbWrapper = new TMDbWrapper();
        }

        public string GetNewRootDirectory()
        {
            return FileOperations.GetDirectory();
        }

        public async Task<bool> ScanAllMoviesAsync(IProgress<int> progress, string directory)
        {
            return await filmRepository.ScanAllMovies(directory, progress);
        }

        public ObservableCollection<Movie> GetProcessedMovies(string sortType, SortOrder sortOrder, string filter)
        {
            return filmRepository.GetProcessedMovies(sortType, sortOrder, filter).ToObservableCollection();
        }

        public async Task<Movie> GetDetailsFromTMDbAsync(Movie movie)
        {
            try
            {
                var tmdbSearchResult = await tmdbWrapper.SearchMovieAsync(movie);

                //TODO: Fill Details into movie
                movie.Poster = tmdbWrapper.GetPosterFromTMDb(tmdbSearchResult.PosterPath, "w500");
                movie.Description = tmdbSearchResult.Overview;

                return movie;
            }
            catch (MovieNotFoundException ex)
            {
                MessageBox.Show(ex.Message);
                return movie;
            }

        }

        public void UpdateMovie(Movie movie, Movie movieFromTMDb)
        {
            filmRepository.UpdateMovie(movie, movieFromTMDb);
        }
    }
}
