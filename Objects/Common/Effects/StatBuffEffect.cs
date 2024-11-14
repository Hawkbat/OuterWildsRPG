using OuterWildsRPG.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OuterWildsRPG.Objects.Common.Effects
{
    public abstract class StatBuffEffect<TThis, TData> : BuffEffect<TThis, TData>, IStatBuffEffect where TThis : BuffEffect<TThis, TData>, new() where TData : EntityLikeData, new()
    {
        public float Add = 0f;
        public float Multiply = 1f;

        float IStatBuffEffect.Add => Add;
        float IStatBuffEffect.Multiply => Multiply;
    }

    public interface IStatBuffEffect
    {
        public float Add { get; }
        public float Multiply { get; }
    }
}
