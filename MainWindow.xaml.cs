using OxyPlot;
using OxyPlot.Axes;
using OxyPlot.Series;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Windows;
using System.Windows.Controls;

namespace CPS
{
    public partial class MainWindow : Window
    {
        #region Variables
        private double amplitude;
        private double startTime;
        private double duration;
        private double basicPeriod;
        private double fillFactor;
        private double samplingFrequency;
        private double numberOfCompartments;
        private double probability;
        private int selectedId;
        private double time;

        private OxyPlotModel plot1;
        private OxyPlotModel plot2;
        private Histogram histogram1;
        private Histogram histogram2;
        private LineSeries seriesPoints;
        private ScatterSeries scatterSeries;
        private List<double> values;
        private double[] tmpForRand;
        #endregion

        public MainWindow()
        {
            InitializeComponent();
            plot1 = new OxyPlotModel();
            Plot1.DataContext = plot1;
            plot2 = new OxyPlotModel();
            Plot2.DataContext = plot2;
            histogram1 = new Histogram();
            Histogram1.DataContext = histogram1;
            histogram2 = new Histogram();
            Histogram2.DataContext = histogram2;
            AddSignalAndNoisesToListBox();
        }

        private void AddSignalAndNoisesToListBox()
        {
            List<SignalAndNoise> SignalsAndNoises = new List<SignalAndNoise>
            {
                new SignalAndNoise() { Name = "Szum o rozkładzie jednostajnym", Id = 1 },
                new SignalAndNoise() { Name = "Szum gaussowki", Id = 2 },
                new SignalAndNoise() { Name = "Sygnał sinusoidalny", Id = 3 },
                new SignalAndNoise() { Name = "Sygnał sinusoidalny wyprostowany jednopołówkowo", Id = 4 },
                new SignalAndNoise() { Name = "Sygnał sinusoidalny wyprostowany dwupołówkowo", Id = 5 },
                new SignalAndNoise() { Name = "Sygnał prostokątny", Id = 6 },
                new SignalAndNoise() { Name = "Sygnał prostokątny symetryczny", Id = 7 },
                new SignalAndNoise() { Name = "Sygnał trójkątny", Id = 8 },
                new SignalAndNoise() { Name = "Skok jednostkowy", Id = 9 },
                new SignalAndNoise() { Name = "Impuls jednostkowy", Id = 10 },
                new SignalAndNoise() { Name = "Szum jednostkowy", Id = 11 }
            };
            ListOfSignalsAndNoises.ItemsSource = SignalsAndNoises;
        }

        #region SetsOfParameters
        private void FirstSetOfParameters()
        {
            Amplitude.IsEnabled = true;
            StartTime.IsEnabled = true;
            Duration.IsEnabled = true;
            BasicPeriod.IsEnabled = false;
            FillFactor.IsEnabled = false;
        }

        private void SecondSetOfParameters()
        {
            Amplitude.IsEnabled = true;
            StartTime.IsEnabled = true;
            Duration.IsEnabled = true;
            BasicPeriod.IsEnabled = true;
            FillFactor.IsEnabled = false;
        }

        private void ThirdSetOfParameters()
        {
            Amplitude.IsEnabled = true;
            StartTime.IsEnabled = true;
            Duration.IsEnabled = true;
            BasicPeriod.IsEnabled = true;
            FillFactor.IsEnabled = true;
        }
        #endregion

        private void ListOfSignalsAndNoises_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (ListOfSignalsAndNoises.SelectedItem != null)
            {
                var item = (ListBox)sender;
                var SignalAndNoise = (SignalAndNoise)item.SelectedItem;
                selectedId = SignalAndNoise.Id;
                switch (selectedId)
                {
                    case 1:
                    case 2:
                        FirstSetOfParameters();
                        break;
                    case 3:
                    case 4:
                    case 5:
                        SecondSetOfParameters();
                        break;
                    case 6:
                    case 7:
                    case 8:
                        ThirdSetOfParameters();
                        break;
                    case 9:
                        FirstSetOfParameters();
                        break;
                    case 10:
                        break;
                    case 11:
                        break;
                    default:
                        break;
                }
            }
        }

        private void Parameters_BeginningEdit(object sender, DataGridBeginningEditEventArgs e)
        {
            e.Cancel = true;
        }

        #region Signals
        public double Signal1(Random random, double minimum, double maximum)
        {
            return random.NextDouble() * (maximum - minimum) + minimum;
        }

        public double Signal2(Random random, double minimum, double maximum)
        {
            int n = 10;
            double x = 0.0;
            for (int i = 0; i < n; i++)
                x += Signal1(random, minimum, maximum);
            return x * Math.Sqrt(1 / (double)n);
        }

