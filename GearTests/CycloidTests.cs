using Microsoft.VisualStudio.TestTools.UnitTesting;
using InvoluteGears;
using System;
using System.Collections.Generic;
using TwoDimensionLib;
using System.Linq;

namespace GearTests
{
    [TestClass]
    public class CycloidTests
    {
        [DataTestMethod]
        [DataRow(8, 12, 5.0, 0.01, 0.1, 6.35)]
        [DataRow(96, 12, 5.0, 0.01, 0.1, 6.35)]
        [DataRow(12, 96, 5.0, 0.01, 0.1, 6.35)]
        [DataRow(12, 10, 5.0, 0.01, 0.1, 6.35)]
        [DataRow(14, 10, 5.0, 0.01, 0.1, 6.35)]
        [DataRow(10, 12, 5.0, 0.01, 0.1, 6.35)]
        [DataRow(10, 14, 5.0, 0.01, 0.1, 6.35)]
        public void TestPairedTeeth(int teeth, int otherTeeth,
            double module, double tolerance,
            double backlash, double cutterDiameter)
        {
            CycloidalGear gear = new(
                teeth,              // Teeth
                otherTeeth,         // Opposing teeth
                0.0,                // No tooth blunting
                0.0,                // No opposite tooth blunting
                module,             // Module
                tolerance,          // Tolerance for point count reduction
                backlash,           // Backlash measured in fractions of the module
                cutterDiameter      // Cutter diameter
            );

            Assert.IsNotNull(gear);
            IList<Coordinate> gearPoints = gear.GenerateCompleteGearPath().ToList();
            Assert.IsNotNull(gearPoints);
            Assert.IsTrue(gearPoints.Count > 0);
        }
        [DataTestMethod]
        [DataRow(96, 12, 50, 50, 5.0, 0.01, 0.1, 6.35)]
        [DataRow(12, 96, 50, 50, 5.0, 0.01, 0.1, 6.35)]
        [DataRow(12, 10, 50, 50, 5.0, 0.01, 0.1, 6.35)]
        [DataRow(10, 12, 50, 50, 5.0, 0.01, 0.1, 6.35)]
        public void TestBluntedTeeth(int teeth, int otherTeeth,
            int blunting, int otherBlunting,
            double module, double tolerance,
            double backlash, double cutterDiameter)
        {
            CycloidalGear gear = new(
                teeth,              // Teeth
                otherTeeth,         // Opposing teeth
                blunting/100.0,     // Tooth blunting
                otherBlunting/100.0,// Opposite tooth blunting
                module,             // Module
                tolerance,          // Tolerance for point count reduction
                backlash,           // Backlash measured in fractions of the module
                cutterDiameter      // Cutter diameter
            );

            Assert.IsNotNull(gear);
            IList<Coordinate> gearPoints = gear.GenerateCompleteGearPath().ToList();
            Assert.IsNotNull(gearPoints);
            Assert.IsTrue(gearPoints.Count > 0);
        }
    }
}