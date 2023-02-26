using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Collections.Generic;
using TwoDimensionLib;

namespace TwoDLibTests
{
    [TestClass]
    public class SplineTests
    {
        [TestMethod]
        public void TestCubicCurve()
        {
            static double xFunc(double x) => x;
            static double yFunc(double x)
                => 1 / 3.0 * x * x * x - x * x - 8 * x + 7;

            List<Coordinate> lineSegs = new();
            for (double xVal = -4; xVal <= 6; xVal += 0.05)
                lineSegs.Add(new Coordinate(xVal, yFunc(xVal)));

            static Coordinate cFunc(double x) => new(xFunc(x), yFunc(x));
            Spline s = new(3, cFunc, -4.0, +6);
            Coordinate[] splineCoords = s.ControlPoints;
            Assert.AreEqual(lineSegs[0].X, splineCoords[0].X, 0.000001);
            Assert.AreEqual(lineSegs[0].Y, splineCoords[0].Y, 0.000001);
            Assert.AreEqual(lineSegs[^1].X, splineCoords[3].X, 0.000001);
            Assert.AreEqual(lineSegs[^1].Y, splineCoords[3].Y, 0.000001);
        }
    }
}
