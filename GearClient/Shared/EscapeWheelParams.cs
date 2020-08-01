using System;
using System.ComponentModel.DataAnnotations;

namespace GearClient.Shared
{
    public class EscapeWheelParams
    {
        [Required]
        [Range(6, 1000, ErrorMessage = "Must be 6 or more")]
        public int Teeth { get; set; }

        [Required]
        [RegularExpression(@"\d+(\.\d)?", ErrorMessage = "Positive angle to nearest 10th of a degree")]
        public string UndercutAngle { get; set; }

        [Required]
        [RegularExpression(@"\d+(\.\d\d?)?", ErrorMessage = "Positive number to nearest 100th of a mm")]
        public string Tolerance { get; set; }

        [Required]
        [RegularExpression(@"\d+(\.\d\d?)?", ErrorMessage = "Number > 0 to nearest 100th of a mm")]
        public string Module { get; set; }

        [Required]
        [RegularExpression(@"\d+(\.\d\d?)?", ErrorMessage = "Number > 0 to nearest 100th of a mm")]
        public string FaceLength { get; set; }

        [Required]
        [RegularExpression(@"\d+(\.\d\d?)?", ErrorMessage = "Number > 0 to nearest 100th of a mm")]
        public string TipPitch { get; set; }

        [Required]
        [RegularExpression(@"\d+(\.\d\d?)?", ErrorMessage = "Number > 0 to nearest 100th of a mm")]
        public string BaseDiameter { get; set; }

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
