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
                        NetworkHandler.Instance.AddMultipleMilkMolarsAllClientsClientRpc(amount);
                    }
                }
                else
                {
                    foreach (var player in StartOfRound.Instance.allPlayerScripts.Where(x => x.isPlayerControlled))
                    {
                        int amount;
                        if (configUpgradePointsToFinder.Value) { amount = objectsOnDesk.OfType<MilkMolarBehavior>().Where(x => x.playerFoundBy == player).Count(); }
                        else { amount = objectsOnDesk.OfType<MilkMolarBehavior>().Where(x => x.lastPlayerHeldBy == player).Count(); }

                        if (amount > 0)
                        {
                            NetworkHandler.Instance.AddMultipleMilkMolarsClientRpc(player.playerSteamId, amount); // TODO: player steam id
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
