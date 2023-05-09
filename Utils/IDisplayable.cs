using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OuterWildsRPG.Utils
{
    public interface IDisplayable
    {
        string ToDisplayString(bool richText = true);
    }
}
