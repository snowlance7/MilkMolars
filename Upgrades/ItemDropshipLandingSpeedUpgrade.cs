using BepInEx.Logging;
using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Linq;
using Unity.Netcode;
using static MilkMolars.Plugin;

namespace MilkMolars.Upgrades
{
    internal class ItemDropshipLandingSpeedUpgrade : MilkMolarUpgrade
    {
        public ItemDropshipLandingSpeedUpgrade()
        {
            name = "itemDropshipLandingSpeed";
            title = "Item Dropship Landing Speed";
            description = "";
            type = UpgradeType.OneTimeUnlock;
            //cost = configitemDrop
        }
    }

    [HarmonyPatch]
    internal class Patches
    {
        private static ManualLogSource logger = Plugin.LoggerInstance;

        [HarmonyPostfix]
        [HarmonyPatch(typeof(Terminal), nameof(Terminal.BuyItemsServerRpc))]
        public static void BuyItemsServerRpcPostfix(Terminal __instance)
        {
            // TODO: Itemdropship upgrade here
            /*if (NetworkManager.Singleton.IsHost || NetworkManager.Singleton.IsServer)
            {
                MilkMolarUpgrade dropShipUpgrade = NetworkHandler.MegaMilkMolarUpgrades.Where(x => x.name == "itemDropshipLandingSpeed").FirstOrDefault();
                if (dropShipUpgrade != null && dropShipUpgrade.unlocked)
                {
                    ItemDropship dropShip = UnityEngine.Object.FindObjectsOfType<ItemDropship>().FirstOrDefault();
                    if (dropShip != null && !dropShip.playersFirstOrder)
                    {
                        dropShip.shipTimer = 39f;
                    }
                }
            }*/
        }
    }
}
