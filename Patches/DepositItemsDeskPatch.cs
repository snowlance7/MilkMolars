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
            if (NetworkManager.Singleton.IsHost || NetworkManager.Singleton.IsServer)
            {
                if (configSharedMilkMolars.Value)
                {
                    int amount = objectsOnDesk.OfType<MilkMolarBehavior>().Count();
                    if (amount > 0)
                    {
                        foreach (var player in StartOfRound.Instance.allPlayerScripts)
                        {
                            NetworkHandler.UpdateClientsMilkMolars(player.actualClientId, amount, true);
                        }

                        NetworkHandler.Instance.AddMultipleMilkMolarsAllClientsClientRpc(amount);
                    }
                }
                else
                {
                    foreach (var player in StartOfRound.Instance.allPlayerScripts)
                    {
                        int amount;
                        if (configUpgradePointsToFinder.Value) { amount = objectsOnDesk.OfType<MilkMolarBehavior>().Where(x => x.playerFoundBy == player).Count(); }
                        else { amount = objectsOnDesk.OfType<MilkMolarBehavior>().Where(x => x.lastPlayerHeldBy == player).Count(); }

                        if (amount > 0)
                        {
                            NetworkHandler.UpdateClientsMilkMolars(player.actualClientId, amount, true);
                            NetworkHandler.Instance.AddMultipleMilkMolarsClientRpc(player.actualClientId, amount);
                        }
                    }
                }

                int megaAmount = objectsOnDesk.OfType<MegaMilkMolarBehavior>().Count();
                if (megaAmount > 0)
                {
                    NetworkHandler.MegaMilkMolars.Value += megaAmount;
                    NetworkHandler.Instance.AddMultipleMegaMilkMolarsClientRpc(megaAmount);
                }
            }
        }
    }
}
