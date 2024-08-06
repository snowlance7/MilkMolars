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

        int ActivationMethod;
        bool Shared;

        public PlayerControllerB lastPlayerHeldBy;
        public PlayerControllerB playerFoundBy;

        /* Activation Methods
         * 1 - Grab
         * 2 - Use
         * 3 - Ship
         * 4 - Sell
         */

        public override void Start()
        {
            base.Start();

            ItemSFX.enabled = true;
            scanNode.subText = "";

            ActivationMethod = configMilkMolarActivateMethod.Value;
            Shared = configSharedMilkMolars.Value;

            if (ActivationMethod == 2)
            {
                itemProperties.toolTips[0] = "Activate [LMB]";
            }
            if (ActivationMethod == 1)
            {
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

            if (ActivationMethod == 3 && isInShipRoom && playerHeldBy != null && playerHeldBy.isInHangarShipRoom)
            {
                ActivateMolar();
            }

            if (ActivationMethod == 1 && playerHeldBy != null && isHeld && !isPocketed)
            {
                ActivateMolar();
            }
        }

        public override void ItemActivate(bool used, bool buttonDown = true)
        {
            base.ItemActivate(used, buttonDown);
            if (buttonDown)
            {
                if (ActivationMethod == 2)
                {
                    ActivateMolar();
                }
            }
        }

        public void ActivateMolar()
        {
            ItemSFX.Play();
            playerHeldBy.DespawnHeldObject();

            if (configUpgradePointsToFinder.Value)
            {
                MilkMolarController.AddMilkMolar(playerFoundBy);
            }
            else
            {
                MilkMolarController.AddMilkMolar(playerHeldBy);
            }
        }
    }
}