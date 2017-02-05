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

        public void RegisterProgressChanged(EventHandler<ProgressEventArgs> processEventHandler)
        {
            filmRepository.ScanProgressChanged += processEventHandler;
        }

        public async Task<bool> ScanAllMoviesAsync()
        {
            string rootDirectory = fileOperations.GetRootDirectory();
            if (rootDirectory != null)
            {
                var result = await Task.Run(() => filmRepository.ScanAllMovies(rootDirectory));
                return result;
            }
            else
                return false;
        }

        public ObservableCollection<Movie> GetProcessedMovies(string sortType, string filter)
        {
            return filmRepository.GetProcessedMovies(sortType, filter).ToObservableCollection();
        }
    }
}
