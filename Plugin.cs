using BepInEx;
using BepInEx.Configuration;
using BepInEx.Logging;
using GameNetcodeStuff;
using HarmonyLib;
using LethalLib.Modules;
using Steamworks.Data;
using Steamworks.Ugc;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using Unity.Netcode;
using Unity.Networking.Transport;
using UnityEngine;
using UnityEngine.Rendering;

namespace MilkMolars
{
    [BepInPlugin(modGUID, modName, modVersion)]
    [BepInDependency(LethalLib.Plugin.ModGUID)]
    public class Plugin : BaseUnityPlugin
    {
        private const string modGUID = "Snowlance.MilkMolars";
        private const string modName = "MilkMolars";
        private const string modVersion = "1.0.0";

        public static Plugin PluginInstance;
        public static ManualLogSource LoggerInstance;
        private readonly Harmony harmony = new Harmony(modGUID);
        public static PlayerControllerB localPlayer { get { return StartOfRound.Instance.localPlayerController; } }

        public static AssetBundle? ModAssets;

        public static AudioClip ActivateSFX;

        // Milk Molar Configs
        public static ConfigEntry<string> configMilkMolarLevelRarities;
        public static ConfigEntry<string> configMilkMolarCustomLevelRarities;
        public static ConfigEntry<string> configMilkMolarSpawnAmount;

        public static ConfigEntry<int> configMilkMolarActivateMethod;
        public static ConfigEntry<bool> configSharedMilkMolars;
        public static ConfigEntry<bool> configUpgradePointsToFinder;

        // Mega Milk Molar Configs
        public static ConfigEntry<string> configMegaMilkMolarLevelRarities;
        public static ConfigEntry<string> configMegaMilkMolarCustomLevelRarities;
        public static ConfigEntry<string> configMegaMilkMolarSpawnAmount;

        public static ConfigEntry<int> configMegaMilkMolarActivateMethod;

        // Client Configs
        public static ConfigEntry<int> configNotifyMethod;
        public static ConfigEntry<bool> configPlaySound;

        // Upgrades
        public static ConfigEntry<string> configShovelDamageUpgrade;
        public static ConfigEntry<string> configDamageResistanceUpgrade;
        public static ConfigEntry<string> configSprintSpeedUpgrade;
        public static ConfigEntry<string> configSprintEnduranceUpgrade;
        // TODO: Continue here

        private void Awake()
        {
            if (PluginInstance == null)
            {
                PluginInstance = this;
            }

            LoggerInstance = PluginInstance.Logger;

            harmony.PatchAll();

            InitializeNetworkBehaviours();

            // Configs

            // Milk Molar Configs
            configMilkMolarLevelRarities = Config.Bind("Milk Molar Rarities", "Level Rarities", "ExperimentationLevel:10, AssuranceLevel:10, VowLevel:10, OffenseLevel:30, AdamanceLevel:50, MarchLevel:50, RendLevel:50, DineLevel:50, TitanLevel:80, ArtificeLevel:80, EmbrionLevel:100, All:30, Modded:30", "Rarities for each level. See default for formatting.");
            configMilkMolarCustomLevelRarities = Config.Bind("Milk Molar Rarities", "Custom Level Rarities", "", "Rarities for modded levels. Same formatting as level rarities.");
            configMilkMolarSpawnAmount = Config.Bind("Milk Molar Rarities", "Spawn Amount Min Max", "ExperimentationLevel:1-2, AssuranceLevel:1-4, VowLevel:1-5, OffenseLevel:3-5, AdamanceLevel:4-8, MarchLevel:2-6, RendLevel:3-10, DineLevel:4-10, TitanLevel:6-15, ArtificeLevel:7-14, EmbrionLevel:10-20, All:1-5, Modded:1-10", "The minimum and maximum amount of Milk Molars to spawn after scrap spawns in round for each moon.");

            configMilkMolarActivateMethod = Config.Bind("Milk Molar", "Activate Method", 2, "Activation method for Milk Molars. 1 - Grab, 2 - Use, 3 - Ship, 4 - Sell.\nGrab: Grabbing the Milk Molar will activate it.\nUse: Using the Milk Molar while its in your inventory will activate it.\nShip: Milk Molar will activate when brought inside the ship.\nSell: Milk Molar will activate when sold to the company.");
            configSharedMilkMolars = Config.Bind("Milk Molar", "Shared Milk Molars", true, "By default (true), Milk Molars will give 1 upgrade point to each player when activated. Setting this to false will only give 1 upgrade point to the player who activated it.");
            configUpgradePointsToFinder = Config.Bind("Milk Molar", "Upgrade Points to Finder", false, "This only works when configMilkMolarActivateMethod is SHIP or SELL and configSharedMilkMolars is false. Setting this to true will only give an upgrade point to the first person who held the Milk Molar when activating it.");

            // Mega Milk Molar Configs
            configMegaMilkMolarLevelRarities = Config.Bind("Mega Milk Molar Rarities", "Mega Milk Molar Level Rarities", "ExperimentationLevel:10, AssuranceLevel:10, VowLevel:10, OffenseLevel:30, AdamanceLevel:50, MarchLevel:50, RendLevel:50, DineLevel:50, TitanLevel:80, ArtificeLevel:80, EmbrionLevel:100, All:30, Modded:30", "Rarities for each level. See default for formatting.");
            configMegaMilkMolarCustomLevelRarities = Config.Bind("Mega Milk Molar Rarities", "Mega Milk Molar Custom Level Rarities", "", "Rarities for modded levels. Same formatting as level rarities.");
            configMegaMilkMolarSpawnAmount = Config.Bind("Mega Milk Molar Rarities", "Spawn Amount Min Max", "ExperimentationLevel:1-2, AssuranceLevel:1-3, VowLevel:1-3, OffenseLevel:2-3, AdamanceLevel:3-5, MarchLevel:2-3, RendLevel:3-5, DineLevel:3-6, TitanLevel:5-10, ArtificeLevel:5-12, EmbrionLevel:7-15, All:1-3, Modded:1-5", "The minimum and maximum amount of Mega Milk Molars to spawn after scrap spawns in round for each moon.");

            configMilkMolarActivateMethod = Config.Bind("Mega Milk Molar", "Activate Method", 3, "Activation method for Mega Milk Molars. 1 - Grab, 2 - Use, 3 - Ship, 4 - Sell.\nGrab: Grabbing the Mega Milk Molar will activate it.\nUse: Using the Mega Milk Molar while its in your inventory will activate it.\nShip: Mega Milk Molar will activate when brought inside the ship.\nSell: Mega Milk Molar will activate when sold to the company.");

            // Client Configs
            configNotifyMethod = Config.Bind("Client Settings", "Notify Method", 1, "The method in which players are notified of Milk Molar activations. 1 - Popup/DisplayTip, 2 - Chat Message, 3 - None");
            configPlaySound = Config.Bind("Client Settings", "Play Sound", true, "Play sound when milk molar is activated");

            // Loading Assets
            string sAssemblyLocation = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);

