using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OuterWildsRPG.External
{
    public interface ICustomShiplogModesAPI
    {
        public void AddMode(ShipLogMode mode, Func<bool> isEnabledSupplier, Func<string> nameSupplier);
    }
}
