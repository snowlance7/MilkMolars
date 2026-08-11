using HarmonyLib;

namespace MilkMolars
{
    [HarmonyPatch(typeof(DepositItemsDesk))]
    internal class DepositItemsDeskPatch
    {
        [HarmonyPostfix]
        [HarmonyPatch(nameof(DepositItemsDesk.delayedAcceptanceOfItems))]
        public static void delayedAcceptanceOfItemsPostFix(DepositItemsDesk __instance,  GrabbableObject[] objectsOnDesk) // TODO: Test this
        { 
            if (!__instance.IsServer) { return; }
            // TODO: Do SellToCompany Activate method here
        }
    }
}
