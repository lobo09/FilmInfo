using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media.Imaging;

namespace FilmInfo.Model
{
    public class Movie
    {
        public string Name { get; set; }
        public int Year { get; set; }
        public DateTime MkvCreationTime { get; set; }
        public BitmapImage Poster { get; set; }
        public string Description { get; set; }
        public string MkvFile { get; set; }
        public string MkvFileFull { get; set; }
        public string NfoFile { get; set; }
        public string NfoFileFull { get; set; }
        public string PosterFile { get; set; }
        public string PosterFileFull { get; set; }
    }
}
