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
using Newtonsoft.Json;
using GameNetcodeStuff;

namespace MilkMolars
{
    [HarmonyPatch(typeof(PlayerControllerB))]
    internal class PlayerControllerBPatch
    {
        private static ManualLogSource logger = Plugin.LoggerInstance;

        [HarmonyPostfix]
        [HarmonyPatch(nameof(PlayerControllerB.ConnectClientToPlayerObject))]
        public static void ConnectClientToPlayerObjectPostfix(PlayerControllerB __instance) // TODO: Test this
        {
            logger.LogDebug("In ConnectClientToPlayerObjectPostfix");
            MilkMolarController.Init();
        }

        [HarmonyPrefix]
        [HarmonyPatch(nameof(PlayerControllerB.DamagePlayer))]
        public static void DamagePlayerPrefix(PlayerControllerB __instance, ref int damageNumber, bool fallDamage) // TODO: Test this
        {
            if (localPlayer.actualClientId == __instance.actualClientId)
            {
                // DAMAGE REDUCTION UPGRADE
                MilkMolarUpgrade damageResist = MilkMolarController.GetUpgradeByName("DamageResistance");
                if (damageResist != null && damageResist.unlocked)
                {
                    damageNumber -= (int)(damageNumber * (damageResist.currentTierAmount / 100));
                }

                // FALL DAMAGE REDUCTION UPGRADE
                if (fallDamage)
                {
                    MilkMolarUpgrade fallDamageResist = MilkMolarController.GetUpgradeByName("FallDamageReduction");
                    if (fallDamageResist != null && fallDamageResist.unlocked)
                    {
                        damageNumber -= (int)(damageNumber * (fallDamageResist.currentTierAmount / 100));
                    }
                }
            }
        }
    }
}
