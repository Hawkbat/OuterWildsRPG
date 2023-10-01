using OuterWildsRPG.Components;
using OuterWildsRPG.Enums;
using OuterWildsRPG.Objects.Common;
using OuterWildsRPG.Objects.Drops;
using OuterWildsRPG.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Events;

namespace OuterWildsRPG.Objects.Shops
{
    public static class ShopManager
    {
        public class ShopItemEvent : UnityEvent<Shop, Drop> { }
        public static ShopItemEvent OnBuyDrop = new();
        public static ShopItemEvent OnSellDrop = new();
        public static ShopItemEvent OnBuyBackDrop = new();

        static readonly Dictionary<string, Shop> shops = new();

        static readonly Dictionary<string, ShopkeeperController> shopkeepers = new();

        public static Shop GetShop(string id, string modID = null)
        {
            if (shops.TryGetValue(Entity.GetID(id, modID), out var shop))
            {
                return shop;
            }
            return null;
        }

        public static bool RegisterShop(Shop shop)
        {
            if (!shops.ContainsKey(shop.FullID))
            {
                shops.Add(shop.FullID, shop);
                return true;
            }
            return false;
        }

        public static IEnumerable<Shop> GetAllShops()
            => shops.Values;

        public static int GetCurrencyAmount() => ShopSaveData.Instance.Currency;

        public static ShopkeeperController GetShopkeeper(Shop shop)
            => shopkeepers.TryGetValue(shop.FullID, out var shopkeeper) ? shopkeeper : null;

        public static IEnumerable<Drop> GetBuyableShopItems(Shop shop) => shop.Items.Select(i => i.Drop);
        public static IEnumerable<Drop> GetSellableShopItems(Shop shop) =>
            DropManager.GetHotbarDrops().Concat(DropManager.GetInventoryDrops()).Distinct();
        public static IEnumerable<Drop> GetBuyBackShopItems(Shop shop)
        {
            if (!ShopSaveData.Instance.Sales.ContainsKey(shop.FullID)) return Enumerable.Empty<Drop>();
            return ShopSaveData.Instance.Sales[shop.FullID].Keys.Select(d => DropManager.GetDrop(d));
        }

        public static int GetBuyAmount(Shop shop, Drop drop)
        {
            var item = shop.Items.Find(i => i.Drop == drop);
            if (item == null) return 0;
            var stock = ShopSaveData.Instance.Purchases.ContainsKey(shop.FullID) && ShopSaveData.Instance.Purchases[shop.FullID].TryGetValue(item.Drop.FullID, out var count) ? item.InitialStock - count : item.InitialStock;
            return stock;
        }
        public static int GetSellAmount(Shop shop, Drop drop)
            => DropManager.GetHotbarDrops().Concat(DropManager.GetInventoryDrops()).Count(d => d == drop);
        public static int GetBuyBackAmount(Shop shop, Drop drop)
        {
            if (ShopSaveData.Instance.Sales.ContainsKey(shop.FullID) && ShopSaveData.Instance.Sales[shop.FullID].TryGetValue(drop.FullID, out var count))
                return count;
            return 0;
        }

        public static int GetBuyCost(Shop shop, Drop drop) => GetCostByRarity(drop.Rarity);
        public static int GetSellCost(Shop shop, Drop drop) => GetCostByRarity(drop.Rarity) / 2;
        public static int GetBuyBackCost(Shop shop, Drop drop) => GetBuyableShopItems(shop).Contains(drop) ? GetBuyCost(shop, drop) : GetCostByRarity(drop.Rarity);

        public static bool BuyItem(Shop shop, Drop drop)
        {
            if (!GetBuyableShopItems(shop).Contains(drop)) return false;
            if (GetBuyAmount(shop, drop) <= 0) return false;
            if (GetBuyCost(shop, drop) > GetCurrencyAmount()) return false;
            if (!DropManager.ReceiveDrop(drop)) return false;
            if (!ShopSaveData.Instance.Purchases.ContainsKey(shop.FullID)) ShopSaveData.Instance.Purchases.Add(shop.FullID, new());
            if (!ShopSaveData.Instance.Purchases[shop.FullID].ContainsKey(drop.FullID)) ShopSaveData.Instance.Purchases[shop.FullID].Add(drop.FullID, 0);
            ShopSaveData.Instance.Currency -= GetBuyCost(shop, drop);
            ShopSaveData.Instance.Purchases[shop.FullID][drop.FullID]++;
            SaveDataManager.Save();
            OnBuyDrop.Invoke(shop, drop);
            return true;
        }

        public static bool SellItem(Shop shop, Drop drop)
        {
            if (!GetSellableShopItems(shop).Contains(drop)) return false;
            if (GetSellAmount(shop, drop) <= 0) return false;
            if (!DropManager.RemoveDrop(drop)) return false;
            if (!ShopSaveData.Instance.Sales.ContainsKey(shop.FullID)) ShopSaveData.Instance.Sales.Add(shop.FullID, new());
            if (!ShopSaveData.Instance.Sales[shop.FullID].ContainsKey(drop.FullID)) ShopSaveData.Instance.Sales[shop.FullID].Add(drop.FullID, 0);
            ShopSaveData.Instance.Currency += GetSellCost(shop, drop);
            ShopSaveData.Instance.Sales[shop.FullID][drop.FullID]++;
            SaveDataManager.Save();
            OnSellDrop.Invoke(shop, drop);
            return true;
        }

        public static bool BuyBackItem(Shop shop, Drop drop)
        {
            if (!GetBuyBackShopItems(shop).Contains(drop)) return false;
            if (GetBuyBackAmount(shop, drop) <= 0) return false;
            if (!DropManager.ReceiveDrop(drop)) return false;
            ShopSaveData.Instance.Currency -= GetBuyBackCost(shop, drop);
            ShopSaveData.Instance.Sales[shop.FullID][drop.FullID]--;
            if (ShopSaveData.Instance.Sales[shop.FullID][drop.FullID] <= 0) ShopSaveData.Instance.Sales[shop.FullID].Remove(drop.FullID);
            SaveDataManager.Save();
            OnBuyBackDrop.Invoke(shop, drop);
            return true;
        }

        static int GetCostByRarity(DropRarity rarity)
        {
            return rarity switch
            {
                DropRarity.Common => 2,
                DropRarity.Uncommon => 10,
                DropRarity.Rare => 30,
                DropRarity.Epic => 100,
                DropRarity.Legendary => 300,
                _ => 2,
            };
        }

        public static void SetUp()
        {
            foreach (var shop in GetAllShops())
            {
                var character = UnityUtils.GetTransformAtPath(shop.CharacterPath, $"Failed to locate character for shop {shop.FullID}");
                if (character == null) continue;
                if (shop.CharacterClone != null)
                {
                    var go = GameObject.Instantiate(character.gameObject);
                    go.name = shop.FullID;
                    character = UnityUtils.PlaceProp(go.transform, shop.CharacterClone);
                    go.SetActive(true);
                }
                var shopkeep = character.gameObject.AddComponent<ShopkeeperController>();
                shopkeep.Init(shop);
                WorldIconManager.MakeTarget(shopkeep.transform);
                shopkeepers.Add(shop.FullID, shopkeep);
            }
        }

        public static void CleanUp()
        {
            shopkeepers.Clear();
        }
    }
}
