using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using TwoDimensionLib;

namespace TwoDLibTests
{
    [TestClass]
    public class AngleTests
    {
        [TestMethod]
        public void CanCreateZeroAngle()
        {
            Angle a = new();
            Assert.AreEqual(0.0, a.Degrees);
        }

        [TestMethod]
        public void ConvertsRadiansDegrees()
        {
            Angle a = new()
            {
                Degrees = 23.54
            };
            Assert.AreEqual(23.54 * Math.PI / 180.0, a.Radians);
            a = new()
            {
                Radians = 1.0
            };
            Assert.AreEqual(180 / Math.PI, a.Degrees);
        }

        [TestMethod]
        public void CanDoTrig()
        {
            Angle a = new()
            {
                Degrees = 30
            };
            Assert.AreEqual(0.5, a.Sin, 1e-6);
            Assert.AreEqual(Math.Sqrt(3) / 2, a.Cos, 1e-6);
            Assert.AreEqual(1 / Math.Sqrt(3), a.Tan, 1e-6);
            Assert.AreEqual(2, a.Cosec, 1e-6);
            Assert.AreEqual(2 / Math.Sqrt(3), a.Sec, 1e-6);
            Assert.AreEqual(Math.Sqrt(3), a.Cot, 1e-6);
        }

        [TestMethod]
        public void CanAdd()
        {
            Angle a = new()
            {
                Degrees = 30
            };
            Angle b = new()
            {
                Degrees = 45
            };
            Assert.AreEqual(75, (a + b).Degrees, 1e-6);
            Assert.AreEqual(-15, (a - b).Degrees, 1e-6);
        }

        [TestMethod]
        public void CanNegate()
        {
            Angle a = new()
            {
                Degrees = 30
            };
            Assert.AreEqual(-30, -a.Degrees, 1e-6);
        }

        [TestMethod]
        public void ACosWorks()
        {
            Angle a = Angle.ACos(0.5);
            Angle b = Angle.ACos(-0.5);
            Assert.AreEqual(60, a.Degrees, 1e-6);
            Assert.AreEqual(120, b.Degrees, 1e-6);
        }

        [TestMethod]
        public void ASecWorks()
        {
            Angle a = Angle.ASec(2);
            Angle b = Angle.ASec(-2);
            Assert.AreEqual(60, a.Degrees, 1e-6);
            Assert.AreEqual(120, b.Degrees, 1e-6);
        }

        [TestMethod]
        public void ASinWorks()
        {
            Angle a = Angle.ASin(0.5);
            Angle b = Angle.ASin(-0.5);
            Assert.AreEqual(30, a.Degrees, 1e-6);
            Assert.AreEqual(-30, b.Degrees, 1e-6);
        }

        [TestMethod]
        public void ACosecWorks()
        {
            Angle a = Angle.ACosec(2);
            Angle b = Angle.ACosec(-2);
            Assert.AreEqual(30, a.Degrees, 1e-6);
            Assert.AreEqual(-30, b.Degrees, 1e-6);
        }
        [TestMethod]
        public void ATanWorks()
        {
            Angle a = Angle.ATan(99, 99);
            Angle b = Angle.ATan(1);
            Angle c = Angle.ATan(99, -99);
            Angle d = Angle.ATan(-99, -99);
            Angle e = Angle.ATan(-99, 99);
            Angle f = Angle.ATan(-1);
            Assert.AreEqual(45, a.Degrees, 1e-6);
            Assert.AreEqual(45, b.Degrees, 1e-6);
            Assert.AreEqual(135, c.Degrees, 1e-6);
            Assert.AreEqual(-135, d.Degrees, 1e-6);
            Assert.AreEqual(-45, e.Degrees, 1e-6);
            Assert.AreEqual(-45, f.Degrees, 1e-6);

            double r3 = Math.Sqrt(3);
            Assert.AreEqual(30, Angle.ATan(1 / r3).Degrees, 1e-6);
            Assert.AreEqual(60, Angle.ATan(r3).Degrees, 1e-6);
            Assert.AreEqual(-30, Angle.ATan(-1 / r3).Degrees, 1e-6);
            Assert.AreEqual(-60, Angle.ATan(-r3).Degrees, 1e-6);

            Assert.AreEqual(30, Angle.ATan(1, r3).Degrees, 1e-6);
            Assert.AreEqual(60, Angle.ATan(r3, 1).Degrees, 1e-6);
            Assert.AreEqual(120, Angle.ATan(r3, -1).Degrees, 1e-6);
            Assert.AreEqual(150, Angle.ATan(1, -r3).Degrees, 1e-6);
            Assert.AreEqual(-30, Angle.ATan(-1, r3).Degrees, 1e-6);
            Assert.AreEqual(-60, Angle.ATan(-r3, 1).Degrees, 1e-6);
            Assert.AreEqual(-120, Angle.ATan(-r3, -1).Degrees, 1e-6);
            Assert.AreEqual(-150, Angle.ATan(-1, -r3).Degrees, 1e-6);
        }

        [TestMethod]
        public void ACotWorks()
        {
            Angle a = Angle.ACot(99, 99);
            Angle b = Angle.ACot(1);
            Angle c = Angle.ACot(99, -99);
            Angle d = Angle.ACot(-99, -99);
            Angle e = Angle.ACot(-99, 99);
            Angle f = Angle.ACot(-1);
            Assert.AreEqual(45, a.Degrees, 1e-6);
            Assert.AreEqual(45, b.Degrees, 1e-6);
            Assert.AreEqual(-45, c.Degrees, 1e-6);
            Assert.AreEqual(-135, d.Degrees, 1e-6);
            Assert.AreEqual(135, e.Degrees, 1e-6);
            Assert.AreEqual(135, f.Degrees, 1e-6);
        }
    }
}