            ModAssets = AssetBundle.LoadFromFile(Path.Combine(Path.GetDirectoryName(Info.Location), "milkmolar_assets"));
            if (ModAssets == null)
            {
                Logger.LogError($"Failed to load custom assets.");
                return;
            }
            LoggerInstance.LogDebug($"Got AssetBundle at: {Path.Combine(sAssemblyLocation, "milkmolar_assets")}");

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

            // Finished
            Logger.LogInfo($"{modGUID} v{modVersion} has loaded!");
        }

        public Dictionary<Levels.LevelTypes, int> GetLevelRarities(string levelsString)
        {
            try
            {
                Dictionary<Levels.LevelTypes, int> levelRaritiesDict = new Dictionary<Levels.LevelTypes, int>();

                if (levelsString != null && levelsString != "")
                {
                    string[] levels = levelsString.Split(',');

                    foreach (string level in levels)
                    {
                        string[] levelSplit = level.Split(':');
                        if (levelSplit.Length != 2) { continue; }
                        string levelType = levelSplit[0].Trim();
                        string levelRarity = levelSplit[1].Trim();

                        if (Enum.TryParse<Levels.LevelTypes>(levelType, out Levels.LevelTypes levelTypeEnum) && int.TryParse(levelRarity, out int levelRarityInt))
                        {
                            levelRaritiesDict.Add(levelTypeEnum, levelRarityInt);
                        }
                        else
                        {
                            LoggerInstance.LogError($"Error: Invalid level rarity: {levelType}:{levelRarity}");
                        }
                    }
                }
                return levelRaritiesDict;
            }
            catch (Exception e)
            {
                Logger.LogError($"Error: {e}");
                return null;
            }
        }

        public Dictionary<string, int> GetCustomLevelRarities(string levelsString)
        {
            try
            {
                Dictionary<string, int> customLevelRaritiesDict = new Dictionary<string, int>();

                if (levelsString != null)
                {
                    string[] levels = levelsString.Split(',');

                    foreach (string level in levels)
                    {
                        string[] levelSplit = level.Split(':');
                        if (levelSplit.Length != 2) { continue; }
                        string levelType = levelSplit[0].Trim();
                        string levelRarity = levelSplit[1].Trim();

                        if (int.TryParse(levelRarity, out int levelRarityInt))
                        {
                            customLevelRaritiesDict.Add(levelType, levelRarityInt);
                        }
                        else
                        {
                            LoggerInstance.LogError($"Error: Invalid level rarity: {levelType}:{levelRarity}");
                        }
                    }
                }
                return customLevelRaritiesDict;
            }
            catch (Exception e)
            {
                Logger.LogError($"Error: {e}");
                return null;
            }
        }

        public static void DespawnItemInSlotOnClient(int itemSlot)
        {
            HUDManager.Instance.itemSlotIcons[itemSlot].enabled = false;
            localPlayer.DestroyItemInSlotAndSync(itemSlot);
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
            LoggerInstance.LogDebug("Finished initializing network behaviours");
        }
    }
}
