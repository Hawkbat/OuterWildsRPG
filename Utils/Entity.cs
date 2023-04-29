using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OuterWildsRPG.Utils
{
    public abstract class Entity<TThis, TData> : EntityLike<TThis, TData> where TThis : Entity<TThis, TData>, new() where TData : EntityData, new()
    {
        public string ModID;
        public string ID;
        public string Name;
        public string FullID => $"{ModID}/{ID}";

        public override void Load(TData data, string modID)
        {
            ModID = modID;
            ID = data.id;
            Name = data.name;
        }

        public abstract void Resolve(TData data, Dictionary<string, TThis> entities);
    }
}
