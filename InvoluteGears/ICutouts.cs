using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TwoDimensionLib;

namespace InvoluteGears;

public interface ICutouts
{
    IList<DrawablePath> CalculateCutouts(IGearProfile gear);
}
