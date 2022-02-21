using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using TwoDimensionLib;
namespace TwoDLibTests
{
    [TestClass]
    public class CoordinateTests
    {
        [TestMethod]
        public void CanCreate()
        {
            Coordinate c = new ();
            Assert.IsNotNull(c);
            Assert.IsTrue(c == Coordinate.Empty);
        }

        [TestMethod]
        public void CanConvertToString()
        {
            Coordinate c = new (3, 1.5);
            Assert.IsNotNull(c);
            Assert.IsTrue(c == new Coordinate(3, 1.5));
            Assert.AreEqual("(3, 1.5)", c.ToString());
        }

        [TestMethod]
        public void CanOffset()
        {
            Coordinate c = new (3, 1.5);
            c = c.Offset(5, -8);
            Assert.IsTrue(new Coordinate(8, -6.5) == c);
        }

        [TestMethod]
        public void CanOffsetByCoordinate()
        {
            Coordinate c = new (3, 1.5);
            Coordinate dc = new (5, -8);
            c = c.Offset(dc);
            Assert.IsTrue(new Coordinate(8, -6.5) == c);
        }

        [TestMethod]
        public void CanScale()
        {
            Coordinate c = new (3, 1.5);
            c = c.Scale(3.5);
            Assert.IsTrue(new Coordinate(10.5, 5.25) == c);
        }

        [TestMethod]
        public void CanAdd()
        {
            Coordinate c = new (3, 1.5);
            Coordinate dc = new (5, -8);
            c += dc;
            Assert.IsTrue(new Coordinate(8, -6.5) == c);
        }

        [TestMethod]
        public void CanSubtract()
        {
            Coordinate c = new (3, 1.5);
            Coordinate dc = new (5, -8);
            c -= dc;
            Assert.IsTrue(new Coordinate(-2, 9.5) == c);
        }

        [TestMethod]
        public void CanNegate()
        {
            Coordinate c = new (3, 1.5);
            Coordinate dc = -c;
            Assert.IsTrue(new Coordinate(-3, -1.5) == dc);
        }

        [TestMethod]
        public void ConvertsFromPolar()
        {
            Coordinate c = Coordinate.FromPolar(1.0, Math.PI / 6);
            double cos30 = Math.Sqrt(3) / 2;
            Assert.AreEqual(cos30, c.X, 1e-7);
            Assert.AreEqual(0.5, c.Y, 1e-7);
        }

        [TestMethod]
        public void Conjugates()
        {
            Coordinate c = new (3, 1.5);
            Assert.AreEqual(new Coordinate(3, -1.5), c.Conjugate);
        }

        [TestMethod]
        public void CorrectMagnitude()
        {
            Coordinate c = new (3, 4);
            Assert.AreEqual(5, c.Magnitude);
        }

        [TestMethod]
        public void CorrectPhase()
        {
            Coordinate c = new (-1, -1);
            Assert.AreEqual(Math.PI * -0.75, c.Phase);
        }

        [TestMethod]
        public void CorrectGradients()
        {
            Coordinate c = new (4, -5);
            Assert.AreEqual(-1.25, c.Gradient);
            c = new (0, 1);
            Assert.AreEqual(double.MaxValue, c.Gradient);
            c = new (0, -1);
            Assert.AreEqual(double.MinValue, c.Gradient);
        }

        [TestMethod]
        public void Rotates()
        {
            Coordinate c = new (1, 1);
            c = c.Rotate(Math.PI / 4);
            Assert.AreEqual(Math.Sqrt(2), c.Y, 1e-6);
            Assert.AreEqual(0, c.X, 1E-6);
        }

        [TestMethod]
        public void RotatesAbout()
        {
            Coordinate c = new (2, 2);
            Coordinate o = new (1, 1);
            c = c.RotateAbout(o, Math.PI / 4);
            Assert.AreEqual(1 + Math.Sqrt(2), c.Y);
            Assert.AreEqual(1, c.X);
        }
    }
}