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

			return new Tuple<double, int>(maxVal, maxIdx);
		}

		public static string MakeList(double[] list)
		{
			string listString = "";
			foreach (double item in list)
			{
				listString = listString + Math.Round(item, 2).ToString() + "\n";
			}
			return listString;
		}

		public static PlotModel MakePlot(string name, double[] vals, double spanLength)
		{
			PlotModel model = new PlotModel { Title = name };
			LineSeries series = new LineSeries();
			int numPoints = vals.Length;

			for (int i = 0; i < numPoints; ++i)
			{
				series.Points.Add(new DataPoint((spanLength / (numPoints - 1) * i), vals[i]));
			}
			model.Series.Add(series);
			return model;
		}

		public static PlotModel MPlot { get; set; }

		public static PlotModel VPlot { get; set; }

		public void CalculateClick(object sender, RoutedEventArgs e)
		{
			// declare/instantiate variables
			E80Analysis analysis = new E80Analysis();
			double xLocation;
			bool? pct;

			// get user inputs
			try
			{
				xLocation = double.Parse(xLoc.Text);
				pct = RadioPct.IsChecked;
				analysis.Span = double.Parse(SpanLength.Text);
				analysis.IncrementInches = double.Parse(Increment.Text);
				analysis.ImpactFactor = double.Parse(ImpactFactor.Text);
				analysis.DistFactor = double.Parse(DistributionFactor.Text);
				analysis.GetTrainType(LoadType.SelectedIndex);
			}
			catch (Exception)
			{
				MessageBox.Show("Invalid input!");
				return;
			}

			// if the percent radio button is checked, set the xLocation value to its value times the span length
			if ((bool)pct)
			{
				xLocation = analysis.Span * xLocation;
			}

			analysis.GetTrain();
			Dictionary<string, double> vals = analysis.CalculateSingleLocation(xLocation);
			double mMax = vals["m"];
			double vMax = vals["v"];
			double mMaxLoc = vals["mloc"];
			double vMaxLoc = vals["vloc"];
			MomentResult.Text = $"The maximum moment {mMax:0.00} k-ft occurs while the front of the train is at {mMaxLoc:0.00} ft.";
			ShearResult.Text = $"The maximum shear {vMax:0.00} k occurs while the front of the train is at {vMaxLoc:0.00} ft.";
		}

		public void ClearClick(object sender, RoutedEventArgs e)
		{
			MomentResult.Text = "";
			ShearResult.Text = "";
		}

		public void PlotClick(object sender, RoutedEventArgs e)
		{
			// declare/instantiate variables
			E80Analysis analysis = new E80Analysis();
			int plotPoints;

			// get user inputs
			try
			{
				plotPoints = int.Parse(PlotPoints.Text);
				analysis.Span = double.Parse(SpanLength.Text);
				analysis.IncrementInches = double.Parse(Increment.Text);
				analysis.ImpactFactor = double.Parse(ImpactFactor.Text);
				analysis.DistFactor = double.Parse(DistributionFactor.Text);
				analysis.GetTrainType(LoadType.SelectedIndex);
			}
			catch (Exception)
			{
				MessageBox.Show("Invalid input!");
				return;
			}

			analysis.GetTrain();
			Tuple<double[], double[]> vals = analysis.CalculateEnvelope(plotPoints);

			double[] maxMs = vals.Item1;
			double[] maxVs = vals.Item2;

			MPlot = MakePlot("Moments", maxMs, analysis.Span);
			VPlot = MakePlot("Shears", maxVs, analysis.Span);

			PlotWindow plots = new PlotWindow();
			plots.Show();
		}

		public void ValClick(object sender, RoutedEventArgs e)
		{
			// declare/instantiate variables
			E80Analysis analysis = new E80Analysis();
			int plotPoints;

			// get user inputs
			try
			{
				plotPoints = int.Parse(PlotPoints.Text);
				analysis.Span = double.Parse(SpanLength.Text);
				analysis.IncrementInches = double.Parse(Increment.Text);
				analysis.ImpactFactor = double.Parse(ImpactFactor.Text);
				analysis.DistFactor = double.Parse(DistributionFactor.Text);
				analysis.GetTrainType(LoadType.SelectedIndex);
			}
			catch (Exception)
			{
				MessageBox.Show("Invalid input!");
				return;
			}

			analysis.GetTrain();
			Tuple<double[], double[]> vals = analysis.CalculateEnvelope(plotPoints);

			double[] maxMs = vals.Item1;
			double[] maxVs = vals.Item2;

			TextResultsWindow results = new TextResultsWindow();

			double[] locations = new double[plotPoints + 1];

			for (int i = 0; i < plotPoints + 1; ++i)
			{
				locations[i] = analysis.Span / (plotPoints) * i;
			}

			results.Locations.Text = MakeList(locations);
			results.Moments.Text = MakeList(maxMs);
			results.Shears.Text = MakeList(maxVs);
			results.Show();
		}
	}
}
