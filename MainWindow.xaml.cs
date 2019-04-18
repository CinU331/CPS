using Microsoft.Win32;
using OxyPlot;
using OxyPlot.Axes;
using OxyPlot.Series;
using System;
using System.Collections.Generic;
using System.IO;
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
        private int numberOfCompartments;
        private double probability;
        private int selectedId;
        private double time;

        private List<SignalAndNoise> SignalsAndNoises;
        private OxyPlotModel plot1;
        private OxyPlotModel plot2;
        private OxyPlotModel resultPlot;
        private Histogram histogram1;
        private Histogram histogram2;
        private Histogram resultHistogram;
        private LineSeries seriesPoints;
        private LineSeries originalSignalPoints;
        private ScatterSeries scatterSeries;
        private List<KeyValuePair<double, double>> values;
        private List<KeyValuePair<double, double>> values2;
        private List<KeyValuePair<double, double>> result;
        private List<KeyValuePair<double, double>> sampledValues;
        private List<KeyValuePair<double, double>> quantizedValues;
        private double[] tmpForRand;
        private bool firstPlotExist;
        private bool secondPlotExist;
        private int selectedExercise;

        private double average;
        private double absoluteMean;
        private double averagePower;
        private double variance;
        private double effectiveValue;
        #endregion

        public MainWindow()
        {
            InitializeComponent();
            SetDataContext();
            AddSignalAndNoisesToListBox();
        }

        private void AddSignalAndNoisesToListBox()
        {
            SignalsAndNoises = new List<SignalAndNoise>
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

        #region SignalsEquation
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
            time = startTime + (n / samplingFrequency);
            if (time >= ((k * basicPeriod) + startTime) && time < ((fillFactor * basicPeriod) + (k * basicPeriod) + startTime))
                return amplitude;
            else if (time >= ((fillFactor * basicPeriod) - (k * basicPeriod) + startTime) && time < (basicPeriod + (k * basicPeriod) + startTime))
                return 0;
            else return 1;
        }

        public double Signal7(int n, double k)
        {
            time = startTime + (n / samplingFrequency);
            if (time >= ((k * basicPeriod) + startTime) && time < ((fillFactor * basicPeriod) + (k * basicPeriod) + startTime))
                return amplitude;
            else if (time >= ((fillFactor * basicPeriod) + (k * basicPeriod) + startTime) && time < (basicPeriod + (k * basicPeriod) + startTime))
                return -amplitude;
            else return 1;
        }

        public double Signal8(int n, double k)
        {
            time = startTime + (n / samplingFrequency);
            if (time >= ((k * basicPeriod) + startTime) && time < ((fillFactor * basicPeriod) + (k * basicPeriod) + startTime))
                return amplitude * (time - (k * basicPeriod) - startTime) / (fillFactor * basicPeriod);
            else if (time >= ((fillFactor * basicPeriod) + (k * basicPeriod) + startTime) && time < (basicPeriod + (k * basicPeriod) + startTime))
                return (-amplitude * (time - (k * basicPeriod) - startTime) / (basicPeriod - (basicPeriod * fillFactor))) + (amplitude / (1 - fillFactor));
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
        #region GeneratingPlots
        private void GeneratePlot(OxyPlotModel plot, String type, List<KeyValuePair<double, double>> values)
        {
            if (selectedId == 0)
                return;
            plot.PlotModel = new PlotModel();
            plot.PlotModel.Axes.Clear();
            plot.PlotModel.Axes.Add(new LinearAxis() { Position = AxisPosition.Bottom, Title = "t[s]", AxisTickToLabelDistance = 5, ExtraGridlines = new Double[] { 0 } });
            double yValue;
            double step = 1 / samplingFrequency;
            originalSignalPoints = new LineSeries();
            double endTime = startTime + duration;
            if (type == "load")
            {
                plot.PlotModel.Axes.Add(new LinearAxis() { Position = AxisPosition.Left, Title = "A", AxisTickToLabelDistance = 5, ExtraGridlines = new Double[] { 0 } });
                for (int i = 0; i < values.Count; i++)
                    originalSignalPoints.Points.Add(new DataPoint(values[i].Key, values[i].Value));
                plot.PlotModel.Series.Add(originalSignalPoints);
                plot.PlotModel.Title = SignalsAndNoises[selectedId - 1].Name;
                return;
            }
            Random random = new Random();
            plot.PlotModel.Title = SignalsAndNoises[selectedId - 1].Name;
            plot.PlotModel.Axes.Add(new LinearAxis() { Position = AxisPosition.Left, Title = "A", MajorStep = amplitude / 2, AxisTickToLabelDistance = 5, ExtraGridlines = new Double[] { 0 } });
            Type t = this.GetType();
            MethodInfo method = t.GetMethod(type);

            if (selectedId == 1 || selectedId == 2)
            {
                for (double i = startTime; i <= endTime; i += step)
                {
                    yValue = (double)method.Invoke(this, new object[] { random, -amplitude, amplitude });
                    originalSignalPoints.Points.Add(new DataPoint(i, yValue));
                    values.Add(new KeyValuePair<double, double>(i, yValue));
                }
                plot.PlotModel.Series.Add(originalSignalPoints);
            }
            else if (selectedId >= 3 && selectedId <= 5)
            {
                int n = 1;
                for (double i = startTime; i <= endTime; i += step, n++)
                {
                    yValue = (double)method.Invoke(this, new object[] { n });
                    originalSignalPoints.Points.Add(new DataPoint(i, yValue));
                    values.Add(new KeyValuePair<double, double>(i, yValue));
                }
                plot.PlotModel.Series.Add(originalSignalPoints);
            }
            else if (selectedId >= 6 && selectedId <= 8)
            {
                double k = 0;
                int n = 1, tmp = 1;
                for (double i = startTime; i <= endTime; i += step, n++)
                {
                    yValue = (double)method.Invoke(this, new object[] { n, k });
                    originalSignalPoints.Points.Add(new DataPoint(i, yValue));
                    values.Add(new KeyValuePair<double, double>(i, yValue));
                    if (i > startTime + (basicPeriod * tmp))
                    {
                        k++;
                        tmp++;
                    }
                }
                plot.PlotModel.Series.Add(originalSignalPoints);
            }
            else if (selectedId == 9)
            {
                for (double i = startTime; i <= endTime; i += step)
                {
                    yValue = (double)method.Invoke(this, new object[] { i });
                    originalSignalPoints.Points.Add(new DataPoint(i, yValue));
                    values.Add(new KeyValuePair<double, double>(i, yValue));
                }
                plot.PlotModel.Series.Add(originalSignalPoints);
            }
            else if (selectedId == 10)
            {
                scatterSeries = new ScatterSeries() { MarkerSize = 2 };
                for (double i = startTime; i <= endTime; i += step)
                {
                    yValue = (double)method.Invoke(this, new object[] { i });
                    scatterSeries.Points.Add(new DataPoint(i, yValue));
                    values.Add(new KeyValuePair<double, double>(i, yValue));
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
                    values.Add(new KeyValuePair<double, double>(i, yValue));
                }
                plot.PlotModel.Series.Add(scatterSeries);
            }
        }
        private void GenerateHistogram(Histogram histogram, List<KeyValuePair<double, double>> values)
        {
            List<double> yValues = (from y in values select y.Value).ToList();
            double stepForHistogram = (yValues.Max() - yValues.Min()) / numberOfCompartments;
            List<double> compartmentsForHistogram = new List<double>
            {
                yValues.Min()
            };
            for (int i = 1; i < numberOfCompartments; i++)
            {
                compartmentsForHistogram.Add(compartmentsForHistogram[i - 1] + stepForHistogram);
            }

            int tmp = 0;
            if (selectedId >= 3 && selectedId <= 9)
            {
                if ((duration / basicPeriod) % 1 != 0)
                {
                    double endTime = startTime + duration;
                    int step = (int)Math.Floor(duration / basicPeriod);
                    double start = endTime - (step * basicPeriod);
                    for (int i = 0; i < values.Count; i++)
                        if (values[i].Key < start)
                            tmp++;
                }
            }

            double[] amount = new double[compartmentsForHistogram.Count];
            for (int i = tmp; i < yValues.Count; i++)
            {
                for (int j = 0; j < compartmentsForHistogram.Count; j++)
                    if (j == numberOfCompartments - 1)
                    {
                        if (yValues[i] >= compartmentsForHistogram[j])
                        {
                            amount[j] += 1;
                            break;
                        }
                    }
                    else
                    {
                        if (yValues[i] >= compartmentsForHistogram[j] && yValues[i] < compartmentsForHistogram[j + 1])
                        {
                            amount[j] += 1;
                            break;
                        }
                    }
            }

            histogram.HistogramModel = new PlotModel();
            histogram.HistogramModel.Axes.Add(new CategoryAxis { Position = AxisPosition.Bottom, Title = "Numer przedziału" });
            histogram.HistogramModel.Axes.Add(new LinearAxis { Position = AxisPosition.Left, Title = "Liczba próbek" });
            var series = new ColumnSeries();
            for (int i = 0; i < compartmentsForHistogram.Count; i++)
                series.Items.Add(new ColumnItem { Value = amount[i] });
            histogram.HistogramModel.Series.Add(series);
        }
        private void GenerateResultPlot(OxyPlotModel plot, List<KeyValuePair<double, double>> values)
        {
            plot.PlotModel = new PlotModel();
            plot.PlotModel.Axes.Clear();
            plot.PlotModel.Title = "Sygnał wynikowy";
            plot.PlotModel.Axes.Add(new LinearAxis() { Position = AxisPosition.Bottom, Title = "t[s]", AxisTickToLabelDistance = 5, ExtraGridlines = new Double[] { 0 } });
            plot.PlotModel.Axes.Add(new LinearAxis() { Position = AxisPosition.Left, Title = "A", AxisTickToLabelDistance = 5, ExtraGridlines = new Double[] { 0 } });
            seriesPoints = new LineSeries();
            for (int i = 0; i < values.Count; i++)
            {
                seriesPoints.Points.Add(new DataPoint(values[i].Key, values[i].Value));
            }
            plot.PlotModel.Series.Add(seriesPoints);
        }
        private void GenerateSampledSignal(OxyPlotModel plot, List<KeyValuePair<double, double>> values)
        {
            int nSample = (int)samplingFrequency;
            sampledValues = new List<KeyValuePair<double, double>>();
            plot.PlotModel = new PlotModel();
            plot.PlotModel.Axes.Clear();
            plot.PlotModel.Title = "Po próbkowaniu";
            plot.PlotModel.Axes.Add(new LinearAxis() { Position = AxisPosition.Bottom, Title = "t[s]", AxisTickToLabelDistance = 5, ExtraGridlines = new Double[] { 0 } });
            plot.PlotModel.Axes.Add(new LinearAxis() { Position = AxisPosition.Left, Title = "A", AxisTickToLabelDistance = 5, ExtraGridlines = new Double[] { 0 } });
            scatterSeries = new ScatterSeries() { MarkerSize = 2 };
            for (int i = 0; i < values.Count; i = i + nSample)
            {
                scatterSeries.Points.Add(new DataPoint(values[i].Key, values[i].Value));
                sampledValues.Add(new KeyValuePair<double, double>(values[i].Key, values[i].Value));
            }
            plot.PlotModel.Series.Add(scatterSeries);
        }
        private void GenerateQuantizedSignal(OxyPlotModel plot, List<KeyValuePair<double, double>> values)
        {
            double yMin = sampledValues.OrderBy(s => s.Value).First().Value;
            double yMax = sampledValues.OrderBy(s => s.Value).Last().Value;
            double step = Math.Abs(yMax - yMin) / samplingFrequency;
            List<double> levelsOfQuantization = new List<double>();
            for (int i = 0; i < samplingFrequency; i++)
                levelsOfQuantization.Add(yMin + i * step);
            quantizedValues = new List<KeyValuePair<double, double>>();
            plot.PlotModel = new PlotModel();
            plot.PlotModel.Axes.Clear();
            plot.PlotModel.Title = "Po kwantyzacji (czarny), oryginalny sygnał (zielony)";
            plot.PlotModel.Axes.Add(new LinearAxis() { Position = AxisPosition.Bottom, Title = "t[s]", AxisTickToLabelDistance = 5, ExtraGridlines = new Double[] { 0 } });
            plot.PlotModel.Axes.Add(new LinearAxis() { Position = AxisPosition.Left, Title = "A", AxisTickToLabelDistance = 5, ExtraGridlines = new Double[] { 0 } });
            seriesPoints = new LineSeries() { Color = OxyColors.Black };
            for (int i = 0; i < values.Count; i++)
            {
                double nearest = levelsOfQuantization.TakeWhile(p => p <= values[i].Value).LastOrDefault();
                seriesPoints.Points.Add(new DataPoint(values[i].Key, nearest));
                quantizedValues.Add(new KeyValuePair<double, double>(values[i].Key, nearest));
            }
            plot.PlotModel.Series.Add(seriesPoints);
            plot.PlotModel.Series.Add(originalSignalPoints);
        }
        #endregion
        #region CalculateParameters
        private void CalculateParameters(List<KeyValuePair<double, double>> values, int plotId)
        {
            int n2 = values.Count;
            int n1 = 1, tmp = 0;
            if (selectedId >= 3 && selectedId <= 9)
            {
                if ((duration / basicPeriod) % 1 != 0)
                {
                    double endTime = startTime + duration;
                    int step = (int)Math.Floor(duration / basicPeriod);
                    double start = endTime - (step * basicPeriod);
                    for (int i = 0; i < n2; i++)
                        if (values[i].Key < start)
                            tmp++;
                    n1 = tmp;
                }
            }
            double sum = 0;
            for (int i = tmp; i < n2; i++)
                sum += values[i].Value;
            average = sum / (n2 - n1 + 1);
            for (int i = tmp; i < n2; i++)
                variance += Math.Pow((values[i].Value - average), 2);
            variance = Math.Round(variance / (n2 - n1 + 1), 3);
            average = Math.Round(sum / (n2 - n1 + 1), 3);

            sum = 0;
            for (int i = tmp; i < n2; i++)
                sum += Math.Abs(values[i].Value);
            absoluteMean = Math.Round(sum / (n2 - n1 + 1), 3);

            sum = 0;
            for (int i = tmp; i < n2; i++)
                sum += Math.Pow(values[i].Value, 2);
            averagePower = sum / (n2 - n1 + 1);
            effectiveValue = Math.Round(Math.Sqrt(averagePower), 3);
            averagePower = Math.Round(averagePower, 3);

            if (plotId == 1)
            {
                Average.Text = average.ToString();
                AbsoluteMean.Text = absoluteMean.ToString();
                AveragePower.Text = averagePower.ToString();
                Variance.Text = variance.ToString();
                EffectiveValue.Text = effectiveValue.ToString();
            }
            else if (plotId == 2)
            {
                Average2.Text = average.ToString();
                AbsoluteMean2.Text = absoluteMean.ToString();
                AveragePower2.Text = averagePower.ToString();
                Variance2.Text = variance.ToString();
                EffectiveValue2.Text = effectiveValue.ToString();
            }
            else
            {
                ResultAverage.Text = average.ToString();
                ResultAbsoluteMean.Text = absoluteMean.ToString();
                ResultAveragePower.Text = averagePower.ToString();
                ResultVariance.Text = variance.ToString();
                ResultEffectiveValue.Text = effectiveValue.ToString();
            }
        }
        #endregion
        #region ButtonsOnClick
        private void Generate_Click(object sender, RoutedEventArgs e)
        {
            SetParametersFromTextBox();
            values = new List<KeyValuePair<double, double>>();
            GeneratePlot(plot1, "Signal" + selectedId.ToString(), values);
            GenerateHistogram(histogram1, values);
            CalculateParameters(values, 1);
            firstPlotExist = true;
            Save1.IsEnabled = true;
            if (secondPlotExist)
            {
                Add.IsEnabled = true;
                Substract.IsEnabled = true;
                Multiply.IsEnabled = true;
                Divide.IsEnabled = true;
            }
        }
        private void Generate2_Click(object sender, RoutedEventArgs e)
        {
            SetParametersFromTextBox();
            values2 = new List<KeyValuePair<double, double>>();
            GeneratePlot(plot2, "Signal" + selectedId.ToString(), values2);
            GenerateHistogram(histogram2, values2);
            CalculateParameters(values2, 2);
            secondPlotExist = true;
            Save2.IsEnabled = true;
            if (firstPlotExist)
            {
                Add.IsEnabled = true;
                Substract.IsEnabled = true;
                Multiply.IsEnabled = true;
                Divide.IsEnabled = true;
            }
        }
        private void Add_Click(object sender, RoutedEventArgs e)
        {
            if (values.Count == values2.Count && values[0].Key == values2[0].Key && values[values.Count - 1].Key == values2[values2.Count - 1].Key)
            {
                result = new List<KeyValuePair<double, double>>();
                for (int i = 0; i < values.Count; i++)
                    result.Insert(i, new KeyValuePair<double, double>(values[i].Key, values[i].Value + values2[i].Value));
                GenerateResultPlot(resultPlot, result);
                GenerateHistogram(resultHistogram, result);
                CalculateParameters(result, 3);
                SaveResult.IsEnabled = true;
            }
            else
                MessageBox.Show("Sygnały znajdują się w innym przedziale czasowym bądź mają inną częstotliwość próbkowania.");
        }
        private void Substract_Click(object sender, RoutedEventArgs e)
        {
            if (values.Count == values2.Count && values[0].Key == values2[0].Key && values[values.Count - 1].Key == values2[values2.Count - 1].Key)
            {
                result = new List<KeyValuePair<double, double>>();
                for (int i = 0; i < values.Count; i++)
                    result.Insert(i, new KeyValuePair<double, double>(values[i].Key, values[i].Value - values2[i].Value));
                GenerateResultPlot(resultPlot, result);
                GenerateHistogram(resultHistogram, result);
                CalculateParameters(result, 3);
                SaveResult.IsEnabled = true;
            }
            else
                MessageBox.Show("Sygnały znajdują się w innym przedziale czasowym bądź mają inną częstotliwość próbkowania.");
        }
        private void Multiply_Click(object sender, RoutedEventArgs e)
        {
            if (values.Count == values2.Count && values[0].Key == values2[0].Key && values[values.Count - 1].Key == values2[values2.Count - 1].Key)
            {
                result = new List<KeyValuePair<double, double>>();
                for (int i = 0; i < values.Count; i++)
                    result.Insert(i, new KeyValuePair<double, double>(values[i].Key, values[i].Value * values2[i].Value));
                GenerateResultPlot(resultPlot, result);
                GenerateHistogram(resultHistogram, result);
                CalculateParameters(result, 3);
                SaveResult.IsEnabled = true;
            }
            else
                MessageBox.Show("Sygnały znajdują się w innym przedziale czasowym bądź mają inną częstotliwość próbkowania.");
        }
        private void Divide_Click(object sender, RoutedEventArgs e)
        {
            if (values.Count == values2.Count && values[0].Key == values2[0].Key && values[values.Count - 1].Key == values2[values2.Count - 1].Key)
            {
                result = new List<KeyValuePair<double, double>>();
                for (int i = 0; i < values.Count; i++)
                {
                    if (values2[i].Value == 0)
                        values2[i] = new KeyValuePair<double, double>(values2[i].Key, 0.000000001);
                    result.Insert(i, new KeyValuePair<double, double>(values[i].Key, values[i].Value / values2[i].Value));
                }
                GenerateResultPlot(resultPlot, result);
                GenerateHistogram(resultHistogram, result);
                CalculateParameters(result, 3);
                SaveResult.IsEnabled = true;
            }
            else
                MessageBox.Show("Sygnały znajdują się w innym przedziale czasowym bądź mają inną częstotliwość próbkowania.");

        }
        private void Load1_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog();
            if (openFileDialog.ShowDialog() == true)
                LoadFromBinFile(openFileDialog.FileName, ref values);
            GeneratePlot(plot1, "load", values);
            GenerateHistogram(histogram1, values);
            CalculateParameters(values, 1);
            firstPlotExist = true;
            Save1.IsEnabled = true;
            if (secondPlotExist)
            {
                Add.IsEnabled = true;
                Substract.IsEnabled = true;
                Multiply.IsEnabled = true;
                Divide.IsEnabled = true;
            }
        }
        private void Save1_Click(object sender, RoutedEventArgs e)
        {
            SaveFileDialog saveFileDialog = new SaveFileDialog();
            if (saveFileDialog.ShowDialog() == true)
                SaveToBinFile(saveFileDialog.FileName, values);
        }
        private void Load2_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog();
            if (openFileDialog.ShowDialog() == true)
                LoadFromBinFile(openFileDialog.FileName, ref values2);
            GeneratePlot(plot2, "load", values2);
            GenerateHistogram(histogram2, values2);
            CalculateParameters(values2, 2);
            secondPlotExist = true;
            Save2.IsEnabled = true;
            if (firstPlotExist)
            {
                Add.IsEnabled = true;
                Substract.IsEnabled = true;
                Multiply.IsEnabled = true;
                Divide.IsEnabled = true;
            }
        }
        private void Save2_Click(object sender, RoutedEventArgs e)
        {
            SaveFileDialog saveFileDialog = new SaveFileDialog();
            if (saveFileDialog.ShowDialog() == true)
                SaveToBinFile(saveFileDialog.FileName, values2);
        }
        private void LoadResult_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog();
            if (openFileDialog.ShowDialog() == true)
                LoadFromBinFile(openFileDialog.FileName, ref result);
            GenerateResultPlot(resultPlot, result);
            GenerateHistogram(resultHistogram, result);
            CalculateParameters(result, 3);
            SaveResult.IsEnabled = true;
        }
        private void SaveResult_Click(object sender, RoutedEventArgs e)
        {
            SaveFileDialog saveFileDialog = new SaveFileDialog();
            if (saveFileDialog.ShowDialog() == true)
                SaveToBinFile(saveFileDialog.FileName, result);
        }
        #endregion
        #region SetsOfParameters
        private void FirstSetOfParameters()
        {
            Amplitude.IsEnabled = true;
            StartTime.IsEnabled = true;
            Duration.IsEnabled = true;
            BasicPeriod.IsEnabled = false;
            FillFactor.IsEnabled = false;
            Probability.IsEnabled = false;
        }
        private void SecondSetOfParameters()
        {
            Amplitude.IsEnabled = true;
            StartTime.IsEnabled = true;
            Duration.IsEnabled = true;
            BasicPeriod.IsEnabled = true;
            FillFactor.IsEnabled = false;
            Probability.IsEnabled = false;
        }
        private void ThirdSetOfParameters()
        {
            Amplitude.IsEnabled = true;
            StartTime.IsEnabled = true;
            Duration.IsEnabled = true;
            BasicPeriod.IsEnabled = true;
            FillFactor.IsEnabled = true;
            Probability.IsEnabled = false;
        }
        private void FourthSetOfParameters()
        {
            Amplitude.IsEnabled = true;
            StartTime.IsEnabled = true;
            Duration.IsEnabled = true;
            BasicPeriod.IsEnabled = false;
            FillFactor.IsEnabled = false;
            Probability.IsEnabled = true;
        }
        private void SetDataContext()
        {
            plot1 = new OxyPlotModel();
            Plot1.DataContext = plot1;
            plot2 = new OxyPlotModel();
            Plot2.DataContext = plot2;
            resultPlot = new OxyPlotModel();
            ResultPlot.DataContext = resultPlot;
            histogram1 = new Histogram();
            Histogram1.DataContext = histogram1;
            histogram2 = new Histogram();
            Histogram2.DataContext = histogram2;
            resultHistogram = new Histogram();
            ResultHistogram.DataContext = resultHistogram;
        }
        private void SetParametersFromTextBox()
        {
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
                numberOfCompartments = int.Parse(NumberOfCompartments.Text);
            if (Probability.Text != "")
                probability = double.Parse(Probability.Text);
        }
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
                    case 10:
                        FirstSetOfParameters();
                        break;
                    case 11:
                        FourthSetOfParameters();
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
        #endregion
        #region BinFiles
        private void SaveToBinFile(string fileName, List<KeyValuePair<double, double>> values)
        {
            using (BinaryWriter binWriter = new BinaryWriter(File.Open(fileName, FileMode.Create)))
            {
                binWriter.Write(selectedId);
                binWriter.Write(startTime);
                binWriter.Write(duration);
                binWriter.Write(samplingFrequency);
                binWriter.Write(numberOfCompartments);
                for (int i = 0; i < values.Count; i++)
                {
                    binWriter.Write(values[i].Key);
                    binWriter.Write(values[i].Value);
                }
            }
        }
        private void LoadFromBinFile(string fileName, ref List<KeyValuePair<double, double>> values)
        {
            using (BinaryReader binReader = new BinaryReader(File.Open(fileName, FileMode.Open)))
            {
                selectedId = binReader.ReadInt32();
                ListOfSignalsAndNoises.SelectedItem = selectedId;
                startTime = binReader.ReadDouble();
                StartTime.Text = startTime.ToString();
                duration = binReader.ReadDouble();
                Duration.Text = duration.ToString();
                samplingFrequency = binReader.ReadDouble();
                SamplingFrequency.Text = samplingFrequency.ToString();
                numberOfCompartments = binReader.ReadInt32();
                NumberOfCompartments.Text = numberOfCompartments.ToString();
                values = new List<KeyValuePair<double, double>>();
                int i = 0;
                while (binReader.BaseStream.Position != binReader.BaseStream.Length)
                {
                    values.Insert(i, new KeyValuePair<double, double>(binReader.ReadDouble(), binReader.ReadDouble()));
                    i++;
                }
            }
        }
        #endregion

        private void Exercise_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            selectedExercise = Exercises.SelectedIndex;
            switch (selectedExercise)
            {
                case 0:
                    Generate2.Content = "Generuj drugi wykres";
                    Generate2.Click += Generate2_Click;
                    Generate2.Click -= Sample_Click;
                    SamplingFrequencyText.Text = "Częstotliwość próbkowania:";
                    break;
                case 1:
                    Generate2.Content = "Próbkowanie";
                    Generate2.Click -= Generate2_Click;
                    Generate2.Click += Sample_Click;
                    SamplingFrequencyText.Text = "Próbkuj co ile próbek:";
                    break;
            }
        }

        private void Sample_Click(object sender, RoutedEventArgs e)
        {
            if (firstPlotExist)
            {
                if (SamplingFrequency.Text != "")
                    samplingFrequency = double.Parse(SamplingFrequency.Text);
                GenerateSampledSignal(plot2, values);
                GenerateHistogram(histogram2, sampledValues);
                CalculateParameters(sampledValues, 2);
                PrepareForQuantization();
            }
        }

        private void PrepareForQuantization()
        {
            SamplingFrequencyText.Text = "Liczba poziomów kwantyzacji:";
            Generate2.Content = "Kwantyzacja";
            Generate2.Click -= Sample_Click;
            Generate2.Click += Quantization_Click;
        }

        private void Quantization_Click(object sender, RoutedEventArgs e)
        {
            if (SamplingFrequency.Text != "")
                samplingFrequency = double.Parse(SamplingFrequency.Text);
            GenerateQuantizedSignal(plot2, sampledValues);
            GenerateHistogram(histogram2, quantizedValues);
            CalculateParameters(quantizedValues, 2);
        }
    }
}