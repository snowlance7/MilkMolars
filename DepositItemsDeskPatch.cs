using HarmonyLib;

namespace MilkMolars
{
    [HarmonyPatch(typeof(DepositItemsDesk))]
    internal class DepositItemsDeskPatch
    {
        [HarmonyPostfix]
        [HarmonyPatch(nameof(DepositItemsDesk.delayedAcceptanceOfItems))]
        public static void DepositItemsDesk_delayedAcceptanceOfItems_PostFix(DepositItemsDesk __instance,  GrabbableObject[] objectsOnDesk) // TODO: Test this
        { 
            if (!__instance.IsServer) { return; }
            // TODO: Do SellToCompany Activate method here
            foreach (var item in objectsOnDesk)
            {
                if (item == null) { continue; }

                if (item is MilkMolarBehavior molar)
                {
                    if (molar.lastPlayerHeldBy != null)
                        molar.ActivateMolarRpc((int)molar.lastPlayerHeldBy.actualClientId);
                    else
                        molar.ActivateMolarRpc();
                }

                if (item is MegaMilkMolarBehavior megamolar)
                {
                    if (megamolar.lastPlayerHeldBy != null)
                        megamolar.ActivateMolarRpc((int)megamolar.lastPlayerHeldBy.actualClientId);
                    else
                        megamolar.ActivateMolarRpc();
                }
            }
        }
    }
}
