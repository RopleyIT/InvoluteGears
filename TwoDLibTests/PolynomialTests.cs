using Microsoft.VisualStudio.TestTools.UnitTesting;
using TwoDimensionLib;

namespace TwoDLibTests
{
    [TestClass]
    public class PolynomialTests
    {
        [TestMethod]
        public void CanCreateZeroPoly()
        {
            Polynomial zero = Polynomial.Zero();
            Assert.AreEqual(0.0, zero[0]);
            Assert.AreEqual(-1, zero.Order);
        }

        [TestMethod]
        public void CanCreateOnePoly()
        {
            Polynomial one = Polynomial.One();
            Assert.AreEqual(1.0, one[0]);
            Assert.AreEqual(0, one.Order);
        }

        [TestMethod]
        public void CanCreateMultiTermPoly()
        {
            Polynomial y = new (-1.0, 0.0, 3.5, 4.2);
            Assert.AreEqual(4.2, y[3]);
            Assert.AreEqual(-1.0, y[0]);
            Assert.AreEqual(3, y.Order);
        }

        [TestMethod]
        public void CanScalePoly()
        {
            Polynomial y = new (-1.0, 0.0, 3.5, 4.2);
            Polynomial sy = y.Scale(-2);
            Assert.AreEqual(-8.4, sy[3]);
            Assert.AreEqual(2.0, sy[0]);
            Assert.AreEqual(3, sy.Order);
        }

        [TestMethod]
        public void ScaleByZeroAdjustsOrder()
        {
            Polynomial y = new (-1.0, 0.0, 3.5, 4.2);
            Polynomial sy = y.Scale(0);
            Assert.AreEqual(0.0, sy[0]);
            Assert.AreEqual(-1, sy.Order);
        }

        [TestMethod]
        public void IndexerFunctionsCorrectly()
        {
            Polynomial p = Polynomial.Zero();
            p[3] = 3.3;
            Assert.AreEqual(3.3, p[3]);
            Assert.AreEqual(3, p.Order);
            p[7] = 7.7;
            Assert.AreEqual(7.7, p[7]);
            Assert.AreEqual(3.3, p[3]);
            Assert.AreEqual(7, p.Order);
            p[7] = 0.0;
            Assert.AreEqual(3.3, p[3]);
            Assert.AreEqual(3, p.Order);
            p[3] = 0.0;
            Assert.AreEqual(0.0, p[0]);
            Assert.AreEqual(-1, p.Order);
        }

        [TestMethod]
        public void CanEvaluate()
        {
            Polynomial y = new (-1.0, 0.0, 3.5, 4.2);
            double v = y.Evaluate(2.1);
            Assert.AreEqual(53.3312, v, 0.0000001);
        }

        [TestMethod]
        public void CanMap()
        {
            Polynomial y = new (-1.0, 0.0, 3.5, 4.2);
            Polynomial m = new (-1.0, 0.0, 2.0);
            Polynomial z = y.Transform(m);
            Assert.AreEqual(6, z.Order);
            Assert.AreEqual(33.6, z[6], 0.00000001);
            Assert.AreEqual(0.0, z[5]);
            Assert.AreEqual(-36.4, z[4], 0.00000001);
            Assert.AreEqual(0.0, z[3]);
            Assert.AreEqual(11.2, z[2], 0.00000001);
            Assert.AreEqual(0.0, z[1]);
            Assert.AreEqual(-1.7, z[0], 0.00000001);
        }

        [TestMethod]
        public void CanMultiply()
        {
            Polynomial y = new (-1.0, 0.0, 3.5, 4.2);
            Polynomial m = new (-1.0, 0.0, 2.0);
            Polynomial z = y.MultiplyBy(m);
            Assert.AreEqual(5, z.Order);
            Assert.AreEqual(8.4, z[5], 0.00000001);
            Assert.AreEqual(7.0, z[4], 0.00000001);
            Assert.AreEqual(-4.2, z[3], 0.00000001);
            Assert.AreEqual(-5.5, z[2], 0.00000001);
            Assert.AreEqual(0.0, z[1]);
            Assert.AreEqual(1.0, z[0], 0.00000001);
        }

