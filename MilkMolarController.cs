using GameNetcodeStuff;
using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine.PlayerLoop;
using static MilkMolars.Plugin;

namespace MilkMolars
{
    public static class MilkMolarController
    {
        public static int MilkMolars = 0;

        public static void AddMilkMolar(PlayerControllerB player)
        {
            if (configSharedMilkMolars.Value)
            {
                NetworkHandler.Instance.AddMilkMolarServerRpc(player.actualClientId);
                MilkMolars++;
                HUDManager.Instance.DisplayTip("Milk Molar found!", "You found a Milk Molar! Open the upgrade menu to spend your Milk Molars. (M by default)");
            }
            else
            {
                MilkMolars++;
                HUDManager.Instance.DisplayTip("Milk Molar found!", "You found a Milk Molar! Open the upgrade menu to spend your Milk Molars. (M by default)");
            }
        }

        public static void AddMegaMilkMolar(PlayerControllerB player)
        {
            NetworkHandler.Instance.AddMegaMilkMolarServerRpc(player.actualClientId);
            HUDManager.Instance.DisplayTip("Mega Milk Molar found!", "You found a Mega Milk Molar! Open the upgrade menu to spend Mega Milk Molars for the group. (M by default)");
        }
    }
}