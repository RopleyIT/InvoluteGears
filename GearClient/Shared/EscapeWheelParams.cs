using System;
using System.Collections.Generic;
using System.Text;
using System.ComponentModel.DataAnnotations;

namespace GearClient.Shared
{
    public class EscapeWheelParams
    {
        [Range(6, 1000, ErrorMessage = "Must be 6 or more")]
        public int Teeth { get; set; }
        [Range(0, 100000, ErrorMessage = "Zero or more, measured in 100ths of a mm")]
        public int Tolerance { get; set; }
        [Range(0, 600, ErrorMessage = "Zero to 600, measured in 10ths of a degree")]
        public int UndercutAngle { get; set; }
        [Range(1, 100000, ErrorMessage = "One or more, measured in 100ths of a mm")]
        public int Module { get; set; }
        [Range(1, 100000, ErrorMessage = "One or more, measured in 100ths of a mm")]
        public int FaceLength { get; set; }
        [Range(1, 100000, ErrorMessage = "One or more, measured in 100ths of a mm")]
        public int TipPitch { get; set; }
        [Range(1, 100000, ErrorMessage = "One or more, measured in 100ths of a mm")]
        public int BaseDiameter { get; set; }
        [Range(0, 100000, ErrorMessage = "Zero or more, measured in 100ths of a mm")]
        public int SpindleDiameter { get; set; }
        [Range(0, 100000, ErrorMessage = "Zero or more, measured in 100ths of a mm")]
        public int InlayDiameter { get; set; }
        [Range(0, 100000, ErrorMessage = "Zero or more, measured in 100ths of a mm")]
        public int KeyFlatWidth { get; set; }
    }
}
