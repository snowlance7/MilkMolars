﻿using BepInEx.Logging;
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

        ActivateMethod ActivationMethod;

        /* Activation Methods
         * 1 - Grab
         * 2 - Use
         * 3 - Ship
         * 4 - Sell
         */

        public override void Start()
        {
            if (UnityEngine.Random.Range(0, 2) == 0)
            {
                itemProperties.restingRotation = new Vector3(90, 0, 0);
            }

            base.Start();

            ItemSFX.enabled = true;
            scanNode.subText = "";

            ActivationMethod = configMegaMilkMolarActivateMethod.Value;

            if (ActivationMethod == ActivateMethod.Use)
            {
                itemProperties.toolTips[0] = "Activate [LMB]";
            }
            if (ActivationMethod == ActivateMethod.Grab)
            {
                customGrabTooltip = "Activate [E]";
            }
        }

        public override void Update()
        {
            base.Update();

            if (ActivationMethod == ActivateMethod.ReturnToShip && isInShipRoom && playerHeldBy != null && playerHeldBy.isInHangarShipRoom)
            {
                ActivateMolar();
            }
        }

        public override void InteractItem()
        {
            base.InteractItem();
            
            if (ActivationMethod == ActivateMethod.Grab)
            {
                ActivateMolar();
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
            if (configPlaySound.Value) { ItemSFX.Play(); }
            playerHeldBy.DespawnHeldObject();

            MilkMolarController.AddMegaMilkMolar();
        }
    }
}