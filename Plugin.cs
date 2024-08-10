using BepInEx;
using BepInEx.Configuration;
using BepInEx.Logging;
using GameNetcodeStuff;
using HarmonyLib;
using InteractiveTerminalAPI.UI;
using LethalLib.Modules;
using Steamworks.Data;
using Steamworks.Ugc;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using TMPro;
using Unity.Netcode;
using Unity.Networking.Transport;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.TextCore.Text;

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

        public static TMP_FontAsset NotoEmojiFA;

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

        // Milk Molar Upgrades
        public static ConfigEntry<string> configShovelDamageUpgrade;
        public static ConfigEntry<string> configDamageResistanceUpgrade;
        public static ConfigEntry<string> configSprintSpeedUpgrade;
        public static ConfigEntry<string> configSprintEnduranceUpgrade;
        public static ConfigEntry<string> configSprintRegenerationUpgrade;
        public static ConfigEntry<string> configJumpHeightUpgrade;
        public static ConfigEntry<string> configCarryWeightStaminaCostUpgrade;
        public static ConfigEntry<string> configCarryWeightSprintSpeedUpgrade;
        public static ConfigEntry<string> configIncreasedInventorySizeUpgrade;
        public static ConfigEntry<string> configCritChanceUpgrade;
        public static ConfigEntry<string> configClimbSpeedUpgrade;
        public static ConfigEntry<string> configFallDamageReductionUpgrade;
        public static ConfigEntry<string> configHealthRegenUpgrade;

        // Mega Milk Molar Upgrades
        public static ConfigEntry<string> configSignalTransmitterUpgrade;
        public static ConfigEntry<string> configIncreasedShopDealsUpgrade;
        public static ConfigEntry<string> configLandingSpeedUpgrade;
        public static ConfigEntry<string> configItemDropshipLandingSpeedUpgrade;
        public static ConfigEntry<string> configKeepItemsOnShipChanceUpgrade;
        public static ConfigEntry<string> configTravelDiscountUpgrade;
        public static ConfigEntry<string> configTimeOnMoonUpgrade;
        public static ConfigEntry<string> configCompanyCruiserHealthUpgrade;
        public static ConfigEntry<string> configCompanyCruiserAccelerationUpgrade;
        public static ConfigEntry<string> configCompanyCruiserSpeedUpgrade;
        public static ConfigEntry<string> configCompanyCruiserTurningUpgrade;
        public static ConfigEntry<string> configCompanyCruiserDamageUpgrade;


        private void Awake()
        {
            if (PluginInstance == null)
            {
                PluginInstance = this;
            }

            LoggerInstance = PluginInstance.Logger;

            harmony.PatchAll();

            MilkMolarInputs.Init();

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

            // Milk Molar Upgrades Configs
            configShovelDamageUpgrade = Config.Bind("Milk Molar Upgrades", "Shovel Damage Upgrade", "5:1, 10:2", "");
            configDamageResistanceUpgrade = Config.Bind("Milk Molar Upgrades", "Damage Resistance Upgrade", "1:5, 2:10, 3:15, 4:20, 5:25, 6:30, 7:35, 8:40, 9:45, 10:50", "");
            configSprintSpeedUpgrade = Config.Bind("Milk Molar Upgrades", "Sprint Speed Upgrade", "", "");
            configSprintEnduranceUpgrade = Config.Bind("Milk Molar Upgrades", "Sprint Endurance Upgrade", "", "");
            configSprintRegenerationUpgrade = Config.Bind("Milk Molar Upgrades", "Sprint Regeneration Upgrade", "", "");
            configJumpHeightUpgrade = Config.Bind("Milk Molar Upgrades", "Jump Height Upgrade", "", "");
            configCarryWeightStaminaCostUpgrade = Config.Bind("Milk Molar Upgrades", "Carry Weight Stamina Cost Upgrade", "", "");
            configCarryWeightSprintSpeedUpgrade = Config.Bind("Milk Molar Upgrades", "Carry Weight Sprint Speed Upgrade", "", "");
            configIncreasedInventorySizeUpgrade = Config.Bind("Milk Molar Upgrades", "Increased Inventory Upgrade", "", "");
            configCritChanceUpgrade = Config.Bind("Milk Molar Upgrades", "Crit Chance Upgrade", "", "");
            configClimbSpeedUpgrade = Config.Bind("Milk Molar Upgrades", "Climb Speed Upgrade", "", "");
            configFallDamageReductionUpgrade = Config.Bind("Milk Molar Upgrades", "Fall Damage Reduction Upgrade", "", "");
            configHealthRegenUpgrade = Config.Bind("Milk Molar Upgrades", "Health Regen Upgrade", "", "");
            

            // Mega Milk Molar Upgrades Configs
            configSignalTransmitterUpgrade = Config.Bind("Mega Milk Molar Upgrades", "Signal Transmitter Upgrade", "", "");
            configIncreasedShopDealsUpgrade = Config.Bind("Mega Milk Molar Upgrades", "Increased Shop Deals Upgrade", "", "");
            configLandingSpeedUpgrade = Config.Bind("Mega Milk Molar Upgrades", "Landing Speed Upgrade", "", "");
            configItemDropshipLandingSpeedUpgrade = Config.Bind("Mega Milk Molar Upgrades", "Item Dropship Landing Speed Upgrade", "", "");
            configKeepItemsOnShipChanceUpgrade = Config.Bind("Mega Milk Molar Upgrades", "Keep Items On Ship Upgrade", "", "");
            configTravelDiscountUpgrade = Config.Bind("Mega Milk Molar Upgrades", "Travel Discount Upgrade", "", "");
            configTimeOnMoonUpgrade = Config.Bind("Mega Milk Molar Upgrades", "Time On Moon Upgrade", "", "");
            configCompanyCruiserHealthUpgrade = Config.Bind("Mega Milk Molar Upgrades", "Company Cruiser Health Upgrade", "", "");
            configCompanyCruiserAccelerationUpgrade = Config.Bind("Mega Milk Molar Upgrades", "Company Cruiser Acceleration Upgrade", "", "");
            configCompanyCruiserSpeedUpgrade = Config.Bind("Mega Milk Molar Upgrades", "Company Cruiser Speed Upgrade", "", "");
            configCompanyCruiserTurningUpgrade = Config.Bind("Mega Milk Molar Upgrades", "Company Cruiser Turning Upgrade", "", "");
            configCompanyCruiserDamageUpgrade = Config.Bind("Mega Milk Molar Upgrades", "Company Cruiser Damage Upgrade", "", "");

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

            InteractiveTerminalManager.RegisterApplication<UpgradeTerminalUIPlayer>(["mmu", "milk molar upgrades", "milk molar"], caseSensitive: false);

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