        public double Signal3(int n)
        {
            return amplitude * Math.Sin((2 * Math.PI * (n / samplingFrequency - startTime)) / basicPeriod);
        }

        public double Signal4(int n)
        {
            return 0.5 * amplitude * ((Math.Sin((2 * Math.PI * (n / samplingFrequency - startTime)) / basicPeriod)) + Math.Abs(Math.Sin((2 * Math.PI * (n / samplingFrequency - startTime)) / basicPeriod)));
        }

        public double Signal5(int n)
        {
            return amplitude * (Math.Abs(Math.Sin((2 * Math.PI * (n / samplingFrequency - startTime)) / basicPeriod)));
        }

        public double Signal6(int n, double k)
        {
            time = n / samplingFrequency - startTime;
            if (time >= ((k * basicPeriod) + startTime) && time < ((fillFactor * basicPeriod) + (k * basicPeriod) + startTime))
                return amplitude;
            else if (time >= ((fillFactor * basicPeriod) - (k * basicPeriod) + startTime) && time < (basicPeriod + (k * basicPeriod) + startTime))
                return 0;
            else return 1;
        }

        public double Signal7(int n, double k)
        {
            time = n / samplingFrequency - startTime;
            if (time >= ((k * basicPeriod) + startTime) && time < ((fillFactor * basicPeriod) + (k * basicPeriod) + startTime))
                return amplitude;
            else if (time >= ((fillFactor * basicPeriod) + (k * basicPeriod) + startTime) && time < (basicPeriod + (k * basicPeriod) + startTime))
                return -amplitude;
            else return 1;
        }

        public double Signal8(int n, double k)
        {
            time = n / samplingFrequency - startTime;
            if (time >= ((k * basicPeriod) + startTime) && time < ((fillFactor * basicPeriod) + (k * basicPeriod) + startTime))
                return amplitude * (time - (k * basicPeriod) - startTime) / (fillFactor * basicPeriod);
            else if (time >= ((fillFactor * basicPeriod) + (k * basicPeriod) + startTime) && time < (basicPeriod + (k * basicPeriod) + startTime))
                return (-amplitude * (time - (k * basicPeriod) - startTime) / (basicPeriod - (basicPeriod * fillFactor))) + (amplitude/(1 - fillFactor));
            else return 1;
        }

        public double Signal9(double time)
        {
            if (time > 0)
                return amplitude;
            else if (time == 0)
                return amplitude / 2;
            else
                return 0;
        }

        public double Signal10(double time)
        {
            if (time == 0)
                return 1;
            else return 0;
        }

        public double Signal11(Random random)
        {
            int index = random.Next(100);
            return tmpForRand[index];
        }
        #endregion
        private void SetParameters()
        {
            amplitude = 10;
            startTime = -10;
            duration = 20;
            samplingFrequency = 1024;
            numberOfCompartments = 10;
            fillFactor = 0.5;
            basicPeriod = 2;
            probability = 0.5;

            if (Amplitude.Text != "")
                amplitude = double.Parse(Amplitude.Text);
            if (StartTime.Text != "")
                startTime = double.Parse(StartTime.Text);
            if (Duration.Text != "")
                duration = double.Parse(Duration.Text);
            if (BasicPeriod.Text != "")
                basicPeriod = double.Parse(BasicPeriod.Text);
            if (FillFactor.Text != "")
                fillFactor = double.Parse(FillFactor.Text);
            if (SamplingFrequency.Text != "")
                samplingFrequency = double.Parse(SamplingFrequency.Text);
            if (NumberOfCompartments.Text != "")
                numberOfCompartments = double.Parse(NumberOfCompartments.Text);
            if (Probability.Text != "")
                probability = double.Parse(Probability.Text);
        }

