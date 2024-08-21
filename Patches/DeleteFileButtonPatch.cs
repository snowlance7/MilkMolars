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
            logger.LogDebug("Deleting Milk Molar files for save: " + __instance.fileToDelete);

            string appDataPath = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
            logger.LogDebug("Got appData path at: " + appDataPath); // C:\Users\snowl\AppData\Local // C:\Users\snowl\AppData\LocalLow\ZeekerssRBLX\Lethal Company
            string path = Path.Combine(appDataPath + "Low", "ZeekerssRBLX", "Lethal Company", modName);
            logger.LogDebug("Got save path at: " + path);

            if (!Directory.Exists(path))
            {
                Directory.CreateDirectory(path);
            }

            string milkMolarsPath = Path.Combine(path, $"ClientsMilkMolars{__instance.fileToDelete}.json");
            string megaMilkMolarsPath = Path.Combine(path, $"MegaMilkMolars{__instance.fileToDelete}.json");
            string milkMolarUpgradesPath = Path.Combine(path, $"ClientsMilkMolarUpgrades{__instance.fileToDelete}.json");
            string megaMilkMolarUpgradesPath = Path.Combine(path, $"MegaMilkMolarUpgrades{__instance.fileToDelete}.json");

            if (File.Exists(milkMolarsPath)) { File.Delete(milkMolarsPath); }
            if (File.Exists(megaMilkMolarsPath)) { File.Delete(megaMilkMolarsPath); }
            if (File.Exists(milkMolarUpgradesPath)) { File.Delete(milkMolarUpgradesPath); }
            if (File.Exists(megaMilkMolarUpgradesPath)) { File.Delete(megaMilkMolarUpgradesPath); }
        }
    }
}
