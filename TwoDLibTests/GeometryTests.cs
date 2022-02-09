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
    }
}
