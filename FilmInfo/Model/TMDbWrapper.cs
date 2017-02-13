using FilmInfo.Exceptions;
using FilmInfo.Utility;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TMDbLib.Client;
using TMDbLib.Objects.General;
using TMDbLib.Objects.Search;
using System.Windows.Media.Imaging;
using System.Text.RegularExpressions;

namespace FilmInfo.Model
{
    public class TMDbWrapper
    {
        private TMDbClient tmdbClient = new TMDbClient("d64071ba861f30df005b0c9e0ff9b67e");

        public SearchMovie SearchMovie(Movie movie)
        {
            var movieTitle = movie.Name;
            var movieYear = movie.Year;

            SearchContainer<SearchMovie> movies = tmdbClient.SearchMovieAsync(movieTitle, "de", 0, true, movieYear).Result;


            if (movies.TotalResults == 0)
            {
                throw new MovieNotFoundException(movie);
            }
            else
            {
                var exactMatchList = new List<SearchMovie>();
                foreach (var item in movies.Results)
                {
                    var itemTitle = Regex.Replace(item.Title, ":", "");
                    var itemYear = item.ReleaseDate.Value.Year;

                    if (itemTitle.Equals(movieTitle, StringComparison.OrdinalIgnoreCase)
                        && itemYear == movieYear)
                    {
                        exactMatchList.Add(item);
                    }
                }
                if (exactMatchList.Count == 1)
                    return exactMatchList.First();
                else
                    return ChooseMovie(movies);
            }
        }

        public BitmapImage GetPosterFromTMDb(string posterPath, string resolution)
        {
            var uri = new Uri("https://image.tmdb.org/t/p/" + resolution + posterPath);
            return new BitmapImage(uri);
        }

        private SearchMovie ChooseMovie(SearchContainer<SearchMovie> movies)
        {
            throw new NotImplementedException();
        }

    }
}
