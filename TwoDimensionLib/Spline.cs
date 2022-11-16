using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TwoDimensionLib
{
    public class Spline
    {
        const int resolution = 1024;
        Coordinate[] controlPoints = new Coordinate[4];

        Coordinate CalcPoint(double t)
        {
            Coordinate rVal = controlPoints[3].Scale(t * t * t);
            rVal += controlPoints[2].Scale(3 * (1 - t) * t * t);
            rVal += controlPoints[1].Scale(3 * (1 - t) * (1 - t) * t);
            rVal += controlPoints[0].Scale((1 - t) * (1 - t) * (1 - t));
            return rVal;
        }

        public void FitCubicToFunction(Func<double, double> func, 
            double xMin, double xMax)
        {
            // Find the dy/dx gradient at the start and end of the curve

            double dx = (xMax - xMin)/resolution;
            double gradStart = (func(xMin + dx) - func(xMin)) / dx;
            double gradEnd = (func(xMax) - func(xMax - dx)) / dx;

            // TODO . . .
        }
    }
}
