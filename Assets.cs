using OuterWildsRPG.Enums;
using OuterWildsRPG.Utils;
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
        public static AudioClip HearthianJingleAudioClip;
        public static AudioClip NomaiJingleAudioClip;
        public static AudioClip StrangerJingleAudioClip;
        public static AudioClip LevelUpJingleAudioClip;
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

        public static void Init()
        {
            CheckboxOnTex = OuterWildsRPG.Instance.ModHelper.Assets.GetTexture("assets/CheckboxOn.png");
            CheckboxOffTex = OuterWildsRPG.Instance.ModHelper.Assets.GetTexture("assets/CheckboxOff.png");
            CheckboxOnSprite = Sprite.Create(CheckboxOnTex, new Rect(0f, 0f, CheckboxOnTex.width, CheckboxOnTex.height), Vector2.one * 0.5f, 256f);
            CheckboxOffSprite = Sprite.Create(CheckboxOffTex, new Rect(0f, 0f, CheckboxOffTex.width, CheckboxOffTex.height), Vector2.one * 0.5f, 256f);
            HearthianJingleAudioClip = OuterWildsRPG.Instance.ModHelper.Assets.GetAudio("assets/HearthianJingle.wav");
            NomaiJingleAudioClip = OuterWildsRPG.Instance.ModHelper.Assets.GetAudio("assets/NomaiJingle.wav");
            StrangerJingleAudioClip = OuterWildsRPG.Instance.ModHelper.Assets.GetAudio("assets/StrangerJingle.wav");
            LevelUpJingleAudioClip = OuterWildsRPG.Instance.ModHelper.Assets.GetAudio("assets/LevelUpJingle.wav");
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

        public static Texture2D GetIconTexture(string filename, string modID)
        {
            if (string.IsNullOrEmpty(filename)) return null;
            var fullID = Entity.GetID(filename, modID);
            if (iconTextures.ContainsKey(fullID)) return iconTextures[fullID];
            var mod = OuterWildsRPG.Instance.ModHelper.Interaction.TryGetMod(modID);
            var path = Path.GetFullPath(Path.Combine(mod.ModHelper.Manifest.ModFolderPath, $"icons/{filename}"));
            if (File.Exists(path))
            {
                var icon = mod.ModHelper.Assets.GetTexture($"icons/{filename}");
                icon.name = fullID;
                iconTextures.Add(fullID, icon);
                return icon;
            } else
            {
                OuterWildsRPG.LogError($"Failed to load icon {fullID} at {path}");
            }
            iconTextures.Add(fullID, null);
            return null;
        }

        public static Sprite GetIconSprite(string filename, string modID)
        {
            if (string.IsNullOrEmpty(filename)) return null;
            var fullID = Entity.GetID(filename, modID);
            if (iconSprites.ContainsKey(fullID)) return iconSprites[fullID];
            var icon = GetIconTexture(filename, modID);
            if (icon != null)
            {
                var sprite = Sprite.Create(icon, new Rect(0f, 0f, icon.width, icon.height), Vector2.one * 0.5f, 32f);
                sprite.name = fullID;
                iconSprites.Add(fullID, sprite);
                return sprite;
            }
            iconSprites.Add(fullID, null);
            return null;
        }

        public static Texture2D GetEntityTexture(string id)
        {
            var (entityID, mod) = Entity.ParseID(id);
            return GetIconTexture($"{entityID}.png", mod.ModHelper.Manifest.UniqueName);
        }

        public static Sprite GetEntitySprite(string id)
        {
            var (entityID, mod) = Entity.ParseID(id);
            return GetIconSprite($"{entityID}.png", mod.ModHelper.Manifest.UniqueName);
        }

        public static string GetTextFileContents(string filename, string modID)
        {
            if (string.IsNullOrEmpty(filename)) return null;
            var mod = OuterWildsRPG.Instance.ModHelper.Interaction.TryGetMod(modID);
            var path = Path.GetFullPath(Path.Combine(mod.ModHelper.Manifest.ModFolderPath, filename));
            if (File.Exists(path))
            {
                var contents = File.ReadAllText(path);
                return contents;
            }
            return null;
        }
    }
}
