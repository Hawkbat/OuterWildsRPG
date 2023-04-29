using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OuterWildsRPG.Utils
{
    public abstract class MultipleEntityData<T> : EntityLikeData where T: EntityData
    {
        public abstract IEnumerable<T> GetEntities();
    }
}
