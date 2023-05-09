using OWML.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OuterWildsRPG.Utils
{
    public abstract class Entity<TThis, TData> : EntityLike<TThis, TData>, IEntity where TThis : Entity<TThis, TData>, new() where TData : EntityData, new()
    {
        public string ModID;
        public string ID;
        public string Name;

        public string FullID => Entity.GetID(ID, ModID);

        string IEntity.ModID { get => ModID; set => ModID = value; }
        string IEntity.ID { get => ID; set => ID = value; }
        string IEntity.Name { get => Name; set => Name = value; }

        public override void Load(TData data, string modID)
        {
            base.Load(data, modID);
            ModID = modID;
            ID = data.id;
            Name = data.name;
        }

        public override void Resolve()
        {
            base.Resolve();
            TranslationUtils.RegisterGeneral(FullID, Name);
        }

        public virtual string ToDisplayString(bool richText = true) => TranslationUtils.GetGeneral(FullID);

        public override string ToString() => FullID;
    }

    public static class Entity
    {
        public static string GetID(string entityID, string modID)
        {
            if (string.IsNullOrEmpty(modID))
                modID = OuterWildsRPG.ModID;
            if (entityID.Contains("@"))
            {
                var (realEntityID, mod) = ParseID(entityID);
                if (mod is OuterWildsRPG) return realEntityID;
                entityID = realEntityID;
                modID = mod.ModHelper.Manifest.UniqueName;
            } else if (modID == OuterWildsRPG.ModID)
            {
                return entityID;
            }
            return $"{modID}@{entityID}";
        }

        public static (string, IModBehaviour) ParseID(string id)
        {
            IModBehaviour mod = OuterWildsRPG.Instance;
            if (id.Contains("@"))
            {
                var bits = id.Split('@');
                var modID = bits[0];
                var entityID = bits[1];
                mod = OuterWildsRPG.Instance.ModHelper.Interaction.TryGetMod(modID);
                id = entityID;
            }
            return (id, mod);
        }
    }

    public interface IEntity : IEntityLike, IDisplayable
    {
        public string ModID { get; set; }
        public string ID { get; set; }
        public string Name { get; set; }
        public string FullID { get; }
    }
}
