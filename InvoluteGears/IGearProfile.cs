﻿using System.Collections.Generic;
using System.Drawing;

namespace InvoluteGears
{
    /// <summary>
    /// Interface used when computing
    /// the shape of gear cutouts
    /// </summary>

    public interface IGearProfile
    {
        string Information { get; }
        string ShortName { get; }
        int ToothCount { get; }
        double Module { get; }
        double MaxError { get; }
        double InnerDiameter { get; }
        double CutDiameter { get; }
        IEnumerable<PointF> GenerateCompleteGearPath();
    }
}