        private void GeneratePlot(OxyPlotModel plot, String type)
        {
            if (selectedId == 0)
                return;
            Type t = this.GetType();
            MethodInfo method = t.GetMethod(type);
            plot.PlotModel = new PlotModel();
            plot.PlotModel.Axes.Clear();
            plot.PlotModel.Axes.Add(new LinearAxis() { Position = AxisPosition.Bottom, AxisTickToLabelDistance = 5, ExtraGridlines = new Double[] { 0 } });
            plot.PlotModel.Axes.Add(new LinearAxis() { Position = AxisPosition.Left, MajorStep = amplitude / 2, AxisTickToLabelDistance = 5, ExtraGridlines = new Double[] { 0 } });
            Random random = new Random();
            values = new List<double>();
            double yValue;
            double step = 1 / samplingFrequency;
            seriesPoints = new LineSeries();
            double endTime = startTime + duration;

            if (selectedId == 1 || selectedId == 2)
            {
                for (double i = startTime; i <= endTime; i += step)
                {
                    yValue = (double)method.Invoke(this, new object[] { random, -amplitude, amplitude });
                    seriesPoints.Points.Add(new DataPoint(i, yValue));
                    values.Add(yValue);
                }
                plot.PlotModel.Series.Add(seriesPoints);
            }
            else if (selectedId >= 3 && selectedId <= 5)
            {
                int n = 1;
                for (double i = startTime; i <= endTime; i += step, n++)
                {
                    yValue = (double)method.Invoke(this, new object[] { n });
                    seriesPoints.Points.Add(new DataPoint(i, yValue));
                    values.Add(yValue);
                }
                plot.PlotModel.Series.Add(seriesPoints);
            }
            else if (selectedId >= 6 && selectedId <= 8)
            {
                double k = 0;
                int n = 1, tmp = 1;
                for (double i = startTime; i <= endTime; i += step, n++)
                {
                    yValue = (double)method.Invoke(this, new object[] { n, k });
                    seriesPoints.Points.Add(new DataPoint(i, yValue));
                    values.Add(yValue);
                    if (i > basicPeriod * tmp)
                    {
                        k++;
                        tmp++;
                    }
                }
                plot.PlotModel.Series.Add(seriesPoints);
            }
            else if (selectedId == 9)
            {
                for (double i = startTime; i <= endTime; i += step)
                {
                    yValue = (double)method.Invoke(this, new object[] { i });
                    seriesPoints.Points.Add(new DataPoint(i, yValue));
                    values.Add(yValue);
                }
                plot.PlotModel.Series.Add(seriesPoints);
            }
            else if (selectedId == 10)
            {
                scatterSeries = new ScatterSeries(){ MarkerSize = 2 };
                for (double i = startTime; i <= endTime; i += step)
                {
                    yValue = (double)method.Invoke(this, new object[] { i });
                    scatterSeries.Points.Add(new DataPoint(i, yValue));
                    values.Add(yValue);
                }
                plot.PlotModel.Series.Add(scatterSeries);
            }
            else if (selectedId == 11)
            {
                tmpForRand = new double[100];
                int count = (int)(probability * 100);
                for (int i = 0; i < tmpForRand.Length; i++)
                {
                    if (i < count)
                        tmpForRand[i] = amplitude;
                    else
                        tmpForRand[i] = 0;
                }
                scatterSeries = new ScatterSeries() { MarkerSize = 2 };
                for (double i = startTime; i <= endTime; i += step)
                {
                    yValue = (double)method.Invoke(this, new object[] { random });
                    scatterSeries.Points.Add(new DataPoint(i, yValue));
                    values.Add(yValue);
                }
                plot.PlotModel.Series.Add(scatterSeries);
            }
        }

        private void GenerateHistogram(Histogram histogram)
        {
            double stepForHistogram = (values.Max() - values.Min()) / numberOfCompartments;
            List<double> compartmentsForHistogram = new List<double>
            {
                values.Min()
            };
            for (int i = 1; i < numberOfCompartments; i++)
            {
                compartmentsForHistogram.Add(compartmentsForHistogram[i - 1] + stepForHistogram);
            }

            double[] amount = new double[compartmentsForHistogram.Count];
            for (int i = 0; i < values.Count; i++)
            {
                for (int j = 0; j < compartmentsForHistogram.Count; j++)
                    if (j == numberOfCompartments - 1)
                    {
                        if (values[i] >= compartmentsForHistogram[j])
                        {
                            amount[j] += 1;
                            break;
                        }
                    }
                    else
                    {
                        if (values[i] >= compartmentsForHistogram[j] && values[i] < compartmentsForHistogram[j + 1])
                        {
                            amount[j] += 1;
                            break;
                        }
                    }
            }

            histogram.HistogramModel = new PlotModel();
            var series = new ColumnSeries();
            for (int i = 0; i < compartmentsForHistogram.Count; i++)
                series.Items.Add(new ColumnItem { Value = amount[i] });
            histogram.HistogramModel.Series.Add(series);
        }

        private void Generate_Click(object sender, RoutedEventArgs e)
        {
            SetParameters();
            GeneratePlot(plot1, "Signal" + selectedId.ToString());
            GenerateHistogram(histogram1);
        }

        private void Generate2_Click(object sender, RoutedEventArgs e)
        {
            SetParameters();
            GeneratePlot(plot2, "Signal" + selectedId.ToString());
            GenerateHistogram(histogram2);
        }
    }
}