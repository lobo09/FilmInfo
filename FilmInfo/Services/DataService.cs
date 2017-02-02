using FilmInfo.Model;
using System;
using System.Collections.Generic;
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

        public List<Movie> GetAllMovies()
        {
            return filmRepository.FilmDatabase;
        }

        public bool ScanAllMovies()
        {
            var rootDirectory = fileOperations.GetRootDirectory();
            return filmRepository.ScanAllMovies(rootDirectory);
        }
    }
}
