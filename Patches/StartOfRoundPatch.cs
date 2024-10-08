using BepInEx.Logging;
using HarmonyLib;

namespace MilkMolars
{
    [HarmonyPatch(typeof(StartOfRound))]
    internal class StartOfRoundPatch
    {
        private static ManualLogSource logger = Plugin.LoggerInstance;

        [HarmonyPostfix]
        [HarmonyPatch(nameof(StartOfRound.AutoSaveShipData))]
        public static void AutoSaveShipDataPostfix(StartOfRound __instance)
        {
            logger.LogDebug("AutoSaveShipDataPostfix called");
            NetworkHandler.SaveDataToFile();
        }

        [HarmonyPrefix]
        [HarmonyPatch(nameof(StartOfRound.playersFiredGameOver))]
        public static void playersFiredGameOverPrefix(StartOfRound __instance)
        {
            logger.LogDebug("In EndPlayersFiredSequenceClientRpcPostfix");

            NetworkHandler.Instance.ResetAllData();
        }
    }
}
