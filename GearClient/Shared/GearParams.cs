using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Numerics;
using System.Text;

namespace GearClient.Shared
{
    public class GearParams
    {
        [Range(6, 1000, ErrorMessage = "Must be 6 or more")]
        public int Teeth { get; set; }
        [Range(0, 1000, ErrorMessage = "Zero or more, measured in 10ths of a %")]
        public int ProfileShift { get; set; }
        [Range(0, 100000, ErrorMessage = "Zero or more, measured in 100ths of a mm")]
        public int Tolerance { get; set; }
        [Range(100, 600, ErrorMessage = "100 to 600, measured in 10ths of a degree")]
        public int PressureAngle { get; set; }
        [Range(1, 100000, ErrorMessage = "One or more, measured in 100ths of a mm")]
        public int Module { get; set; }
        [Range(0, 100000, ErrorMessage = "Zero or more, measured in 100ths of a mm")]
        public int Backlash { get; set; }
        [Range(0, 100000, ErrorMessage = "Zero or more, measured in 100ths of a mm")]
        public int CutterDiameter { get; set; }
        [Range(0, 100000, ErrorMessage = "Zero or more, measured in 100ths of a mm")]
        public int SpindleDiameter { get; set; }
        [Range(0, 100000, ErrorMessage = "Zero or more, measured in 100ths of a mm")]
        public int InlayDiameter { get; set; }
        [Range(0, 100000, ErrorMessage = "Zero or more, measured in 100ths of a mm")]
        public int KeyFlatWidth { get; set; }

    }
}
