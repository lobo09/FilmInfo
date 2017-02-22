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

        public DataService()
        {
            filmRepository = new FilmRepository();
        }

        public string GetNewRootDirectory()
        {
            return FileOperations.GetDirectory();
        }

        public async Task<bool> ScanAllMoviesAsync(IProgress<int> progress, string directory)
        {
            return await filmRepository.ScanAllMovies(directory, progress);
        }

        public ObservableCollection<Movie> GetProcessedMovies(string sortType, SortOrder sortOrder, string filterName,string filterGenre, int? filterFskMin, int? filterFskMax, 
                                                                int? filterRatingMin, int? filterRatingMax, int? filterYearMin, int? filterYearMax)
        {
            return filmRepository.GetProcessedMovies(sortType, sortOrder, filterName, filterGenre, filterFskMin, filterFskMax, 
                                                        filterRatingMin, filterRatingMax,filterYearMin, filterYearMax).ToObservableCollection();
        }

        public async Task GetDetailsFromTMDbAsync(Movie movie)
        {
            await filmRepository.GetDetailsFromTMDbAsync(movie);
            
        }

        public void ChangeSelectionOnMovie(Movie movie)
        {
            filmRepository.ToggleSelectionOnMovie(movie);
            
        }
    }
}
