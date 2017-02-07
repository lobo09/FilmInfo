using FilmInfo.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FilmInfo.Model;
using FilmInfo.Views;

namespace FilmInfo
{
    public class ViewModelLocator
    {
        private static MainViewModel mainViewModel = new MainViewModel();
        private static DetailViewModel detailViewModel = new DetailViewModel();
        private static DetailView detailView = new DetailView();

        public static MainViewModel MainViewModel
        {
            get
            {
                return mainViewModel;
            }
        }

        public static DetailViewModel DetailViewModel
        {
            get
            {
                return detailViewModel;
            }
        }

        public static void OpenDetailView(Movie movie)
        {
            //detailView = new DetailView();
            detailView.Show();
        }
    }
}
