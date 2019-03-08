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

        public static double Moment(double P, double loc, double x, double L)
        {
            double R = P * x / L;
            if (loc < x)
            {
                return R * (L - loc) - P * (x - loc);
            }
            else
            {
                return R * (L - loc);
            }
        }

        public static double Shear(double P, double loc, double x, double L)
        {
            double R = P * x / L;
            if (loc < x)
            {
                return P - R;
            }
            else
            {
                return -R;
            }
        }

        public Dictionary<string, double> CalculateLocation(double xLocation)
        {
            double spanLength = 0;
            double incr = 0;
            double impactFactor = 0;
            double distFactor = 0;

            try
            {
                spanLength = double.Parse(SpanLength.Text);
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

            int numAxles = axleSpaces.Count();

            double[] axlePositions = new double[numAxles];
            axlePositions[0] = -axleSpaces[0];
            for (int i = 1; i < numAxles; i++)
            {
                    axlePositions[i] = axlePositions[i - 1] - axleSpaces[i];
            }
            double mMaxVal = 0;
            double vMaxVal = 0;
            double mMaxLoc = 0;
            double vMaxLoc = 0;

            while (axlePositions.Min() < spanLength)
            {
                List<double> loadsOnBridge = new List<double>();
                List<double> axlesOnBridge = new List<double>();
                for (int j = 0; j < axlePositions.Count(); j++)
                {
                    if (axlePositions[j] > 0 && axlePositions[j] < spanLength)
                    {
                        loadsOnBridge.Add(axleLoads[j]);
                        axlesOnBridge.Add(axlePositions[j]);
                    }
                }

                double[] moments = new double[loadsOnBridge.Count()];
                double[] shears = new double[loadsOnBridge.Count()];
                for (int j = 0; j < loadsOnBridge.Count(); j++)
                {
                    moments[j] = Moment(loadsOnBridge[j], xLocation, axlesOnBridge[j], spanLength);
                    shears[j] = Shear(loadsOnBridge[j], xLocation, axlesOnBridge[j], spanLength);
                }

                double momentTotal = moments.Sum();
                double shearTotal = Math.Abs(shears.Sum());

                if (momentTotal > mMaxVal)
                {
                    mMaxVal = momentTotal;
                    mMaxLoc = axlePositions[0];
                }
                if (shearTotal > vMaxVal)
                {
                    vMaxVal = shearTotal;
                    vMaxLoc = axlePositions[0];
                }
                for (int j = 0; j < axlePositions.Count(); j++)
                {
                    axlePositions[j] = axlePositions[j] + incr;
                }
            }

            double mMax = mMaxVal * (1 + impactFactor) * distFactor;
            double vMax = vMaxVal * (1 + impactFactor) * distFactor;
            Dictionary<string, double> result = new Dictionary<string, double>();
            result.Add("m", mMax);
            result.Add("v", vMax);
            result.Add("mloc", mMaxLoc);
            result.Add("vloc", vMaxLoc);
            return result;
        }

        public void Calculate()
        {
            double spanLength = 0;
            double xLocation = 0;
            bool? feet = false;
            bool? pct = false;

            try
            {
                spanLength = double.Parse(SpanLength.Text);
                xLocation = double.Parse(xLoc.Text);
                feet = RadioFeet.IsChecked;
                pct = RadioPct.IsChecked;
            }
            catch (Exception)
            {
                MessageBox.Show("Invalid input!");
            }
            if ((bool)pct)
            {
                xLocation = spanLength * xLocation;
            }

            Dictionary<string,double> vals = CalculateLocation(xLocation);
            double mMax = vals["m"];
            double vMax = vals["v"];
            double mMaxLoc = vals["mloc"];
            double vMaxLoc = vals["vloc"];
            MomentResult.Text = $"The maximum moment {mMax:0.00} k-ft occurs while the front of the train is at {mMaxLoc:0.00} ft.";
            ShearResult.Text = $"The maximum shear {vMax:0.00} k occurs while the front of the train is at {vMaxLoc:0.00} ft.";
        }

        public void CalculatePlots()
        {
            double spanLength = 0;
            Tuple<double[], double[]> vals = new Tuple<double[], double[]>(null,null);
            vals = CalculateEnv();
            try
            {
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
            int plotPoints = 0;

            try
            {
                spanLength = double.Parse(SpanLength.Text);
                plotPoints = int.Parse(PlotPoints.Text);
            }
            catch (Exception)
            {
                MessageBox.Show("Invalid input!");
            }


            double[] locations = new double[plotPoints + 1];
            double[] mMaxs = new double[plotPoints + 1];
            double[] vMaxs = new double[plotPoints + 1];

            for (int i = 0; i < plotPoints+1; ++i)
            {
                locations[i] = spanLength / (plotPoints) * i;
                Dictionary<string, double> vals = CalculateLocation(locations[i]);
                mMaxs[i] = vals["m"];
                vMaxs[i] = vals["v"];
            }

            return new Tuple<double[], double[]>(mMaxs, vMaxs);
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
