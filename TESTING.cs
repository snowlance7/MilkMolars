using HarmonyLib;
using SnowyLib;
using static MilkMolars.Plugin;

/* bodyparts
 * 0 head
 * 1 right arm
 * 2 left arm
 * 3 right leg
 * 4 left leg
 * 5 chest
 * 6 feet
 * 7 right hip
 * 8 crotch
 * 9 left shoulder
 * 10 right shoulder */

namespace MilkMolars
{
    [HarmonyPatch]
    public static class TESTING
    {
        [HarmonyPostfix, HarmonyPatch(typeof(HUDManager), nameof(HUDManager.PingScan_performed))]
        public static void PingScan_performedPostFix()
        {
            if (!Utils.testing) { return; }
        }

        [StaticUpdate]
        public static void Update()
        {
            if (!Utils.testing) { return; }
        }

        [HarmonyPrefix, HarmonyPatch(typeof(HUDManager), nameof(HUDManager.SubmitChat_performed))]
        public static void SubmitChat_performedPrefix(HUDManager __instance)
        {
            try
            {
                if (!Utils.testing || !__instance.IsServer) { return; }
                string msg = __instance.chatTextField.text;
                string[] args = msg.Split(" ");

                switch (args[0])
                {
                    default:
                        break;
                }
            }
            catch (System.Exception e)
            {
                logger.LogError(e);
                return;
            }
        }
    }
}