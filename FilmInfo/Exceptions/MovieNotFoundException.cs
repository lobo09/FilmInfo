using FilmInfo.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FilmInfo.Exceptions   
{
    public class MovieNotFoundException : Exception
    {
        public MovieNotFoundException(Movie movie) : base($"\"{movie.Name}\" of Year {movie.Year} not found")
        {
        }
    }
}
