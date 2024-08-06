using BepInEx.Logging;
using HarmonyLib;
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
    [HarmonyPatch(typeof(DepositItemsDesk))]
    internal class DepositItemsDeskPatch
    {
        private static ManualLogSource logger = Plugin.LoggerInstance;

        [HarmonyPostfix]
        [HarmonyPatch(nameof(DepositItemsDesk.delayedAcceptanceOfItems))]
        public static void delayedAcceptanceOfItemsPostFix(GrabbableObject[] objectsOnDesk) // TODO: Test this
        {
            // TODO: Assuming this runs on every client
            if (!configSharedMilkMolars.Value)
            {
                int amount;
                if (configUpgradePointsToFinder.Value) { amount = objectsOnDesk.OfType<MilkMolarBehavior>().Where(x => x.playerFoundBy == localPlayer).Count(); }
                else { amount = objectsOnDesk.OfType<MilkMolarBehavior>().Where(x => x.playerHeldBy == localPlayer).Count(); }
                if (amount > 0) { MilkMolarController.AddMultipleMilkMolars(amount); }
            }
            else
            {
                int amount = objectsOnDesk.OfType<MilkMolarBehavior>().Count();
                if (amount > 0) { MilkMolarController.AddMultipleMilkMolars(amount); }
            }

            if (NetworkManager.Singleton.IsHost || NetworkManager.Singleton.IsServer)
            {
                int megaAmount = objectsOnDesk.OfType<MegaMilkMolarBehavior>().Count();
                if (megaAmount > 0)
                {
                    MilkMolarController.AddMultipleMegaMilkMolars(megaAmount);
                }
            }
        }
    }
}
