using BepInEx;
using BepInEx.Configuration;
using BepInEx.Logging;
using GameNetcodeStuff;
using HarmonyLib;
using InteractiveTerminalAPI.UI;
using LethalLib.Modules;
using MilkMolars.LGU;
using MilkMolars.UI;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using TMPro;
using Unity.Netcode;
using UnityEngine;

namespace MilkMolars
{
    [BepInPlugin(MyPluginInfo.PLUGIN_GUID, MyPluginInfo.PLUGIN_NAME, MyPluginInfo.PLUGIN_VERSION)]
    [BepInDependency(LethalLib.Plugin.ModGUID)]
    [BepInDependency("MoreShipUpgrades", BepInDependency.DependencyFlags.SoftDependency)]
    [BepInDependency(DawnLib.PLUGIN_GUID)]
    internal class Plugin : BaseUnityPlugin
    {
        public static Plugin PluginInstance;
        public static ManualLogSource LoggerInstance;
        private readonly Harmony harmony = new Harmony(MyPluginInfo.PLUGIN_GUID);
        public static PlayerControllerB localPlayer { get { return GameNetworkManager.Instance.localPlayerController; } }
        public static ulong localPlayerId { get { return GameNetworkManager.Instance.localPlayerController.playerSteamId; } } // TODO: Change this back to steamId
        public static bool IsServerOrHost { get { return NetworkManager.Singleton.IsServer || NetworkManager.Singleton.IsHost; } }

        public static AssetBundle? ModAssets;

        public static AudioClip ActivateSFX;

        public static TMP_FontAsset NotoEmojiFA;

        // LGU Compatibility Configs
        public static ConfigEntry<bool> configLGUCompatible;
        public static ConfigEntry<float> configLGUMilkMolarContributeAmount;
        public static ConfigEntry<float> configLGUMegaMilkMolarContributeAmount;

        // Milk Molar Configs
        public static ConfigEntry<string> configMilkMolarLevelRarities;
        public static ConfigEntry<string> configMilkMolarCustomLevelRarities;
        public static ConfigEntry<string> configMilkMolarSpawnAmount;

        public static ConfigEntry<ActivateMethod> configMilkMolarActivateMethod;
        public static ConfigEntry<bool> configSharedMilkMolars;
        public static ConfigEntry<bool> configUpgradePointsToFinder;

        // Mega Milk Molar Configs
        public static ConfigEntry<string> configMegaMilkMolarLevelRarities;
        public static ConfigEntry<string> configMegaMilkMolarCustomLevelRarities;
        public static ConfigEntry<string> configMegaMilkMolarSpawnAmount;

        public static ConfigEntry<ActivateMethod> configMegaMilkMolarActivateMethod;

        // Client Configs
        public static ConfigEntry<bool> configPlaySound;
        public static ConfigEntry<float> configSoundVolume;
        public static ConfigEntry<bool> configShowNotification;
        public static ConfigEntry<float> configNotificationSize;
        public static ConfigEntry<float> configNotificationPositionX;
        public static ConfigEntry<float> configNotificationPositionY;

        public enum ActivateMethod
        {
            Grab,
            Use,
            ReturnToShip,
            SellToCompany
        }


        private void Awake()
        {
            if (PluginInstance == null)
            {
                PluginInstance = this;
            }

            LoggerInstance = PluginInstance.Logger;
            LoggerInstance.LogDebug("Loaded logger for MilkMolarsMod");

            harmony.PatchAll();
            LoggerInstance.LogDebug("Patched MilkMolarsMod");

            InitializeNetworkBehaviours();
            LoggerInstance.LogDebug("Initialized network behaviours");

            

            LoggerInstance.LogDebug("Got configs");


            // Loading Assets
            string sAssemblyLocation = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);

