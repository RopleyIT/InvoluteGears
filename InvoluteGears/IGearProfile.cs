using System;
using System.Collections.Generic;
using System.Text;

namespace InvoluteGears
{
    /// <summary>
    /// Interface used when computing
    /// the shape of gear cutouts
    /// </summary>
    
    public interface IGearProfile
    {
        int ToothCount { get; }
        double Module { get; }
        double MaxError { get; }
        double InnerDiameter { get; }
    }
}
