﻿using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using TwoDimensionLib;

namespace TwoDLibTests
{
    [TestClass]
    public class TchebyshevTests
    {
        [TestMethod]
        public void CanCreate()
        {
            TchebyshevApproximator t = new TchebyshevApproximator(3, v => v * v + 3.0, -3, 3);
            Assert.IsInstanceOfType(t, typeof (TchebyshevApproximator));
            Assert.AreEqual(-3.0, t.MinimumValue);
            Assert.AreEqual(3.0, t.MaximumValue);
            Assert.AreEqual(3, t.Degree);
            Assert.AreEqual(7.0, t.Function(2.0));
        }

        private double Func(double x)
            => x * x * x * 1.0 / 3.0 + x * x * 2 + x - 10;

        [TestMethod]
        public void ApproximatesExactPoly()
        {
            TchebyshevApproximator t = new TchebyshevApproximator(5, Func, -1.0, 3.0);
            Polynomial p = t.ApproximationPolynomial;
            Assert.AreEqual(5, p.Order);
            Assert.AreEqual(0.0, p[5], 0.00000001);
            Assert.AreEqual(0.0, p[4], 0.00000001);
            Assert.AreEqual(1.0 / 3.0, p[3], 0.00000001);
            Assert.AreEqual(2.0, p[2], 0.00000001);
            Assert.AreEqual(1.0, p[1], 0.00000001);
            Assert.AreEqual(-10.0, p[0], 0.00000001);
        }

        [TestMethod]
        public void ApproximatesReducedPoly()
        {
            TchebyshevApproximator t = new TchebyshevApproximator(2, Func, -1.0, 3.0);
            Polynomial p = t.ApproximationPolynomial;
            Assert.AreEqual(2, p.Order);
            Assert.AreEqual(3.0, p[2], 0.00000001);
            Assert.AreEqual(1.0, p[1], 0.00000001);
            Assert.AreEqual(-32.0/3.0, p[0], 0.00000001);
        }
    }
}
