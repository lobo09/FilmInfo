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

        public DataService()
        {
            filmRepository = new FilmRepository();
        }

        public void RegisterScanProgressStarted(EventHandler<ProgressEventArgs> processEventHandler)
        {
            filmRepository.ScanProgressStarted += processEventHandler;
        }

        public void RegisterScanProgressChanged(EventHandler<ProgressEventArgs> processEventHandler)
        {
            filmRepository.ScanProgressChanged += processEventHandler;
        }

        public void RegisterScanProgressCompleted(EventHandler<ProgressEventArgs> processEventHandler)
        {
            filmRepository.ScanProgressCompleted += processEventHandler;
        }

        public async Task<bool> ScanAllMoviesAsync()
        {
            string rootDirectory = FileOperations.GetDirectory();
            if (rootDirectory != null)
            {
                var result = await filmRepository.ScanAllMovies(rootDirectory);
                return result;
            }
            else
                return false;
        }

        public ObservableCollection<Movie> GetProcessedMovies(string sortType, SortOrderEnum sortOrder, string filter)
        {
            return filmRepository.GetProcessedMovies(sortType, sortOrder, filter).ToObservableCollection();
        }
    }
}
