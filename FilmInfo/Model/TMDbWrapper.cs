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
using FilmInfo.Services;

namespace FilmInfo.Model
{
    public class TMDbWrapper
    {
        private TMDbClient tmdbClient = new TMDbClient("d64071ba861f30df005b0c9e0ff9b67e");

        public async Task<SearchMovie> SearchMovieAsync(Movie movie)
        {
            var movieTitle = movie.Name;
            var movieYear = movie.Year;

            SearchContainer<SearchMovie> movies = await tmdbClient.SearchMovieAsync(movieTitle, "de", 0, true, movieYear).ConfigureAwait(false);


            if (movies.TotalResults == 0)
            {
                movies = await tmdbClient.SearchMovieAsync(movieTitle, "de", 0, true, movieYear + 1).ConfigureAwait(false);
                movies = await tmdbClient.SearchMovieAsync(movieTitle, "de", 0, true, movieYear + 1).ConfigureAwait(false);
            }

            if (movies.TotalResults == 0)
            {
                throw new MovieNotFoundException(movie);
            }
            else
            {
                var exactMatchList = new List<SearchMovie>();
                foreach (var item in movies.Results)
                {
                    var title = Regex.Replace(movie.Name, "[^a-zA-Z0-9]", "");
                    var itemTitle = Regex.Replace(item.Title, "[^a-zA-Z0-9]", "");
                    var itemYear = item.ReleaseDate.Value.Year;

                    if (itemTitle.Equals(title, StringComparison.OrdinalIgnoreCase))
                    {
                        if (itemYear == movie.Year || itemYear + 1 == movie.Year || itemYear - 1 == movie.Year)
                            exactMatchList.Add(item);
                    }
                }

                if (exactMatchList.Count == 1)
                {
                    return exactMatchList.First();
                }
                else if (exactMatchList.Count >= 1)
                {
                    return exactMatchList.OrderByDescending(m => m.Popularity).FirstOrDefault();
                }
                else if (movies.TotalResults == 1)
                {
                    return movies.Results.FirstOrDefault();
                }
                else
                {
                    throw new MovieNotFoundException(movie);
                }
            }
        }

        public BitmapImage GetPosterFromTMDb(string posterPath, string resolution)
        {
            var uri = new Uri("https://image.tmdb.org/t/p/" + resolution + posterPath);
            return new BitmapImage(uri);
        }
    }
}
