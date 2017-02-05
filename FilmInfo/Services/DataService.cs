using FilmInfo.Model;
using FilmInfo.Utility;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FilmInfo.Services
{
    public class DataService
    {
        private FilmRepository filmRepository;
        private FileOperations fileOperations;


        public DataService()
        {
            filmRepository = new FilmRepository();
            fileOperations = new FileOperations();
        }

        public ObservableCollection<Movie> GetAllMovies()
        {
            return filmRepository.FilmDatabase.ToObservableCollection();
        }

        public async Task<bool> ScanAllMoviesAsync()
        {
            var rootDirectory = fileOperations.GetRootDirectory();
            var result = Task.Run(() => filmRepository.ScanAllMovies(rootDirectory));
            return await result;
        }

        public ObservableCollection<Movie> GetProcessedMovies(string sortType, string filter)
        {
            return filmRepository.GetProcessedMovies(sortType, filter).ToObservableCollection();
        }
    }
}
