using OuterWildsRPG.Enums;
using OWML.Common;
using OWML.ModHelper;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace OuterWildsRPG
{
    public static class Assets
    {
        public static Texture2D CheckboxOnTex;
        public static Texture2D CheckboxOffTex;
        public static Sprite CheckboxOnSprite;
        public static Sprite CheckboxOffSprite;
        public static AudioClip QuestJingleAudioClip;
        public static Texture2D ParticleTex;

        public static Color HUDBackColor = new Color(0.4f, 0.4f, 0.4f, 1f);
        public static Color HUDForeColor = Color.white;
        public static Color HUDActiveColor = new Color(0.9686f, 0.498f, 0.2078f);
        public static Color RarityCommonColor = Color.white;
        public static Color RarityUncommonColor = Color.HSVToRGB(120f / 360f, 1f, 1f);
        public static Color RarityRareColor = Color.HSVToRGB(210f / 360f, 1f, 1f);
        public static Color RarityEpicColor = Color.HSVToRGB(285f / 360f, 1f, 1f);
        public static Color RarityLegendaryColor = Color.HSVToRGB(30f / 360f, 1f, 1f);

        static Dictionary<string, Texture2D> iconTextures = new();
        static Dictionary<string, Sprite> iconSprites = new();

        public static void LoadAll()
        {
            CheckboxOnTex = OuterWildsRPG.Instance.ModHelper.Assets.GetTexture("assets/CheckboxOn.png");
            CheckboxOffTex = OuterWildsRPG.Instance.ModHelper.Assets.GetTexture("assets/CheckboxOff.png");
            CheckboxOnSprite = Sprite.Create(CheckboxOnTex, new Rect(0f, 0f, CheckboxOnTex.width, CheckboxOnTex.height), Vector2.one * 0.5f, 256f);
            CheckboxOffSprite = Sprite.Create(CheckboxOffTex, new Rect(0f, 0f, CheckboxOffTex.width, CheckboxOffTex.height), Vector2.one * 0.5f, 256f);
            QuestJingleAudioClip = OuterWildsRPG.Instance.ModHelper.Assets.GetAudio("assets/QuestJingle.wav");
            ParticleTex = OuterWildsRPG.Instance.ModHelper.Assets.GetTexture("assets/Particle.png");
        }

        public static Color GetRarityColor(DropRarity rarity)
        {
            return rarity switch
            {
                DropRarity.Common => RarityCommonColor,
                DropRarity.Uncommon => RarityUncommonColor,
                DropRarity.Rare => RarityRareColor,
                DropRarity.Epic => RarityEpicColor,
                DropRarity.Legendary => RarityLegendaryColor,
                _ => RarityCommonColor,
            };
        }

        public static Texture2D GetEntityTexture(string entityID)
        {
            var (id, mod) = ParseEntityID(entityID);
            if (iconTextures.ContainsKey(id))
            {
                return iconTextures[id];
            }
            var path = Path.GetFullPath(Path.Combine(mod.ModHelper.Manifest.ModFolderPath, $"icons/{id}.png"));
            if (File.Exists(path))
            {
                var icon = mod.ModHelper.Assets.GetTexture($"icons/{id}.png");
                iconTextures.Add(id, icon);
                return icon;
            }
            iconTextures.Add(id, null);
            return null;
        }

        public static Sprite GetEntitySprite(string entityID, float pixelsPerUnit = 32f)
        {
            var (id, _) = ParseEntityID(entityID);
            if (iconSprites.ContainsKey(id))
            {
                return iconSprites[id];
            }
            var texture = GetEntityTexture(id);
            if (texture != null)
            {
                var sprite = Sprite.Create(texture, new Rect(0f, 0f, texture.width, texture.height), Vector2.one * 0.5f, pixelsPerUnit);
                iconSprites.Add(id, sprite);
                return sprite;
            }
            iconSprites.Add(id, null);
            return null;
        }

        static (string, IModBehaviour) ParseEntityID(string id)
        {
            IModBehaviour mod = OuterWildsRPG.Instance;
            if (id.Contains("/"))
            {
                var bits = id.Split('/');
                var modID = bits[0];
                var assetID = bits[1];
                mod = OuterWildsRPG.Instance.ModHelper.Interaction.TryGetMod(modID);
                id = assetID;
            }
            var fullID = $"{mod.ModHelper.Manifest.UniqueName}/{id}";
            return (fullID, mod);
        }
    }
}
