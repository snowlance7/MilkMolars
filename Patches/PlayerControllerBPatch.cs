﻿using BepInEx.Logging;
using HarmonyLib;
using InteractiveTerminalAPI.UI;
using LethalLib.Extras;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Unity.Netcode;
using UnityEngine;
using static MilkMolars.Plugin;
using Newtonsoft.Json;
using GameNetcodeStuff;

namespace MilkMolars
{
    [HarmonyPatch(typeof(PlayerControllerB))]
    internal class PlayerControllerBPatch
    {
        private static ManualLogSource logger = Plugin.LoggerInstance;

        [HarmonyPostfix]
        [HarmonyPatch(nameof(PlayerControllerB.ConnectClientToPlayerObject))]
        public static void ConnectClientToPlayerObjectPostfix(PlayerControllerB __instance) // runs on client only
        {
            logger.LogDebug("In ConnectClientToPlayerObjectPostfix");
            MilkMolarController.Init();
        }
    }
}
