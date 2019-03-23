using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MVCalc
{
    class Train
    {
        public List<double> AxleLoads { get; set; }
        public List<double> AxleSpaces { get; set; }
        public List<double> AxlePositions { get; private set; }
        public int NumAxles { get; private set; }
        public TrainConfig TrainType { get; set; }

        public Train(List<double> axleLoads, List<double> axleSpaces)
        {
            AxleLoads = axleLoads;
            AxleSpaces = axleSpaces;
            NumAxles = AxleLoads.Count;
        }

        public static Train GetE80Train()
        {
            List<double> loads = new List<double> { 40, 80, 80, 80, 80, 52, 52, 52, 52, 40, 80, 80, 80, 80, 52, 52, 52, 52, 8 };
            List<double> spacings = new List<double> { 0, 8, 5, 5, 5, 9, 5, 6, 5, 8, 8, 5, 5, 5, 9, 5, 6, 5, 5.5 };
            return new Train(loads, spacings);
        }

        public static Train GetAltE80Train()
        {
            List<double> loads = new List<double> { 100, 100, 100, 100 };
            List<double> spacings = new List<double> { 0, 5, 6, 5 };
            return new Train(loads, spacings);
        }

        public void AddE80TrailingLoad(double spanLength)
        {
            for (int i = 0; i < spanLength; i++)
            {
                AxleSpaces.Add(1);
                AxleLoads.Add(8);
            }
            NumAxles = AxleLoads.Count;
        }

        public void SetAxlePositions()
        {
            AxlePositions = new List<double>();
            AxlePositions.Add(-AxleSpaces[0]);
            for (int i = 1; i < NumAxles; i++)
            {
                AxlePositions.Add(AxlePositions[i - 1] - AxleSpaces[i]);
            }
        }

        public void Increment(double incr)
        {
            for (int i = 0; i < NumAxles; i++)
            {
                AxlePositions[i] = AxlePositions[i]+incr;
            }
        }
    }

    public enum TrainConfig
    {
        E80,
        E80Alt
    }
}
