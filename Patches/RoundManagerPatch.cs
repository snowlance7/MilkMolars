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
    [HarmonyPatch(typeof(RoundManager))]
    internal class RoundManagerPatch
    {
        private static ManualLogSource logger = Plugin.LoggerInstance;
        private const string tooth = "🦷";

        [HarmonyPostfix]
        [HarmonyPatch(nameof(RoundManager.SpawnScrapInLevel))]
        public static void SpawnScrapInLevelPostfix(RoundManager __instance)
        {
            logger.LogDebug("InSpawnScrapInLevelPostfix");
            MilkMolarController.SpawnMolarsInLevel();
        }
    }
}
