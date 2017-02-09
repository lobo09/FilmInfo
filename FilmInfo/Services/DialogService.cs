using FilmInfo.Views;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FilmInfo.Services
{
    public class DialogService
    {
        private DetailView detailView;


        public void OpenDetailView()
        {
            detailView = new DetailView();
            detailView.ShowDialog();
        }

        public void CloseDetailView()
        {
            detailView.Close();
        }
    }
}
