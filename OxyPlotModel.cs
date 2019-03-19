using OxyPlot;
using System.ComponentModel;

namespace CPS
{
    public class OxyPlotModel : INotifyPropertyChanged
    {

        private PlotModel plotModel;
        public PlotModel PlotModel
        {
            get
            {
                return plotModel;
            }
            set
            {
                plotModel = value; OnPropertyChanged("PlotModel");
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged(string name)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }
    }
}
