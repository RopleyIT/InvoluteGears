## Involute Gear Calculator
This application is used to calculate parameters for involute spur gears, 
and to draw their outlines. It can be used to prepare SVG files for 
export to CAD tools or CNC machines. The gear calculator is a command line
or shell script application, that uses the command line for input, and
either the standard output or a text file for computed results. It is
invoked as follows:

`gears -[option] [arguments]`

Every command has one option flag as a single, case sensitive letter preceded by
a dash character. The arguments that follow the option depend in type and number 
on which option has been selected.

#### Options -p and -P

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
| `backlash` | 10ths of a percent of the module | Adjustment made to allow backlash in the gear. |
| `cutter-diameter` | 100ths of a mm | Diameter of end-mill used to cut gear teeth. |

Example: `gears -P 12 0 10 200 500 20 500`

This will design a 12 tooth gear with no profile shift, curves accurate to 1/10th mm,
with the standard 20 degree pressure angle, a module of 5mm, backlash of 1/10th mm,
using a cutter of diameter 5mm to cut out the gear.

Note that the current version of this program will generate spokes 
for the gears in order to reduce their 
weight and moment of inertia. For teeth up to around 17 teeth no spokes are generated. For
gears above that, the number of spokes is chosen so that no more that
eight teeth would be on the perimeter between consecutive spokes. The
minimum number of spokes is three.

#### Option -m

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

#### Option -C

`gears -C [output-file-path]`

Creates a table of data for a range of gears 
with various pressure angles, tooth counts,
and profile shifts. The results are written to the file given 
in the output file path. For each tooth it tabulates the following data:

| Item | Description |
| ---- | ----------- |
| GAP | The linear distance between the two undercut points on adjacent teeth of the gear. Used to tell you the maximum diameter of the end-mill cutter to use. |
| Db | The base circle diameter from which the involute tooth curve starts. |
| Dd | The dedendum circle diameter. This is the innermost depth the tooth is cut to, if we were able to use a small end-mill. |
| Dc | The reduced dedendum diameter caused by using a cutter of diameter too great to follow the ideal undercut curve. |
| Du | The diameter at which the undercut begins to eat into the involute curve. |
| Dp | The pitch circle diameter for the tooth. This is usually `module * tooth-count`. |
| Da | The addendum diameter of the gear. This is the outermost diameter and includes any extra diameter introduced by profile shifting. |

Following this list, a table appears that shows 
for each pair of gears cut to this profile-shift and pressure angle,
what the contact ratio is between two gears with 
their respective numbers of teeth. All combinations
of tooth pairs from the range 10, 12, 14, 16, 18, 24, 30, 36, 48, 72, 144 are plotted.

The contact ratio is defined as the 
average number of teeth that are in contact on the
involute part of their gear surfaces at 
any one time. If less than 1, the gear tips will be grinding
on the undercut parts for some of their 
rotation, and will run unevenly and noisily.
Ideally this ratio should be above about 1.1 
for smooth handover between teeth.

#### Option -c

`gears -c [output-file-path] [angle1,angle2 ... angleN] [teeth1,teeth2 ... teethM] [module] [cutter-diameter]`

Creates a table of data for the gears whose 
pressure angles and tooth counts have been specified
in the comma-separated arguments after the output 
filename. The comma-separated arguments must not 
have any embedded spaces.

Table data is as for the `-C` option
described above, but with selected pressure 
angles, tooth counts, module, and cutter diameter. The pressure
angles are specified in tenths of a degree, 
and the module and cutter diameter in 100ths of a mm.

For the `-C` option, we assume a sufficiently small
diameter cutter so that the undercut and dedendum do not need 
to be adjusted from the ideal curve. Also for the
`-C` option, we assume the module is 1.0mm as a working default.

Example: `gears -c geardata.txt 145,200 10,12,15,45,48,50 500 635`

Computes the tables for the 14.5 and 20 degree pressure angles, for all gear tooth counts in the
list of values supplied as the second parameter. The gears have a module of 5mm, and
are cut using an end mill of diameter 1/4 inch (6.35mm). Contact ratios are between the gears specified in 
the list of tooth counts.

