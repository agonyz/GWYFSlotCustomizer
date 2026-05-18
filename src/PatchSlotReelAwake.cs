using System;
using System.IO;
using BepInEx;
using HarmonyLib;
using UnityEngine;

namespace GWYFSlotCustomizer;

[HarmonyPatch(typeof(SlotReel), "Awake")]
public static class PatchSlotReelAwake
{
    private static readonly SlotSymbolReplacement[] Replacements =
    {
        new(0, "flower", "flower.png"),
        new(1, "beetle", "beetle.png"),
        new(2, "ankh", "ankh.png"),
        new(3, "eye", "eye.png")
    };

    private static void Postfix(SlotReel __instance)
    {
        if (__instance.atlas == null || __instance.atlas.Length == 0)
        {
            Plugin.Log.LogWarning("SlotReel atlas is missing.");
            return;
        }

        string modFolder = Path.GetDirectoryName(typeof(Plugin).Assembly.Location)!;
        string symbolsFolder = Path.Combine(modFolder, "symbols");

        if (!Directory.Exists(symbolsFolder))
        {
            Directory.CreateDirectory(symbolsFolder);
            Plugin.Log.LogWarning($"Created symbols folder: {symbolsFolder}");
            Plugin.Log.LogWarning("Put flower.png, beetle.png, ankh.png or eye.png there to replace symbols.");
            return;
        }

        foreach (SlotSymbolReplacement replacement in Replacements)
        {
            TryReplaceSymbol(__instance, symbolsFolder, replacement);
        }
    }

    private static void TryReplaceSymbol(
        SlotReel reel,
        string symbolsFolder,
        SlotSymbolReplacement replacement
    )
    {
        if (reel.atlas.Length <= replacement.Index)
        {
            Plugin.Log.LogWarning(
                $"Cannot replace {replacement.DisplayName}: atlas only has {reel.atlas.Length} symbols."
            );
            return;
        }

        string filePath = Path.Combine(symbolsFolder, replacement.FileName);

        if (!File.Exists(filePath))
        {
            return;
        }

        Sprite oldSprite = reel.atlas[replacement.Index];

        Sprite? customSprite = LoadSprite(filePath, replacement.DisplayName);

        if (customSprite == null)
        {
            return;
        }

        reel.atlas[replacement.Index] = customSprite;

        if (reel.symbols != null)
        {
            foreach (var image in reel.symbols)
            {
                if (image != null && image.sprite == oldSprite)
                {
                    image.sprite = customSprite;
                }
            }
        }

        Plugin.Log.LogInfo(
            $"Replaced {replacement.DisplayName}: {oldSprite.name} -> {customSprite.name}"
        );
    }

    private static Sprite? LoadSprite(string filePath, string displayName)
    {
        try
        {
            byte[] imageData = File.ReadAllBytes(filePath);

            Texture2D texture = new(2, 2, TextureFormat.RGBA32, false);

            if (!texture.LoadImage(imageData))
            {
                Plugin.Log.LogWarning($"Failed to load image: {filePath}");
                return null;
            }

            texture.name = $"GWYFSlotCustomizer_{displayName}_Texture";

            Sprite sprite = Sprite.Create(
                texture,
                new Rect(0, 0, texture.width, texture.height),
                new Vector2(0.5f, 0.5f),
                100f
            );

            sprite.name = $"GWYFSlotCustomizer_{displayName}_Sprite";

            return sprite;
        }
        catch (Exception exception)
        {
            Plugin.Log.LogError($"Failed to replace {displayName}: {exception}");
            return null;
        }
    }

    private struct SlotSymbolReplacement
    {
        public int Index;
        public string DisplayName;
        public string FileName;

        public SlotSymbolReplacement(int index, string displayName, string fileName)
        {
            Index = index;
            DisplayName = displayName;
            FileName = fileName;
        }
    }
}