﻿using System;
using System.ComponentModel.DataAnnotations;

namespace GearWeb.Shared
{
    public class CycloidParams
    {
        [Required]
        [Range(6, 1000, ErrorMessage = "Must be 6 or more")]
        public int Teeth { get; set; }

        [Required]
        [Range(6, 1000, ErrorMessage = "Must be 6 or more")]
        public int OpposingTeeth { get; set; }

        [Required]
        [RegularExpression(@"\d+(\.\d\d?)?", ErrorMessage = "Positive number to nearest 100th of a mm")]
        public string Tolerance { get; set; }

        [Required]
        [RegularExpression(@"[1-9]\d*(\.\d+)?", ErrorMessage = "Value >= 1")]
        public string ContactRatio { get; set; }

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
