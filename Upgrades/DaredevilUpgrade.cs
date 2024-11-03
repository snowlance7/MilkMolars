using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Text;
using HarmonyLib;
using Unity.Netcode;
using GameNetcodeStuff;
using BepInEx.Logging;
using static MilkMolars.Plugin;
using static MilkMolars.MilkMolarUpgrade;
using System.Xml.Linq;
using System.ComponentModel;

namespace MilkMolars.Upgrades
{
    internal class DaredevilUpgrade : MilkMolarUpgrade
    {
        public DaredevilUpgrade()
        {
            Name = "Daredevil";
            Title = "Daredevil";
            Description = "Reduces fall damage per upgrade";
            Type = UpgradeType.TierPercent;
            GetTiersFromString(configDaredevilUpgrade.Value);
            Shared = false;
            Visible = true;
        }
    }

    [HarmonyPatch]
    internal class DaredevilUpgradePatches
    {
        private static ManualLogSource logger = Plugin.LoggerInstance;

        [HarmonyPrefix]
        [HarmonyPatch(typeof(PlayerControllerB), nameof(PlayerControllerB.DamagePlayer))]
        public static void DamagePlayerPrefix(PlayerControllerB __instance, ref int damageNumber, bool fallDamage) // TODO: Test this
        {
            if (localPlayer.actualClientId == __instance.actualClientId)
            {
                // FALL DAMAGE REDUCTION UPGRADE
                if (fallDamage)
                {
                    MilkMolarUpgrade fallDamageResist = MilkMolarController.GetUpgradeByName("Daredevil");
                    if (fallDamageResist != null && fallDamageResist.Unlocked)
                    {
                        damageNumber -= (int)(damageNumber * (fallDamageResist.CurrentTierPercent));
                    }
                }
            }
        }
    }
}
