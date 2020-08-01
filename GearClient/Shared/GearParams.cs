using System;
using System.ComponentModel.DataAnnotations;

namespace GearClient.Shared
{
    public class GearParams
    {
        [Required]
        [Range(6, 1000, ErrorMessage = "Must be 6 or more")]
        public int Teeth { get; set; }

        [Required]
        [RegularExpression(@"-?\d?\d(\.\d)?", ErrorMessage = "Number to nearest 10th of a %")]
        public string ProfileShift { get; set; }

        [Required]
        [RegularExpression(@"\d+(\.\d\d?)?", ErrorMessage = "Positive number to nearest 100th of a mm")]
        public string Tolerance { get; set; }

        [Required]
        [RegularExpression(@"\d+(\.\d)?", ErrorMessage = "Positive angle to nearest 10th of a degree")]
        public string PressureAngle { get; set; }

        [Required]
        [RegularExpression(@"\d+(\.\d\d?)?", ErrorMessage = "Number > 0 to nearest 100th of a mm")]
        public string Module { get; set; }

        [Required]
        [RegularExpression(@"\d+(\.\d\d?)?", ErrorMessage = "Positive number to nearest 100th of a mm")]
        public string Backlash { get; set; }

        [Required]
        [RegularExpression(@"\d+(\.\d\d?)?", ErrorMessage = "Positive number to nearest 100th of a mm")]
        public string CutterDiameter { get; set; }

        [Required]
        [RegularExpression(@"\d+(\.\d\d?)?", ErrorMessage = "Positive number to nearest 100th of a mm")]
        public string SpindleDiameter { get; set; }

        [Required]
        [RegularExpression(@"\d+(\.\d\d?)?", ErrorMessage = "Positive number to nearest 100th of a mm")]
        public string InlayDiameter { get; set; }

        [Required]
        [RegularExpression(@"\d+(\.\d\d?)?", ErrorMessage = "Positive number to nearest 100th of a mm")]
        public string KeyFlatWidth { get; set; }
    }
}
