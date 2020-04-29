## Involute Gear Calculator
This application is used to calculate parameters for involute spur gears, 
and to draw their outlines.
It can be used to prepare SVG files for export to CAD tools or CNC machines.

### Command line and parameters

`gears -[option] [arguments]`

Every command has a single option flag as a single, case sensitive letter.
The arguments that follow the option depend in type and number on which option
has been selected.

`gears -p|-P [tooth-count] [profile-shift] [tolerance] [angle] [module] [backlash] [cutter-diameter]`

Option `-p` generates an SVG file for the whole gear, 
as well as a PNG image file of the whole gear for graphical inspection.

Option `-P` generates the same SVG file, but generates a single tooth and gap in an image file
so that it can be zoomed closely to inspect the tooth profile.

| Argument | Units | Description |
| -------- | ---- | ----------- |
| `tooth-count` | Number | The number of teeth this gear will have. |
| `profile-shift` | 1000ths of module | The outward shift of the gear profile. |
| `tolerance` | 100ths of a mm | The tolerated deviation from exact measures of points on the gear curve. |
| `angle` | 10ths of a degree | The pressure angle for the gear. Usually 145, 200, or 250. |
| `module` | 100ths of a mm | The gear module. Gear diameter = `tooth-count * module` at the pitch circle. |
| `backlash` | 1000ths of the module | Adjustment made to allow backlash in the gear. |
| `cutter-diameter` | 100ths of a mm | Diameter of end-mill used to cut gear teeth. |

Example: `gears -P 12 0 10 200 500 20 500`

This will design a 12 tooth gear with no profile shift, curves accurate to 1/10th mm,
with the standard 20 degree pressure angle, a module of 5mm, backlash of 1/10th mm,
using a cutter of diameter 5mm to cut out the gear.

`gears -m [numerator] [denominator] [min-teeth] [max-teeth]`

Sometimes we need to map a gear ratio back onto the same axle using two gears. For example,
this happens in a clock movement where the hour hand must rotate at one twelfth
the rate of the minute hand, but is coaxial with it. Two gears are used to achieve 
the 1/12th reduction, using the two gear ratios 1/4 and 1/3 for example. This option
allows you to find the number of teeth on both the gear and the pinion for the two gears
so that they have the same axle separations, for any overall gear ratio of numerator/denominator.

| Argument | Description |
| -------- | ----------- |
| `numerator` | The numerator of the numerator and denominator pair that define the overall gear ratio to be achieved by the two gears. |
| `denominator` | The denominator. The ratio of gearing will be numerator/denominator. |
| `min-teeth` | The minimum number of teeth you would tolerate on a pinion when searching for gears. |
| `max-teeth` | The maximum number of teeth, at which the search ceases. |

Example: `gears -m 1 60 10 96`

This would search for all combinations of pinion and gear tooth counts for pairs of gears
that together have the same axle separation, but that also give an overall ratio of
1/60th. No tooth or pinion will have fewer than 10 teeth, nor more than 92 teeth.

`gears -C [output-file-path]`

Creates a table of data for a range of gears with various pressure angles, tooth counts,
and profile shifts. The results are written to the file given in the output file path. For each tooth it tabulates the following data:

| Item | Description |
| ---- | ----------- |
| GAP | The linear distance between the two undercut points on adjacent teeth of the gear. Used to tell you the maximum diameter of the end-mill cutter to use. |
| Db | The base circle diameter from which the involute tooth curve starts. |
| Dd | The dedendum circle diameter. This is the innermost depth the tooth is cut to, if we were able to use a small end-mill. |
| Du | The radius at which the undercut begins to eat into the involute curve. |
| Dp | The pitch circle diameter for the tooth. This is usually `module * tooth-count`. |
| Da | The addendum diameter of the gear. This is the outermost diameter and includes any extra diameter introduced by profile shifting. |

Following this list, a table appears that shows for each pair of gears cut to this profile-shift and pressure angle,
what the contact ratio is between two gears with their respective numbers of teeth. All combinations
of tooth pairs from the range 10, 12, 14, 16, 18, 24, 30, 36, 48, 72, 144 are plotted.

The contact ratio is defined as the average number of teeth that are in contact on the
involute part of their gear surfaces at any one time. If less than 1, the gear tips will be grinding
on the undercut parts for some of their rotation, and will run unevenly and noisily.
Ideally this ratio should be above about 1.1 for smooth handover between teeth.

`gears -c [output-file-path] [angle1,angle2 ... angleN] [teeth1,teeth2 ... teethM]`

Creates a table of data for the gears whose pressure angles and tooth counts have been specified
in the comma-separated arguments after the output filename. Table data is as for the `-C` option
described above, but with selected pressure angles and tooth counts. Note that the pressure
angles are specified in tenths of a degree.

Example: `gears -c geardata.txt 145,200 10,12,15,45,48,50`

Computes the tables for the 14.5 and 20 degree pressure angles, for all gear tooth counts in the
list of values supplied as the second parameter. Contact ratios are between the gears specified in 
the list of tooth counts.

