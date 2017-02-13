﻿using FilmInfo.Exceptions;
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

namespace FilmInfo.Services
{
    public class DataService
    {
        private FilmRepository filmRepository;
        private TMDbWrapper tmdbWrapper;

        public DataService()
        {
            filmRepository = new FilmRepository();
            tmdbWrapper = new TMDbWrapper();
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

        public Movie GetDetailsFromTMDb(Movie movie)
        {
            try
            {
                var tmdbSearchResult = tmdbWrapper.SearchMovie(movie);

                //TODO: Fill Details into movie
                movie.Poster = tmdbWrapper.GetPosterFromTMDb(tmdbSearchResult.PosterPath, "w500");

                return movie;
            }
            catch (MovieNotFoundException ex)
            {
                MessageBox.Show(ex.Message);
                return movie;
            }

        }

        public void UpdateMovie(Movie movie, Movie movieFromTMDb)
        {
            filmRepository.UpdateMovie(movie, movieFromTMDb);
        }
    }
}
