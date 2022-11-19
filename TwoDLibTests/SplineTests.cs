using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using TwoDimensionLib;
using Plotter;
using System.Collections.Generic;
using System.IO;

namespace TwoDLibTests
{
    [TestClass]
    public class SplineTests
    {
        [TestMethod]
        public void TestCubicCurve()
        {
            Func<double, double> xFunc = x => x;
            Func<double, double> yFunc = x => 1/3.0*x*x*x - x*x - 8*x + 7;
            
            List<Coordinate> lineSegs = new List<Coordinate>();
            for (double xVal = -4; xVal <= 6; xVal += 0.05)
                lineSegs.Add(new Coordinate(xVal, yFunc(xVal)));
            
            Func<double, Coordinate> cFunc = x =>
                new Coordinate(xFunc(x), yFunc(x));
            Spline s = new Spline(3, cFunc, -4.0, +6);
            Coordinate[] splineCoords = s.ControlPoints;
            Assert.AreEqual(lineSegs[0].X, splineCoords[0].X, 0.000001);
            Assert.AreEqual(lineSegs[0].Y, splineCoords[0].Y, 0.000001);
            Assert.AreEqual(lineSegs[lineSegs.Count-1].X, splineCoords[3].X, 0.000001);
            Assert.AreEqual(lineSegs[lineSegs.Count-1].Y, splineCoords[3].Y, 0.000001);
        }
    }
}
