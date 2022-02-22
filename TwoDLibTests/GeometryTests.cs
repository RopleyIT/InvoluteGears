using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using TwoDimensionLib;

namespace TwoDLibTests
{
    [TestClass]
    public class GeometryTests
    {
        [TestMethod]
        public void PerpDistance()
        {
            Coordinate end1 = new(1, 1);
            Coordinate end2 = new(0, 2);
            Coordinate pt = new(1, 2);
            double distance = Geometry.PerpendicularDistance(pt, end1, end2);
            Assert.AreEqual(1 / Math.Sqrt(2), distance);
        }

        [TestMethod]
        public void TestNewtonRaphson()
        {
            static (double, double) quadratic(double x)
            {
                return (x * x - 6 * x - 55, 2 * x - 6);
            }

            double root1 = Geometry.NewtonRaphson(quadratic, 2.99, 0.00001);
            double root2 = Geometry.NewtonRaphson(quadratic, 3.01, 0.00001);

            Assert.AreEqual(-5.0, root1, 0.00001);
            Assert.AreEqual(11.0, root2, 0.00001);
        }

        [TestMethod]
        public void TestRootBinarySearch()
        {
            double root1 = Geometry.RootBinarySearch(x => x * x - 6 * x - 55, -7, -2, 0.00001);
            
            double root2 = Geometry.RootBinarySearch(x => x * x - 6 * x - 55, 3.1, 18, 0.00001);

            Assert.AreEqual(-5.0, root1, 0.00001);
            Assert.AreEqual(11.0, root2, 0.00001);
        }


    }
}
