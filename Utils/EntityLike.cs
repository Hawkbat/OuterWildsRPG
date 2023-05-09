using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OuterWildsRPG.Utils
{
    public abstract class EntityLike<TThis, TData> : IEntityLike where TThis : EntityLike<TThis, TData>, new() where TData : EntityLikeData, new()
    {
        private TData data;

        public TData GetRawData() => data;

        public virtual void Load(TData data, string modID)
        {
            this.data = data;
        }

        public virtual void Resolve() { }

        public static TThis LoadNew(TData data, string modID)
        {
            if (data == null)
                return null;
            TThis v = new();
            v.Load(data, modID);
            return v;
        }
    }

    public interface IEntityLike
    {
        void Resolve();
    }
}
