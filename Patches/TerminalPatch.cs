using BepInEx.Logging;
using HarmonyLib;
using InteractiveTerminalAPI.UI;
using LethalLib.Extras;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Unity.Netcode;
using UnityEngine;
using static MilkMolars.Plugin;

namespace MilkMolars
{
    [HarmonyPatch(typeof(Terminal))]
    internal class TerminalPatch
    {
        private static ManualLogSource logger = Plugin.LoggerInstance;

        [HarmonyPostfix]
        [HarmonyPatch(nameof(Terminal.Awake))]
        public static void AwakePostfix(Terminal __instance) // TODO: Test this
        {
            if (!__instance.inputFieldText.font.fallbackFontAssetTable.Contains(NotoEmojiFA)) { __instance.inputFieldText.font.fallbackFontAssetTable.Add(NotoEmojiFA); }
            if (!__instance.topRightText.font.fallbackFontAssetTable.Contains(NotoEmojiFA)) { __instance.topRightText.font.fallbackFontAssetTable.Add(NotoEmojiFA); }
            if (!__instance.screenText.fontAsset.fallbackFontAssetTable.Contains(NotoEmojiFA)) { __instance.screenText.fontAsset.fallbackFontAssetTable.Add(NotoEmojiFA); }
        }

        [HarmonyPostfix]
        [HarmonyPatch(nameof(Terminal.Update))]
        public static void UpdatePostfix(Terminal __instance) // TODO: Test this
        {
            if (InteractiveTerminalManager.Instance != null && InteractiveTerminalManager.Instance.isActiveAndEnabled)
            {
                if (MilkMolarController.InUpgradeUI)
                {
                    __instance.topRightText.text = MilkMolarUpgrade.tooth + MilkMolarController.MilkMolars.ToString();
                }
                else if (MilkMolarController.InMegaUpgradeUI)
                {
                    __instance.screenText.text = MilkMolarUpgrade.tooth + NetworkHandler.Instance.MegaMilkMolars.ToString();
                }
            }
        }
    }
}
