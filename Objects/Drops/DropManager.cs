﻿using OuterWildsRPG.Components;
using OuterWildsRPG.Enums;
using OuterWildsRPG.Objects.Common;
using OuterWildsRPG.Utils;
using OWML.Common;
using OWML.ModHelper.Events;
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
        public static DropEvent OnReceiveDrop = new();
        public static DropEvent OnRemoveDrop = new();
        public static DropEvent OnConsumeDrop = new();
        public static DropEvent OnConsumedDropExpired = new();

        public class DropLocationEvent : UnityEvent<DropLocation> { }
        public static DropLocationEvent OnPickUpDrop = new();

        public class DropEquipEvent : UnityEvent<Drop, EquipSlot> { }
        public static DropEquipEvent OnEquipDrop = new();
        public static DropEquipEvent OnUnequipDrop = new();

        static Dictionary<string, Drop> drops = new();

        public static Drop GetDrop(string id, string modID = null)
        {
            if (drops.TryGetValue(Entity.GetID(id, modID), out var drop))
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

        public static IEnumerable<Drop> GetInventoryDrops()
            => DropSaveData.Instance.Inventory.Select(i => GetDrop(i));

        public static Drop GetInventoryDrop(int index)
        {
            if (index >= 0 && index < DropSaveData.Instance.Inventory.Count)
                return GetDrop(DropSaveData.Instance.Inventory[index]);
            return null;
        }

        public static Drop GetEquippedDrop(EquipSlot slot)
        {
            if (!DropSaveData.Instance.Equipment.ContainsKey(slot)) return null;
            return GetDrop(DropSaveData.Instance.Equipment[slot]);
        }

        public static IEnumerable<Drop> GetEquippedDrops()
        {
            return DropSaveData.Instance.Equipment.Values.Select(s => GetDrop(s, OuterWildsRPG.ModID));
        }

        public static int GetTotalInventoryCapacity() => BuffManager.GetInventoryCapacity();
        public static int GetUsedInventoryCapacity() => DropSaveData.Instance.Inventory.Count;
        public static int GetRemainingInventoryCapacity() => GetTotalInventoryCapacity() - GetUsedInventoryCapacity();

        public static bool HasDrop(Drop drop)
        {
            if (DropSaveData.Instance.Inventory.Contains(drop.FullID)) return true;
            if (DropSaveData.Instance.Equipment.ContainsValue(drop.FullID)) return true;
            return false;
        }

        public static bool HasSeenDrop(Drop drop)
            => DropSaveData.Instance.HasSeen.Contains(drop.FullID);

        public static bool SeeDrop(Drop drop) {
            if (DropSaveData.Instance.HasSeen.Add(drop.FullID))
            {
                SaveDataManager.Save();
                return true;
            }
            return false;
        }

        public static bool HasReadDrop(Drop drop)
            => DropSaveData.Instance.HasRead.Contains(drop.FullID);

        public static bool ReadDrop(Drop drop)
        {
            if (DropSaveData.Instance.HasRead.Add(drop.FullID))
            {
                SaveDataManager.Save();
                return true;
            }
            return false;
        }

        public static bool EquipDrop(Drop drop, EquipSlot slot)
        {
            if (drop.EquipSlot != slot || slot == EquipSlot.None) return false;
            if (slot != EquipSlot.Item && !DropSaveData.Instance.Inventory.Contains(drop.FullID)) return false;
            if (DropSaveData.Instance.Equipment.ContainsKey(slot))
            {
                if (!UnequipDrop(GetEquippedDrop(slot), slot))
                    return false;
            }
            if (slot != EquipSlot.Item) DropSaveData.Instance.Inventory.Remove(drop.FullID);
            DropSaveData.Instance.Equipment[slot] = drop.FullID;
            SaveDataManager.Save();
            OnEquipDrop.Invoke(drop, slot);
            return true;
        }

        public static bool UnequipDrop(Drop drop, EquipSlot slot)
        {
            if (!DropSaveData.Instance.Equipment.ContainsKey(slot)) return false;
            if (DropSaveData.Instance.Equipment[slot] != drop.FullID) return false;
            DropSaveData.Instance.Equipment.Remove(slot);
            if (slot != EquipSlot.Item) DropSaveData.Instance.Inventory.Add(drop.FullID);
            SaveDataManager.Save();
            OnUnequipDrop.Invoke(drop, slot);
            return true;
        }

        public static bool ReceiveDrop(Drop drop)
        {
            if (GetRemainingInventoryCapacity() <= 0) return false;
            DropSaveData.Instance.Inventory.Add(drop.FullID);
            SaveDataManager.Save();
            OnReceiveDrop.Invoke(drop);
            if (drop.EquipSlot != EquipSlot.None)
            {
                if (GetEquippedDrop(drop.EquipSlot) == null)
                {
                    EquipDrop(drop, drop.EquipSlot);
                }
            }
            return true;
        }

        public static bool RemoveDrop(Drop drop)
        {
            if (DropSaveData.Instance.Inventory.Remove(drop.FullID))
            {
                SaveDataManager.Save();
                OnRemoveDrop.Invoke(drop);
                return true;
            }
            return false;
        }

        public static IEnumerable<Drop> GetConsumedDrops()
            => DropSaveData.Instance.Consumables.Keys.Select(k => GetDrop(k));

        public static bool ConsumeDrop(Drop drop)
        {
            if (!drop.Consumable) return false;
            DropSaveData.Instance.Inventory.Remove(drop.FullID);
            if (drop.Duration > 0f)
                DropSaveData.Instance.Consumables[drop.FullID] = Time.time + drop.Duration;
            foreach (var buff in drop.Buffs)
                BuffManager.ApplyInstantEffects(buff);
            SaveDataManager.Save();
            OnConsumeDrop.Invoke(drop);
            return true;
        }

        public static bool ExpireConsumedDrop(Drop drop)
        {
            if (DropSaveData.Instance.Consumables.Remove(drop.FullID))
            {
                SaveDataManager.Save();
                OnConsumedDropExpired.Invoke(drop);
                return true;
            }
            return false;
        }

        public static bool SpawnDropPickup(DropLocation location)
        {
            if (HasPickedUpDropLocation(location) && !location.Respawns)
            {
                foreach (var visualPath in location.Visuals)
                {
                    var visual = UnityUtils.GetTransformAtPath(visualPath, $"Failed to remove visual for {location.GetUniqueKey()} drop location");
                    if (visual != null) visual.gameObject.SetActive(false);
                }
                return false;
            }
            try
            {
                var go = new GameObject(location.Drop.FullID);
                UnityUtils.PlaceProp(go.transform, location);
                go.transform.position += go.transform.up * 0.1f;

                var pickup = go.AddComponent<DropPickup>();
                pickup.Init(location);
                return true;
            } catch (Exception ex)
            {
                OuterWildsRPG.LogException(ex, $"Failed to generate pickup for {location.GetUniqueKey()} drop location");
            }
            return false;
        }

        public static DropPickup FindDropPickup(DropLocation location)
            => DropPickup.All.Find(d => d.GetLocation() == location && d.gameObject.activeSelf);

        public static IEnumerable<DropPickup> FindDropPickups(Drop drop)
            => DropPickup.All.Where(d => d.GetLocation().Drop == drop && d.gameObject.activeSelf);

        public static bool PickUpDropLocation(DropLocation dropLocation)
        {
            if (ReceiveDrop(dropLocation.Drop))
            {
                DropSaveData.Instance.HasPickedUp.Add(dropLocation.GetUniqueKey());
                SaveDataManager.Save();
                OnPickUpDrop.Invoke(dropLocation);
                return true;
            }
            return false;
        }

        public static bool HasPickedUpDropLocation(DropLocation dropLocation)
            => DropSaveData.Instance.HasPickedUp.Contains(dropLocation.GetUniqueKey());

        public static void SetUp()
        {
            foreach (var item in GameObject.FindObjectsOfType<OWItem>())
            {
                DropRarity rarity = item.GetItemType() switch
                {
                    ItemType.Scroll => DropRarity.Rare,
                    ItemType.WarpCore => (item as WarpCoreItem).GetWarpCoreType() switch
                    {
                        WarpCoreType.Vessel => DropRarity.Legendary,
                        WarpCoreType.White => DropRarity.Rare,
                        WarpCoreType.Black => DropRarity.Rare,
                        _ => DropRarity.Common,
                    },
                    ItemType.SharedStone => DropRarity.Rare,
                    ItemType.ConversationStone => DropRarity.Rare,
                    ItemType.Lantern => DropRarity.Rare,
                    ItemType.SlideReel => DropRarity.Rare,
                    ItemType.DreamLantern => (item as DreamLanternItem).GetLanternType() switch
                    {
                        DreamLanternType.Nonfunctioning => DropRarity.Epic,
                        DreamLanternType.Malfunctioning => DropRarity.Epic,
                        DreamLanternType.Functioning => DropRarity.Rare,
                        _ => DropRarity.Common,
                    },
                    ItemType.VisionTorch => DropRarity.Legendary,
                    _ => DropRarity.Common,
                };
                string iconPath = item.GetItemType() switch
                {
                    ItemType.Scroll => "SCROLL.png",
                    ItemType.WarpCore => (item as WarpCoreItem).GetWarpCoreType() switch
                    {
                        WarpCoreType.Vessel => "WARPCORE.png",
                        WarpCoreType.VesselBroken => "WARPCORE_BROKEN.png",
                        WarpCoreType.Black => "WARPCORE_BLACK.png",
                        WarpCoreType.White => "WARPCORE_WHITE.png",
                        WarpCoreType.SimpleBroken => "WARPCORE_SIMPLE_BROKEN.png",
                        _ => string.Empty,
                    },
                    ItemType.SharedStone => "STONE.png",
                    ItemType.ConversationStone => "STONE.png",
                    ItemType.Lantern => "LANTERN.png",
                    ItemType.SlideReel => "SLIDEREEL.png",
                    ItemType.DreamLantern => (item as DreamLanternItem).GetLanternType() switch
                    {
                        DreamLanternType.Nonfunctioning => "ARTIFACT_V1.png",
                        DreamLanternType.Malfunctioning => "ARTIFACT_V2.png",
                        DreamLanternType.Functioning => "ARTIFACT.png",
                        _ => string.Empty,
                    },
                    ItemType.VisionTorch => "VISIONTORCH.png",
                    _ => string.Empty,
                };

                var drop = Drop.LoadNew(new DropData()
                {
                    id = UnityUtils.GetTransformPath(item.transform).Replace('/', '$'),
                    name = item.GetDisplayName(),
                    equipSlot = EquipSlot.Item,
                    description = string.Empty,
                    iconPath = iconPath,
                    rarity = rarity,
                    locations = new List<DropLocationData>()
                    {
                        new() {
                            parentPath = UnityUtils.GetTransformPath(item.transform),
                            isRelativeToParent = true,
                            respawns = true,
                        }
                    },
                }, OuterWildsRPG.ModID);
                drop.Resolve();
                RegisterDrop(drop);
            }

            foreach (var drop in GetAllDrops())
                foreach (var location in drop.Locations)
                    SpawnDropPickup(location);
        }

        public static void Update()
        {
            var expiredConsumables = DropSaveData.Instance.Consumables
                .Where(c => Time.time > c.Value)
                .Select(c => c.Key);
            foreach (var id in expiredConsumables)
                ExpireConsumedDrop(GetDrop(id));
        }

        public static void CleanUp()
        {
            var itemDrop = GetEquippedDrop(EquipSlot.Item);
            if (itemDrop != null)
            {
                UnequipDrop(itemDrop, EquipSlot.Item);
                RemoveDrop(itemDrop);
            }
        }
    }
}
