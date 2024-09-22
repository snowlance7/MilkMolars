using BepInEx.Logging;
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
using System.IO;

namespace MilkMolars
{
    [HarmonyPatch(typeof(DeleteFileButton))]
    internal class DeleteFileButtonPatch
    {
        private static ManualLogSource logger = Plugin.LoggerInstance;

        [HarmonyPostfix]
        [HarmonyPatch(nameof(DeleteFileButton.DeleteFile))]
        public static void DeleteFilePostfix(DeleteFileButton __instance)
        {
            //NetworkHandler.DeleteSaveData(__instance.fileToDelete);
        }
    }
}
