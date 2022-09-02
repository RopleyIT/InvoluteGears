using CmdArgs;

namespace Gears
{
    [ArgSet]
    [Description("Arguments specific to the design of escapement wheels")]
    internal class EscapeArgs
    {
        [Required]
        [Arg("--undercut")]
        [Arg("-ua")]
        [Description("The angle of slope backwards relative to a "
            + "radial line of the facing edge of teeth")]
        public double UndercutAngle { get; set; }

        [Required]
        [Arg("--facelength")]
        [Arg("-fl")]
        [Description("The length of the flat facing edge of each tooth")]
        public double FaceLength { get; set; }

        [Required]
        [Arg("--tippitch")]
        [Arg("-tp")]
        [Description("The circumferential length of the 'flat' tip of each tooth")]
        public double TipPitch { get; set; }

        [Required]
        [Arg("--basediameter")]
        [Arg("-bd")]
        [Description("The diameter of the arc joining the base of one tooth to the back of the next")]
        public double BaseDiameter { get; set; }
    }
}
