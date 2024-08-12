using BepInEx.Logging;
using GameNetcodeStuff;
using System.Collections.Generic;
using System.Linq;
using Unity.Netcode;
using UnityEngine;
using static MilkMolars.Plugin;

namespace MilkMolars
{
    internal class MegaMilkMolarBehavior : PhysicsProp
    {
        private static ManualLogSource logger = Plugin.LoggerInstance;

        public AudioSource ItemSFX;
        public ScanNodeProperties scanNode;

        int ActivationMethod;

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

            ActivationMethod = configMegaMilkMolarActivateMethod.Value;

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

            if (ActivationMethod == 3 && isInShipRoom && playerHeldBy != null && playerHeldBy.isInHangarShipRoom)
            {
                ActivateMolar();
            }
        }

        public override void InteractItem()
        {
            base.InteractItem();
            
            if (ActivationMethod == 1)
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
            if (configPlaySound.Value) { ItemSFX.Play(); }
            playerHeldBy.DespawnHeldObject();

            MilkMolarController.AddMegaMilkMolar();
        }
    }
}