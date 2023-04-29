using OuterWildsRPG.Components;
using OuterWildsRPG.Enums;
using OuterWildsRPG.Utils;
using OWML.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Events;

namespace OuterWildsRPG.Objects.Drops
{
    public static class DropManager
    {
        public class DropEvent : UnityEvent<Drop> { }
        public static DropEvent OnPickUpDrop = new();

        public class DropEquipEvent : UnityEvent<Drop, EquipSlot> { }
        public static DropEquipEvent OnEquipDrop = new();
        public static DropEquipEvent OnUnequipDrop = new();

        static Dictionary<string, Drop> drops = new();

        public static Dictionary<string, Drop> Dictionary => drops;

        public static Drop GetDrop(string id, string modID)
        {
            if (!id.Contains("/")) id = $"{modID}/{id}";
            if (drops.TryGetValue(id, out var drop))
            {
                return drop;
            }
            return null;
        }

        public static bool RegisterDrop(Drop drop)
        {
            if (!drops.ContainsKey(drop.FullID))
            {
                drops.Add(drop.FullID, drop);
                return true;
            }
            return false;
        }

        public static IEnumerable<Drop> GetAllDrops()
            => drops.Values;

        public static Drop GetEquippedDrop(EquipSlot slot)
        {
            if (!DropSaveData.Instance.Equipment.ContainsKey(slot)) return null;
            return GetDrop(DropSaveData.Instance.Equipment[slot], OuterWildsRPG.ModID);
        }

        public static IEnumerable<Drop> GetEquippedDrops()
        {
            return DropSaveData.Instance.Equipment.Values.Select(s => GetDrop(s, OuterWildsRPG.ModID));
        }

        public static bool HasDrop(Drop drop)
        {
            if (DropSaveData.Instance.Inventory.Contains(drop.FullID)) return true;
            if (DropSaveData.Instance.Equipment.ContainsValue(drop.FullID)) return true;
            return false;
        }

        public static bool EquipDrop(Drop drop, EquipSlot slot)
        {
            if (drop.EquipSlot != slot) return false;
            if (DropSaveData.Instance.Equipment.ContainsKey(slot)) return false;
            if (!DropSaveData.Instance.Inventory.Contains(drop.FullID)) return false;
            DropSaveData.Instance.Inventory.Remove(drop.FullID);
            DropSaveData.Instance.Equipment[slot] = drop.FullID;
            OnEquipDrop.Invoke(drop, slot);
            return true;
        }

        public static bool UnequipDrop(Drop drop, EquipSlot slot)
        {
            if (!DropSaveData.Instance.Equipment.ContainsKey(slot)) return false;
            if (DropSaveData.Instance.Equipment[slot] != drop.FullID) return false;
            DropSaveData.Instance.Equipment.Remove(slot);
            DropSaveData.Instance.Inventory.Add(drop.FullID);
            OnUnequipDrop.Invoke(drop, slot);
            return true;
        }

        public static bool PickUpDrop(Drop drop)
        {
            DropSaveData.Instance.Inventory.Add(drop.FullID);
            OnPickUpDrop.Invoke(drop);
            if (drop.EquipSlot != EquipSlot.None)
            {
                if (GetEquippedDrop(drop.EquipSlot) == null)
                {
                    EquipDrop(drop, drop.EquipSlot);
                }
            }
            return true;
        }

        public static bool SpawnDropPickup(DropLocation location)
        {
            try
            {
                var parent = UnityUtils.GetTransformAtPath(location.ParentPath);
                var go = new GameObject(location.Drop.Name);
                go.transform.parent = parent;
                go.transform.localPosition = location.Position;
                go.transform.localEulerAngles = location.Rotation;
                var pickup = go.AddComponent<DropPickup>();
                pickup.Init(location);
                return true;
            } catch (Exception ex)
            {
                OuterWildsRPG.Instance.ModHelper.Console.WriteLine($"Failed to generate pickup for {location.Drop.Name} drop location", MessageType.Warning);
                OuterWildsRPG.Instance.ModHelper.Console.WriteLine(ex.Message, MessageType.Error);
            }
            return false;
        }

        public static DropPickup FindDropPickup(DropLocation location)
        {
            var pickup = DropPickup.All.Find(d => d.GetLocation() == location && d.gameObject.activeSelf);
            return pickup;
        }

        public static DropPickup FindDropPickup(Drop drop)
        {
            var pickups = DropPickup.All.Where(d => d.GetLocation().Drop == drop && d.gameObject.activeSelf);
            return pickups.FirstOrDefault();
        }
    }
}
