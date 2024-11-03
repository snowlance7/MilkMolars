using BepInEx.Logging;
using GameNetcodeStuff;
using System.Collections.Generic;
using System.Linq;
using Unity.Netcode;
using UnityEngine;
using static MilkMolars.Plugin;

namespace MilkMolars
{
    internal class MilkMolarBehavior : PhysicsProp
    {
        private static ManualLogSource logger = Plugin.LoggerInstance;

        public AudioSource ItemSFX;
        public ScanNodeProperties scanNode;

        ActivateMethod ActivationMethod;
        bool Shared;

        public PlayerControllerB lastPlayerHeldBy;
        public PlayerControllerB playerFoundBy;

        /* Activation Methods
         * 1 - Grab
         * 2 - Use
         * 3 - Ship
         * 4 - Sell
         */

        public override void Start() // 90 0 0
        {
            if (UnityEngine.Random.Range(0, 2) == 0)
            {
                itemProperties.restingRotation = new Vector3(90, 0, 0);
            }

            base.Start();

            ItemSFX.enabled = true;
            scanNode.subText = "";

            ActivationMethod = configMilkMolarActivateMethod.Value;
            Shared = configSharedMilkMolars.Value;

            if (ActivationMethod == ActivateMethod.Use)
            {
                itemProperties.toolTips = ["Activate [LMB]"];
            }
            if (ActivationMethod == ActivateMethod.Grab)
            {
                grabbable = false;
                customGrabTooltip = "Activate [E]";
            }
        }

        public override void Update()
        {
            base.Update();

            if (playerHeldBy != null)
            {
                lastPlayerHeldBy = playerHeldBy;

                if (playerFoundBy == null) { playerFoundBy = playerHeldBy; }
            }

            if (ActivationMethod == ActivateMethod.ReturnToShip && isInShipRoom && playerHeldBy != null && playerHeldBy.isInHangarShipRoom)
            {
                ActivateMolar();
            }
        }

        public override void InteractItem()
        {
            if (ActivationMethod == ActivateMethod.Grab)
            {
                ActivateMolar();
            }
            else
            {
                base.InteractItem();
            }
        }

        public override void ItemActivate(bool used, bool buttonDown = true)
        {
            base.ItemActivate(used, buttonDown);
            if (buttonDown)
            {
                if (ActivationMethod == ActivateMethod.Use)
                {
                    ActivateMolar();
                }
            }
        }

        public void ActivateMolar()
        {
            if (ActivationMethod != ActivateMethod.Grab)
            {
                if (configUpgradePointsToFinder.Value)
                {
                    MilkMolarController.AddMilkMolar(playerFoundBy);
                }
                else
                {
                    MilkMolarController.AddMilkMolar(playerHeldBy);
                }
            }
            else
            {
                MilkMolarController.AddMilkMolar(localPlayer);
            }

            if (playerHeldBy != null && !isPocketed)
            {
                playerHeldBy.DespawnHeldObject();
            }
            else
            {
                DespawnMolarServerRpc();
            }
        }

        [ServerRpc(RequireOwnership = false)]
        public void DespawnMolarServerRpc()
        {
            if (IsServerOrHost)
            {
                NetworkObject.Despawn(true);
            }
        }
    }
}