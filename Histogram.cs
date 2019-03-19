using OxyPlot;
using System.ComponentModel;

namespace CPS
{
    public class Histogram : INotifyPropertyChanged
    {
        private PlotModel histogramModel;
        public PlotModel HistogramModel
        {
            get { return histogramModel; }
            set { histogramModel = value; OnPropertyChanged("HistogramModel"); }
        }
        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
