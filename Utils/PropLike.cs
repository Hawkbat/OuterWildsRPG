using OuterWildsRPG.Objects.Common.Effects;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace OuterWildsRPG.Utils
{
    public abstract class PropLike<TThis, TData> : EntityLike<TThis, TData>, IPropLike where TThis : PropLike<TThis, TData>, new() where TData : PropLikeData, new()
    {
        public string ParentPath;
        public Vector3 Position;
        public Vector3 Rotation;
        public bool IsRelativeToParent;
        public bool AlignRadial;

        string IPropLike.ParentPath { get => ParentPath; set => ParentPath = value; }
        Vector3 IPropLike.Position { get => Position; set => Position = value; }
        Vector3 IPropLike.Rotation { get => Rotation; set => Rotation = value; }
        bool IPropLike.IsRelativeToParent { get => IsRelativeToParent; set => IsRelativeToParent = value; }
        bool IPropLike.AlignRadial { get => AlignRadial; set => AlignRadial = value; }

        public override void Load(TData data, string modID)
        {
            base.Load(data, modID);
            ParentPath = data.parentPath;
            Position = data.position.ToVector3();
            Rotation = data.rotation.ToVector3();
            IsRelativeToParent = data.isRelativeToParent;
            AlignRadial = data.alignRadial ?? data.rotation == null;
        }
    }

    public interface IPropLike : IEntityLike
    {
        public string ParentPath { get; set; }
        public Vector3 Position { get; set; }
        public Vector3 Rotation { get; set; }
        public bool IsRelativeToParent { get; set; }
        public bool AlignRadial { get; set; }
    }
}
