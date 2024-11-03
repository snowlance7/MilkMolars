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
            Name = "DamageResistance";
            Title = "Damage Resistance";
            Description = "Reduces damage taken per upgrade";
            Type = UpgradeType.TierPercent;
            GetTiersFromString(configDamageResistanceUpgrade.Value);
            Shared = false;
            Visible = true;
        }
    }

    [HarmonyPatch]
    internal class DamageResistanceUpgradePatches
    {
        private static ManualLogSource logger = Plugin.LoggerInstance;

        [HarmonyPrefix]
        [HarmonyPatch(typeof(PlayerControllerB), nameof(PlayerControllerB.DamagePlayer))]
        public static void DamagePlayerPrefix(PlayerControllerB __instance, ref int damageNumber, bool fallDamage) // TODO: Test this
        {
            if (localPlayer.actualClientId == __instance.actualClientId)
            {
                // DAMAGE REDUCTION UPGRADE
                MilkMolarUpgrade damageResist = MilkMolarController.GetUpgradeByName("DamageResistance");
                if (damageResist != null && damageResist.Unlocked)
                {
                    damageNumber -= (int)(damageNumber * damageResist.CurrentTierPercent);
                }
            }
        }
    }
}
