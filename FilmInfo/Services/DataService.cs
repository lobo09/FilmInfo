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

        public string RootDirectory { private set; get; }

        public DataService()
        {
            filmRepository = new FilmRepository();
            fileOperations = new FileOperations();
        }

        public void SetRootDirectory()
        {
            RootDirectory = fileOperations.GetRootDirectory();
        }

        public List<Movie> GetAllMovies()
        {
            return filmRepository.GetAllMovies(RootDirectory);
        }
    }
}
