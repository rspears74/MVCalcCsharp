using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using OxyPlot;
using OxyPlot.Series;

namespace MVCalc
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }

        public static Tuple<double, int> FindMax(List<double> list)
        {
            double maxVal = 0;
            int maxIdx = 0;
            for (int i = 0; i < list.Count(); ++i)
            {
                if (i == 0)
                {
                    maxVal = list[i];
                }
                else
                {
                    if (list[i] > maxVal)
                    {
                        maxVal = list[i];
                        maxIdx = i;
                    }
                }
            }

            return new Tuple<double, int>( maxVal, maxIdx );
        }

        public static string MakeList(double[] list)
        {
            string listString = "";
            foreach (double item in list)
            {
                listString = listString + Math.Round(item,2).ToString() + "\n";
            }
            return listString;
        }

        public static PlotModel MakePlot(string name, double[] vals, double spanLength)
        {
            PlotModel model = new PlotModel { Title = name };
            LineSeries series = new LineSeries();
            int numPoints = vals.Length;

            for (int i=0; i < numPoints; ++i)
            {
                series.Points.Add(new DataPoint((spanLength / (numPoints-1) * i), vals[i]));
            }
            model.Series.Add(series);
            return model;
        }

        private void Calculate()
        {
            double spanLength = 0;
            double xLocation = 0;
            bool? feet = false;
            bool? pct = false;
            double incr = 0;
            double impactFactor = 0;
            double distFactor = 0;

            try
            {
                spanLength = double.Parse(SpanLength.Text);
                xLocation = double.Parse(xLoc.Text);
                feet = RadioFeet.IsChecked;
                pct = RadioPct.IsChecked;
                if ((bool)pct)
                {
                    xLocation = spanLength * xLocation;
                }
                incr = double.Parse(Increment.Text) / 12;
                impactFactor = double.Parse(ImpactFactor.Text);
                distFactor = double.Parse(DistributionFactor.Text);
            }
            catch (Exception)
            {
                MessageBox.Show("Invalid input!");
            }

            List<int> axleLoads =     new List<int> { 40, 80, 80, 80, 80, 52, 52, 52, 52,
                                                      40, 80, 80, 80, 80, 52, 52, 52, 52, 8 };
            List<double> axleSpaces = new List<double> { 0, 8, 5, 5, 5, 9, 5, 6, 5,
                                                         8, 8, 5, 5, 5, 9, 5, 6, 5, 5.5 };

            double trainLen = axleSpaces.Sum();
            while (axleSpaces.Sum() - trainLen < spanLength)
            {
                axleSpaces.Add(1);
                axleLoads.Add(8);
            }

            int numAxles = axleLoads.Count();
            double trainTot = axleSpaces.Sum();

            List<double> positions = new List<double> { };
            double point = -spanLength;
            while (point <= trainTot)
            {
                positions.Add(point);
                point += incr;
            }

            List<double> mArray = new List<double> { };
            List<double> vArray = new List<double> { };

            for (int i = 0; i < positions.Count(); ++i)
            {
                List<double> a = new List<double> { };
                List<double> b = new List<double> { };
                List<double> r1 = new List<double> { };
                List<double> r2 = new List<double> { };
                List<double> m = new List<double> { };
                List<double> v = new List<double> { };

                for (int j = 0; j < numAxles; ++j)
                {
                    double aVal = spanLength + positions[i] - axleSpaces.Take(j + 1).Sum();
                    a.Add(aVal);

                    double bVal = spanLength - aVal;
                    b.Add(bVal);

                    double mVal;
                    if ((0 < bVal) && (bVal < spanLength) && (aVal > xLocation))
                        mVal = axleLoads[j] * (aVal - xLocation);
                    else
                        mVal = 0;
                    m.Add(mVal);

                    double vVal;
                    if ((0 < bVal) && (bVal < spanLength) && (aVal < xLocation))
                        vVal = axleLoads[j];
                    else
                        vVal = 0;
                    v.Add(vVal);

                    double r1Val;
                    r1Val = ((bVal > 0) && (bVal < spanLength)) ? axleLoads[j] * bVal / spanLength : 0;
                    r1.Add(r1Val);

                    double r2Val;
                    r2Val = ((0 < aVal) && (aVal < spanLength)) ? axleLoads[j] * aVal / spanLength : 0;
                    r2.Add(r2Val);
                }

                double mTot = r2.Sum() * (spanLength - xLocation) - m.Sum();
                double vTot = Math.Abs(r1.Sum() - v.Sum());
                mArray.Add(mTot);
                vArray.Add(vTot);
            }

            Tuple<double, int> mMaxVals = FindMax(mArray);
            Tuple<double, int> vMaxVals = FindMax(vArray);

            double mMax = mMaxVals.Item1 * (1 + impactFactor) * distFactor;
            double vMax = vMaxVals.Item1 * (1 + impactFactor) * distFactor;
            double mMaxLoc = positions[mMaxVals.Item2];
            double vMaxLoc = positions[vMaxVals.Item2];

            MomentResult.Text = $"The maximum moment {mMax:0.00} k-ft occurs while the front of the train is at {mMaxLoc:0.00} ft.";
            ShearResult.Text = $"The maximum shear {vMax:0.00} k occurs while the front of the train is at {vMaxLoc:0.00} ft.";
        }

        public void CalculatePlots()
        {
            double spanLength = 0;
            Tuple<double[], double[]> vals = new Tuple<double[], double[]>(null,null);

            try
            {
                vals = CalculateEnv();
                spanLength = double.Parse(SpanLength.Text);
            }
            catch (Exception)
            {
                MessageBox.Show("Invalid input!");
            }

            double[] maxMs = vals.Item1;
            double[] maxVs = vals.Item2;
            
            MPlot = MakePlot("Moments", maxMs, spanLength);
            VPlot = MakePlot("Shears", maxVs, spanLength);
        }

        public Tuple<double[], double[]> CalculateEnv()
        {
            double spanLength = 0;
            double incr = 0;
            double impactFactor = 0;
            double distFactor = 0;
            int plotPoints = 0;

            try
            {
                spanLength = double.Parse(SpanLength.Text);
                incr = double.Parse(Increment.Text) / 12;
                impactFactor = double.Parse(ImpactFactor.Text);
                distFactor = double.Parse(DistributionFactor.Text);
                plotPoints = int.Parse(PlotPoints.Text);
            }
            catch (Exception)
            {
                MessageBox.Show("Invalid input!");
            }


            double[] locations = new double[plotPoints + 1];

            for (int i = 0; i < plotPoints+1; ++i)
            {
                locations[i] = spanLength / (plotPoints) * i;
            }

            double[] maxMs = new double[plotPoints + 1];
            double[] maxVs = new double[plotPoints + 1];


            for (int k = 0; k < plotPoints/2 + 1; ++k)
            {
                List<int> axleLoads = new List<int> { 40, 80, 80, 80, 80, 52, 52, 52, 52,
                                                      40, 80, 80, 80, 80, 52, 52, 52, 52, 8 };
                List<double> axleSpaces = new List<double> { 0, 8, 5, 5, 5, 9, 5, 6, 5,
                                                         8, 8, 5, 5, 5, 9, 5, 6, 5, 5.5 };

                double trainLen = axleSpaces.Sum();
                while (axleSpaces.Sum() - trainLen < spanLength)
                {
                    axleSpaces.Add(1);
                    axleLoads.Add(8);
                }

                int numAxles = axleLoads.Count();
                double trainTot = axleSpaces.Sum();

                List<double> positions = new List<double> { };
                double point = -spanLength;
                while (point <= trainTot)
                {
                    positions.Add(point);
                    point += incr;
                }

                List<double> mArray = new List<double> { };
                List<double> vArray = new List<double> { };

                for (int i = 0; i < positions.Count(); ++i)
                {
                    List<double> a = new List<double> { };
                    List<double> b = new List<double> { };
                    List<double> r1 = new List<double> { };
                    List<double> r2 = new List<double> { };
                    List<double> m = new List<double> { };
                    List<double> v = new List<double> { };

                    for (int j = 0; j < numAxles; ++j)
                    {
                        double aVal = spanLength + positions[i] - axleSpaces.Take(j + 1).Sum();
                        a.Add(aVal);

                        double bVal = spanLength - aVal;
                        b.Add(bVal);

                        double mVal;
                        if ((0 < bVal) && (bVal < spanLength) && (aVal > locations[k]))
                            mVal = axleLoads[j] * (aVal - locations[k]);
                        else
                            mVal = 0;
                        m.Add(mVal);

                        double vVal;
                        if ((0 < bVal) && (bVal < spanLength) && (aVal < locations[k]))
                            vVal = axleLoads[j];
                        else
                            vVal = 0;
                        v.Add(vVal);

                        double r1Val;
                        r1Val = ((bVal > 0) && (bVal < spanLength)) ? axleLoads[j] * bVal / spanLength : 0;
                        r1.Add(r1Val);

                        double r2Val;
                        r2Val = ((0 < aVal) && (aVal < spanLength)) ? axleLoads[j] * aVal / spanLength : 0;
                        r2.Add(r2Val);
                    }

                    double mTot = r2.Sum() * (spanLength - locations[k]) - m.Sum();
                    double vTot = Math.Abs(r1.Sum() - v.Sum());
                    mArray.Add(mTot);
                    vArray.Add(vTot);
                }

                Tuple<double, int> mMaxVals = FindMax(mArray);
                Tuple<double, int> vMaxVals = FindMax(vArray);

                double mMax = mMaxVals.Item1 * (1 + impactFactor) * distFactor;
                double vMax = vMaxVals.Item1 * (1 + impactFactor) * distFactor;

                maxMs[k] = mMax;
                maxVs[k] = vMax;
            }

            for (int i = plotPoints; i > plotPoints/2; --i)
            {
                maxMs[i] = maxMs[plotPoints - i];
                maxVs[i] = maxVs[plotPoints - i];
            }

            return new Tuple<double[], double[]>(maxMs, maxVs);
        }

        public static PlotModel MPlot { get; set; }

        public static PlotModel VPlot { get; set; }

        public void clear()
        {
            MomentResult.Text = "";
            ShearResult.Text = "";
        }

        public void calculateClick(object sender, RoutedEventArgs e)
        {
            Calculate();
        }

        public void clearClick(object sender, RoutedEventArgs e)
        {
            clear();
        }

        public void plotClick(object sender, RoutedEventArgs e)
        {
            try
            {
                CalculatePlots();
                PlotWindow plots = new PlotWindow();
                plots.Show();
            }
            catch (Exception)
            {
                MessageBox.Show("Invalid input!");
                return;
            }
        }

        public void valClick(object sender, RoutedEventArgs e)
        {
            double spanLength = 0;
            Tuple<double[], double[]> vals = new Tuple<double[], double[]>(null, null);

            try
            {
                vals = CalculateEnv();
                spanLength = double.Parse(SpanLength.Text);
            }
            catch (Exception)
            {
                MessageBox.Show("Invalid input!");
            }

            double[] maxMs = vals.Item1;
            double[] maxVs = vals.Item2;

            TextResultsWindow results = new TextResultsWindow();

            int numPoints = maxMs.Length;
            double[] locations = new double[numPoints];

            for (int i = 0; i < numPoints; ++i)
            {
                locations[i] = (spanLength / (numPoints - 1) * i);
            }

            results.Locations.Text = MakeList(locations);
            results.Moments.Text = MakeList(maxMs);
            results.Shears.Text = MakeList(maxVs);
            results.Show();
        }
    }
}
