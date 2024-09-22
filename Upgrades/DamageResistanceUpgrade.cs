using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Text;
using HarmonyLib;
using Unity.Netcode;
using GameNetcodeStuff;
using BepInEx.Logging;
using static MilkMolars.Plugin;

namespace MilkMolars.Upgrades
{
    internal class DamageResistanceUpgrade : MilkMolarUpgrade
    {
        public DamageResistanceUpgrade()
        {
            name = "DamageResistance";
            title = "Damage Resistance";
            description = "Reduces damage taken per upgrade";
            type = UpgradeType.TierPercent;
            GetTiersFromString(configDamageResistanceUpgrade.Value);
        }
    }

    [HarmonyPatch]
    internal class DamageResistanceUpgradePatches
    {
        private static ManualLogSource logger = Plugin.LoggerInstance;

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
            }
        }
    }
}
