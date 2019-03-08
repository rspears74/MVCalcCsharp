using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OxyPlot;
using OxyPlot.Series;

namespace MVCalc
{
    class Plots
    {
        public Plots()
        {
            this.MomentPlot = MainWindow.MPlot;
            this.ShearPlot = MainWindow.VPlot;
        }
        public PlotModel MomentPlot { get; set; }
        public PlotModel ShearPlot { get; set; }
        
    }
}