        [TestMethod]
        public void CanRaiseToIntegerPower()
        {
            Polynomial y = new (-3.0, 2.0, 4.0);
            Polynomial z = y.Power(3);
            Assert.AreEqual(6, z.Order);
            Assert.AreEqual(64.0, z[6], 0.00000001);
            Assert.AreEqual(96.0, z[5], 0.00000001);
            Assert.AreEqual(-96.0, z[4], 0.00000001);
            Assert.AreEqual(-136.0, z[3], 0.00000001);
            Assert.AreEqual(72.0, z[2], 0.00000001);
            Assert.AreEqual(54.0, z[1], 0.00000001);
            Assert.AreEqual(-27.0, z[0], 0.00000001);
        }

        [TestMethod]
        public void CanAdd()
        {
            Polynomial y = new (-3.3, 0.0, 2.2);
            Polynomial w = new (1.0, 1.1, 0.4, -22);
            Polynomial z = y.Plus(w);
            Assert.AreEqual(3, z.Order);
            Assert.AreEqual(-22.0, z[3], 0.00000001);
            Assert.AreEqual(2.6, z[2], 0.00000001);
            Assert.AreEqual(1.1, z[1], 0.00000001);
            Assert.AreEqual(-2.3, z[0], 0.00000001);
        }

        [TestMethod]
        public void CanSubtract()
        {
            Polynomial y = new (-3.3, 0.0, 2.2);
            Polynomial w = new (1.0, 1.1, 0.4, -22);
            Polynomial z = y.Minus(w);
            Assert.AreEqual(3, z.Order);
            Assert.AreEqual(22.0, z[3], 0.00000001);
            Assert.AreEqual(1.8, z[2], 0.00000001);
            Assert.AreEqual(-1.1, z[1], 0.00000001);
            Assert.AreEqual(-4.3, z[0], 0.00000001);
        }

        [TestMethod]
        public void CanSubtractAndNormalise()
        {
            Polynomial y = new (-3.3, 0.0, 2.2, -22);
            Polynomial w = new (1.0, 1.1, 0.4, -22);
            Polynomial z = y.Minus(w);
            Assert.AreEqual(2, z.Order);
            Assert.AreEqual(0.0, z[3], 0.00000001);
            Assert.AreEqual(1.8, z[2], 0.00000001);
            Assert.AreEqual(-1.1, z[1], 0.00000001);
            Assert.AreEqual(-4.3, z[0], 0.00000001);
        }

        [TestMethod]
        public void CanCalculateCombination()
        {

            int nCr = Polynomial.Combination(3, 7);
            Assert.AreEqual(35, nCr);
            nCr = Polynomial.Combination(4, 7);
            Assert.AreEqual(35, nCr);
            nCr = Polynomial.Combination(7, 7);
            Assert.AreEqual(1, nCr);
            nCr = Polynomial.Combination(0, 7);
            Assert.AreEqual(1, nCr);
            nCr = Polynomial.Combination(1, 7);
            Assert.AreEqual(7, nCr);
        }

        [TestMethod]
        public void ComputesBernsteinCoefficients()
        {
            Polynomial p = new (-2.0, 3.0, 1.0, 4.0);
            Assert.AreEqual(-2.0, p.BernsteinCoefficient(0));
            Assert.AreEqual(-1.0, p.BernsteinCoefficient(1));
            Assert.AreEqual(1.0 / 3.0, p.BernsteinCoefficient(2));
            Assert.AreEqual(6.0, p.BernsteinCoefficient(3));
            Assert.AreEqual(0.0, p.BernsteinCoefficient(4));
        }
    }
}
