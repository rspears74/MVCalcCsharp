using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Input;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;

namespace MVCalc
{
    public class E80Analysis
    {

        public Train AnalysisTrain { get; set; }
        public double Span { get; set; }
        public double IncrementInches { get; set; }
        public double ImpactFactor { get; set; }
        public double DistFactor { get; set; }
        public TrainConfig TrainType { get; set; }

        public E80Analysis() { }

        public void GetTrain()
        {
            AnalysisTrain = Train.GetAltE80Train();

            if (TrainType == TrainConfig.E80)
            {
                AnalysisTrain = Train.GetE80Train();
                AnalysisTrain.AddE80TrailingLoad(Span);
            }
        }

        public Dictionary<string, double> CalculateSingleLocation(double xLocation)
        {
            AnalysisTrain.SetAxlePositions();
            double incr = IncrementInches / 12;
            double mMaxVal = 0;
            double vMaxVal = 0;
            double mMaxLoc = 0;
            double vMaxLoc = 0;

            while (AnalysisTrain.AxlePositions.Min() < Span)
            {
                List<double> loadsOnBridge = new List<double>();
                List<double> axlesOnBridge = new List<double>();
                for (int j = 0; j < AnalysisTrain.AxlePositions.Count(); j++)
                {
                    if (AnalysisTrain.AxlePositions[j] > 0 && AnalysisTrain.AxlePositions[j] < Span)
                    {
                        loadsOnBridge.Add(AnalysisTrain.AxleLoads[j]);
                        axlesOnBridge.Add(AnalysisTrain.AxlePositions[j]);
                    }
                }

                double[] moments = new double[loadsOnBridge.Count()];
                double[] shears = new double[loadsOnBridge.Count()];
                for (int j = 0; j < loadsOnBridge.Count(); j++)
                {
                    moments[j] = Moment(loadsOnBridge[j], xLocation, axlesOnBridge[j], Span);
                    shears[j] = Shear(loadsOnBridge[j], xLocation, axlesOnBridge[j], Span);
                }

                double momentTotal = moments.Sum();
                double shearTotal = Math.Abs(shears.Sum());

                if (momentTotal > mMaxVal)
                {
                    mMaxVal = momentTotal;
                    mMaxLoc = AnalysisTrain.AxlePositions[0];
                }
                if (shearTotal > vMaxVal)
                {
                    vMaxVal = shearTotal;
                    vMaxLoc = AnalysisTrain.AxlePositions[0];
                }
                AnalysisTrain.Increment(incr);
            }

            double mMax = mMaxVal * ImpactFactor * DistFactor;
            double vMax = vMaxVal * ImpactFactor * DistFactor;
            Dictionary<string, double> result = new Dictionary<string, double>();
            result.Add("m", mMax);
            result.Add("v", vMax);
            result.Add("mloc", mMaxLoc);
            result.Add("vloc", vMaxLoc);
            return result;
        }

        public Tuple<double[], double[]> CalculateEnvelope(int plotPoints)
        {
            double[] locations = new double[plotPoints + 1];
            double[] mMaxs = new double[plotPoints + 1];
            double[] vMaxs = new double[plotPoints + 1];

            for (int i = 0; i < plotPoints + 1; ++i)
            {
                locations[i] = Span / (plotPoints) * i;
                Dictionary<string, double> vals = CalculateSingleLocation(locations[i]);
                mMaxs[i] = vals["m"];
                vMaxs[i] = vals["v"];
            }

            return new Tuple<double[], double[]>(mMaxs, vMaxs);
        }

        public void GetTrainType(int type)
        {
            if (type == 0)
            {
                TrainType = TrainConfig.E80;
            }
            else
            {
                TrainType = TrainConfig.E80Alt;
            }
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

    }
}
