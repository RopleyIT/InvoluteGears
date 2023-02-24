using InvoluteGears;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using TwoDimensionLib;

namespace GearTests
{
    [TestClass]
    public class CycloidTests
    {
        [DataTestMethod]
        [DataRow(8, 12, 6.0, 0.01, 0.1, 6.35)]
        [DataRow(96, 12, 6.0, 0.01, 0.1, 6.35)]
        [DataRow(12, 96, 6.0, 0.01, 0.1, 6.35)]
        [DataRow(12, 10, 6.0, 0.01, 0.1, 6.35)]
        [DataRow(14, 10, 6.0, 0.01, 0.1, 6.35)]
        [DataRow(10, 12, 6.0, 0.01, 0.1, 6.35)]
        [DataRow(10, 14, 6.0, 0.01, 0.1, 6.35)]
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
            DrawableSet gearPoints = gear.GenerateGearCurves();
            Assert.IsNotNull(gearPoints);
            Assert.AreEqual(1, gearPoints.Paths.Count);
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
                blunting / 100.0,     // Tooth blunting
                otherBlunting / 100.0,// Opposite tooth blunting
                module,             // Module
                tolerance,          // Tolerance for point count reduction
                backlash,           // Backlash measured in fractions of the module
                cutterDiameter      // Cutter diameter
            );

            Assert.IsNotNull(gear);
            DrawablePath gearPoints = gear.GenerateGearCurves().Paths[0];
            Assert.IsNotNull(gearPoints);
            Assert.IsTrue(gearPoints.Curves.Count > 0);
        }
    }
}