            ModAssets = AssetBundle.LoadFromFile(Path.Combine(Path.GetDirectoryName(Info.Location), "milkmolar_assets"));
            if (ModAssets == null)
            {
                Logger.LogError($"Failed to load custom assets.");
                return;
            }
            LoggerInstance.LogDebug($"Got AssetBundle at: {Path.Combine(sAssemblyLocation, "milkmolar_assets")}");

            NotoEmojiFA = ModAssets.LoadAsset<TMP_FontAsset>("Assets/ModAssets/MilkMolars/NotoEmoji-Regular SDF.asset");
            if (NotoEmojiFA == null) { LoggerInstance.LogError("Error: Couldnt get NotoEmojiFA from assets"); return; }
            LoggerInstance.LogDebug($"Got NotoEmojiFA");

            ActivateSFX = ModAssets.LoadAsset<AudioClip>("Assets/ModAssets/MilkMolars/Audio/milkmolaractivate.mp3");
            if (ActivateSFX == null) { LoggerInstance.LogError("Error: Couldnt get ActivateSFX from assets"); return; }
            LoggerInstance.LogDebug($"Got ActivateSFX");

            // Getting MilkMolar
            Item MilkMolar = ModAssets.LoadAsset<Item>("Assets/ModAssets/MilkMolars/MilkMolarItem.asset");
            if (MilkMolar == null) { LoggerInstance.LogError("Error: Couldnt get MilkMolar from assets"); return; }
            LoggerInstance.LogDebug($"Got MilkMolar prefab");

            LethalLib.Modules.NetworkPrefabs.RegisterNetworkPrefab(MilkMolar.spawnPrefab);
            Utilities.FixMixerGroups(MilkMolar.spawnPrefab);
            Items.RegisterScrap(MilkMolar, GetLevelRarities(configMilkMolarLevelRarities.Value), GetCustomLevelRarities(configMilkMolarCustomLevelRarities.Value));

            // Getting MegaMilkMolar
            Item MegaMilkMolar = ModAssets.LoadAsset<Item>("Assets/ModAssets/MilkMolars/MegaMilkMolarItem.asset");
            if (MegaMilkMolar == null) { LoggerInstance.LogError("Error: Couldnt get MegaMilkMolar from assets"); return; }
            LoggerInstance.LogDebug($"Got MegaMilkMolar prefab");

            LethalLib.Modules.NetworkPrefabs.RegisterNetworkPrefab(MegaMilkMolar.spawnPrefab);
            Utilities.FixMixerGroups(MegaMilkMolar.spawnPrefab);
            Items.RegisterScrap(MegaMilkMolar, GetLevelRarities(configMegaMilkMolarLevelRarities.Value), GetCustomLevelRarities(configMegaMilkMolarCustomLevelRarities.Value));

            InteractiveTerminalManager.RegisterApplication<UpgradeTerminalUI>(["mmu", "mm", "milk molar upgrades", "milk molar"], caseSensitive: false);
            if (LGUCompatibility.enabled)
            {
                InteractiveTerminalManager.RegisterApplication<LGUUpgradeTerminalUI>(["mmlgu", "mmulgu", "lgu milk molar upgrades", "milk molar lgu", "mmu lgu", "mm lgu"], caseSensitive: false);
            }

            // Finished
            Logger.LogInfo($"{MyPluginInfo.PLUGIN_NAME} v{MyPluginInfo.PLUGIN_VERSION} has loaded!");
        }

        private static void InitializeNetworkBehaviours()
        {
            var types = Assembly.GetExecutingAssembly().GetTypes();
            foreach (var type in types)
            {
                var methods = type.GetMethods(BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Static);
                foreach (var method in methods)
                {
                    var attributes = method.GetCustomAttributes(typeof(RuntimeInitializeOnLoadMethodAttribute), false);
                    if (attributes.Length > 0)
                    {
                        method.Invoke(null, null);
                    }
                }
            }
            logger.LogDebug("Finished initializing network behaviours");
        }
    }
}
