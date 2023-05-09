using OuterWildsRPG.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OuterWildsRPG.Objects.Common.Effects
{
    public abstract class BuffEffect<TThis, TData> : EntityLike<TThis, TData>, IBuffEffect where TThis : BuffEffect<TThis, TData>, new() where TData : EntityLikeData, new()
    {
        public Buff Buff;

        Buff IBuffEffect.Buff { get => Buff; set => Buff = value; }

        public abstract bool IsInstant();
        public abstract string GetDescription();
    }

    public interface IBuffEffect : IEntityLike
    {
        Buff Buff { get; set; }
        bool IsInstant();
        string GetDescription();
    }
}
