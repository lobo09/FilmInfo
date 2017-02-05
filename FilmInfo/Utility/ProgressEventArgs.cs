using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FilmInfo.Utility
{
    public class ProgressEventArgs : EventArgs
    {
        public ProgressEventArgs(double percentFinished)
        {
            PercentFinished = percentFinished;
        }
        public ProgressEventArgs(int actualItem, int totalItems)
        {
         
            PercentFinished = (double)actualItem / (double)totalItems * 100;
        }

        public double PercentFinished { get; set; }
    }
}

