using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OuterWildsRPG.Utils
{
    public abstract class EntityLike<TThis, TData> where TThis : EntityLike<TThis, TData>, new() where TData : EntityLikeData, new()
    {
        public abstract void Load(TData data, string modID);

        public static TThis LoadNew(TData data, string modID)
        {
            if (data == null)
                return null;
            TThis v = new();
            v.Load(data, modID);
            return v;
        }
    }
}
