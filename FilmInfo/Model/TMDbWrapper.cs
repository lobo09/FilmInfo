﻿using FilmInfo.Exceptions;
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
using FilmInfo.Utility.Enums;

namespace FilmInfo.Model
{


    public class TMDbWrapper
    {
        private TMDbClient tmdbClient = new TMDbClient("d64071ba861f30df005b0c9e0ff9b67e");

        public async Task<SearchMovie> SearchMovieAsync(Movie movie)
        {
            SearchContainer<SearchMovie> movieSearchContainer = await tmdbClient.SearchMovieAsync(movie.Name, "de", 0, true).ConfigureAwait(false);

            SearchMovie bestMatch;
            if (movieSearchContainer.TotalResults != 0)
            {
                bestMatch = FindBestMatch(movie, movieSearchContainer.Results);
            }
            else
            {
                var shortMovieTitle = "";

                if (movie.Name.Contains("-"))
                {
                    shortMovieTitle = Regex.Match(movie.Name, "^[^-]+").Value;
                }
                else
                {
                    var splittedTitle = movie.Name.Split(' ');
                    if (splittedTitle.Length > 2)
                    {
                        shortMovieTitle = $"{splittedTitle[0]} {splittedTitle[1]}";
                    }
                    else if (splittedTitle.Length == 2)
                    {
                        shortMovieTitle = $"{splittedTitle[0]}";
                    }
                }

                movieSearchContainer = await tmdbClient.SearchMovieAsync(shortMovieTitle, "de", 0, true).ConfigureAwait(false);

                if (movieSearchContainer.TotalResults != 0)
                {
                    bestMatch = FindBestMatch(movie, movieSearchContainer.Results);
                }
                else
                {
                    throw new MovieNotFoundException(movie);
                }
            }
            return bestMatch;
        }

        public BitmapImage GetPosterFromTMDb(string posterPath, string resolution)
        {
            var uri = new Uri("https://image.tmdb.org/t/p/" + resolution + posterPath);
            return new BitmapImage(uri);
        }

        private SearchMovie FindBestMatch(Movie movie, List<SearchMovie> searchResults)
        {
            const double TITLE_WEIGHT = 0.70;
            const double YEAR_WEIGHT = 0.30;

            var results = new Dictionary<MatchKey, SearchMovie>();

            foreach (var result in searchResults)
            {
                var titleMatch = StringMatchInPercent(movie.Name, result.Title, MatchCriteria.Short);
                double yearMatch;

                try
                {
                    yearMatch = YearMatchInPercent(movie.Year, result.ReleaseDate.Value.Year);
                }
                catch(InvalidOperationException)
                {
                    yearMatch = 0;
                }

                var totalMatch = (titleMatch * TITLE_WEIGHT) + (yearMatch * YEAR_WEIGHT);

                results.Add(new MatchKey(totalMatch, result.Popularity), result);
            }

            var bestMatch = results.OrderByDescending(m => m.Key.Match).ThenByDescending(m => m.Key.Popularity).Select(m => m.Value).First();

            return bestMatch;
        }

        private double YearMatchInPercent(int year1, int year2)
        {
            if (year1 != 0 && year2 != 0)
            {
                if (year1 == year2)
                {
                    return 1;
                }
                else if (year1 == year2 + 1 || year1 == year2 - 1)
                {
                    return 0.75;
                }
                else if (year1 == year2 + 2 || year1 == year2 - 2)
                {
                    return 0.1;
                }
                else
                {
                    return 0;
                }
            }
            return 0;
        }

        private double StringMatchInPercent(string string1, string string2, MatchCriteria matchType)
        {
            var list1 = Regex.Matches(string1, @"\w+").Cast<Match>().Select(m => m.Value).ToList();
            var list2 = Regex.Matches(string2, @"\w+").Cast<Match>().Select(m => m.Value).ToList();

            List<string> bigList;
            List<string> smallList;
            double matchSum = 0;

            if (list1.Count >= list2.Count)
            {
                bigList = list1;
                smallList = list2;
            }
            else
            {
                bigList = list2;
                smallList = list1;
            }

            foreach (var smallStr in smallList)
            {
                double wordMatch = 0;

                foreach (var bigStr in bigList)
                {
                    wordMatch = Math.Max(WordMatchInPercent(smallStr, bigStr), wordMatch);
                }

                matchSum += wordMatch;
            }

            if (matchType == MatchCriteria.Long)
            {
                return matchSum / bigList.Count;
            }
            else
            {
                return matchSum / smallList.Count;
            }
        }


        private double WordMatchInPercent(string str1, string str2)
        {
            char[] bigCharArray;
            char[] smallCharArray;
            int matchCount = 0;

            if (str1.Length >= str2.Length)
            {
                bigCharArray = str1.ToLower().ToArray();
                smallCharArray = str2.ToLower().ToArray();
            }
            else
            {
                bigCharArray = str2.ToLower().ToArray();
                smallCharArray = str1.ToLower().ToArray();
            }

            for (int i = 0; i < smallCharArray.Length; i++)
            {
                if (smallCharArray[i] == bigCharArray[i])
                    matchCount++;
            }

            return (double)matchCount / bigCharArray.Length;
        }

        private class MatchKey
        {
            public double Match { get; private set; }
            public double Popularity { get; private set; }

            public MatchKey(double match, double popularity)
            {
                Match = match;
                Popularity = popularity;
            }
        }
    }
}
