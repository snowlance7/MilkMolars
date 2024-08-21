using BepInEx;
using BepInEx.Configuration;
using BepInEx.Logging;
using GameNetcodeStuff;
using HarmonyLib;
using InteractiveTerminalAPI.UI;
using LethalLib.Modules;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using TMPro;
using UnityEngine;

// malco-Lategame_Upgrades-3.9.14

namespace MilkMolars
{
    [BepInPlugin(modGUID, modName, modVersion)]
    [BepInDependency(LethalLib.Plugin.ModGUID)]
    [BepInDependency(MoreShipUpgrades.PluginInfo.PLUGIN_GUID, BepInDependency.DependencyFlags.SoftDependency)]
    public class Plugin : BaseUnityPlugin
    {
        public const string modGUID = "Snowlance.MilkMolars";
        public const string modName = "MilkMolars";
        public const string modVersion = "1.0.0";

        public static Plugin PluginInstance;
        public static ManualLogSource LoggerInstance;
        private readonly Harmony harmony = new Harmony(modGUID);
        public static PlayerControllerB localPlayer { get { return GameNetworkManager.Instance.localPlayerController; } }
        public static ulong localPlayerId { get { return GameNetworkManager.Instance.localPlayerController.actualClientId; } } // TODO: Change this back to steamId

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

        // Milk Molar Upgrades
        public static ConfigEntry<string> configShovelDamageUpgrade;
        public static ConfigEntry<string> configDamageResistanceUpgrade;
        public static ConfigEntry<string> configSprintSpeedUpgrade;
        public static ConfigEntry<string> configSprintEnduranceUpgrade;
        //public static ConfigEntry<string> configSprintRegenerationUpgrade;
        public static ConfigEntry<string> configJumpHeightUpgrade;
        public static ConfigEntry<string> configCarryWeightUpgrade;
        public static ConfigEntry<string> configIncreasedInventorySizeUpgrade;
        public static ConfigEntry<string> configCritChanceUpgrade;
        public static ConfigEntry<string> configClimbSpeedUpgrade;
        public static ConfigEntry<string> configFallDamageReductionUpgrade;
        public static ConfigEntry<string> configHealthRegenUpgrade;
        public static ConfigEntry<string> configBailOutUpgrade;
        public static ConfigEntry<string> configCorporateKickbackUpgrade;

        // Mega Milk Molar Upgrades
        public static ConfigEntry<int> configSignalTransmitterUpgrade;
        //public static ConfigEntry<string> configIncreasedShopDealsUpgrade;
        public static ConfigEntry<int> configItemDropshipLandingSpeedUpgrade;
        public static ConfigEntry<string> configKeepItemsOnShipChanceUpgrade;
        public static ConfigEntry<string> configTravelDiscountUpgrade;
        //public static ConfigEntry<string> configCompanyCruiserHealthUpgrade;
        //public static ConfigEntry<string> configCompanyCruiserAccelerationUpgrade;
        //public static ConfigEntry<string> configCompanyCruiserSpeedUpgrade;
        //public static ConfigEntry<string> configCompanyCruiserTurningUpgrade;
        //public static ConfigEntry<string> configCompanyCruiserDamageUpgrade;
        public static ConfigEntry<int> configRevivePlayerUpgrade;

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

            // Configs
            
            // General Configs
            configLGUCompatible = Config.Bind("LGU Compatibility", "LGU Compatible", true, "If true, Milk Molars will be compatible with Lategame Upgrades.");
            configLGUMilkMolarContributeAmount = Config.Bind("LGU Compatibility", "Milk Molar Contribute Amount", 100f, "How much credits Milk Molars will contribute towards LGU Upgrades.");
            configLGUMegaMilkMolarContributeAmount = Config.Bind("LGU Compatibility", "Mega Milk Molar Contribute Amount", 100f, "How much credits Mega Milk Molars will contribute towards LGU Upgrades.");

            // Milk Molar Configs
            configMilkMolarLevelRarities = Config.Bind("Milk Molar Rarities", "Level Rarities", "ExperimentationLevel:10, AssuranceLevel:10, VowLevel:10, OffenseLevel:30, AdamanceLevel:50, MarchLevel:50, RendLevel:50, DineLevel:50, TitanLevel:80, ArtificeLevel:80, EmbrionLevel:100, All:30, Modded:30", "Rarities for each level. See default for formatting.");
            configMilkMolarCustomLevelRarities = Config.Bind("Milk Molar Rarities", "Custom Level Rarities", "", "Rarities for modded levels. Same formatting as level rarities.");
            configMilkMolarSpawnAmount = Config.Bind("Milk Molar Rarities", "Spawn Amount Min Max", "ExperimentationLevel:1-2, AssuranceLevel:1-4, VowLevel:1-5, OffenseLevel:3-5, AdamanceLevel:4-8, MarchLevel:2-6, RendLevel:3-10, DineLevel:4-10, TitanLevel:6-15, ArtificeLevel:7-14, EmbrionLevel:10-20, All:1-5, Modded:1-10", "The minimum and maximum amount of Milk Molars to spawn after scrap spawns in round for each moon.");
            // TODO: Set up min max spawn amount
            configMilkMolarActivateMethod = Config.Bind("Milk Molar", "Activate Method", ActivateMethod.Use, "Activation method for Milk Molars.\nGrab: Grabbing the Milk Molar will activate it.\nUse: Using the Milk Molar while its in your inventory will activate it.\nShip: Milk Molar will activate when brought inside the ship.\nSell: Milk Molar will activate when sold to the company.");
            configSharedMilkMolars = Config.Bind("Milk Molar", "Shared Milk Molars", true, "By default (true), Milk Molars will give 1 upgrade point to each player when activated. Setting this to false will only give 1 upgrade point to the player who activated it.");
            configUpgradePointsToFinder = Config.Bind("Milk Molar", "Upgrade Points to Finder", false, "This only works when configMilkMolarActivateMethod is SHIP or SELL and configSharedMilkMolars is false. Setting this to true will only give an upgrade point to the first person who held the Milk Molar when activating it.");

            // Mega Milk Molar Configs
            configMegaMilkMolarLevelRarities = Config.Bind("Mega Milk Molar Rarities", "Mega Milk Molar Level Rarities", "ExperimentationLevel:10, AssuranceLevel:10, VowLevel:10, OffenseLevel:30, AdamanceLevel:50, MarchLevel:50, RendLevel:50, DineLevel:50, TitanLevel:80, ArtificeLevel:80, EmbrionLevel:100, All:30, Modded:30", "Rarities for each level. See default for formatting.");
            configMegaMilkMolarCustomLevelRarities = Config.Bind("Mega Milk Molar Rarities", "Mega Milk Molar Custom Level Rarities", "", "Rarities for modded levels. Same formatting as level rarities.");
            configMegaMilkMolarSpawnAmount = Config.Bind("Mega Milk Molar Rarities", "Spawn Amount Min Max", "ExperimentationLevel:1-2, AssuranceLevel:1-3, VowLevel:1-3, OffenseLevel:2-3, AdamanceLevel:3-5, MarchLevel:2-3, RendLevel:3-5, DineLevel:3-6, TitanLevel:5-10, ArtificeLevel:5-12, EmbrionLevel:7-15, All:1-3, Modded:1-5", "The minimum and maximum amount of Mega Milk Molars to spawn after scrap spawns in round for each moon.");

            configMegaMilkMolarActivateMethod = Config.Bind("Mega Milk Molar", "Activate Method", ActivateMethod.Use, "Activation method for Mega Milk Molars.\nGrab: Grabbing the Mega Milk Molar will activate it.\nUse: Using the Mega Milk Molar while its in your inventory will activate it.\nShip: Mega Milk Molar will activate when brought inside the ship.\nSell: Mega Milk Molar will activate when sold to the company.");

            // Client Configs
            configPlaySound = Config.Bind("Client Settings", "Play Sound", true, "Play sound when milk molar is activated");

            // Milk Molar Upgrades Configs
            configShovelDamageUpgrade = Config.Bind("Milk Molar Upgrades", "Shovel Damage Upgrade", "0:1, 5:2, 10:3, 18:4", "Increases the damage of the shovel. Default is 1");
            configDamageResistanceUpgrade = Config.Bind("Milk Molar Upgrades", "Damage Resistance Upgrade", "0:0, 1:5, 2:10, 3:15, 4:20, 5:25, 6:30, 7:35, 8:40, 9:45, 10:50", "Percentage damage reduction. Default is 0");
            configSprintSpeedUpgrade = Config.Bind("Milk Molar Upgrades", "Sprint Speed Upgrade", "0:0.5, 1:0.51, 2:0.52, 3:0.54, 5:0.56, 7:0.57, 10:0.6, 15:0.63, 20:0.66, 30:0.7", "Default is 0.5");
            configSprintEnduranceUpgrade = Config.Bind("Milk Molar Upgrades", "Sprint Endurance Upgrade", "0:5, 1:6, 2:8, 3:10, 4:12, 5:14, 6:16, 10:20, 15:25, 20:30", "Increases sprint time. Default is 5");
            //configSprintRegenerationUpgrade = Config.Bind("Milk Molar Upgrades", "Sprint Regeneration Upgrade", "", "");
            configJumpHeightUpgrade = Config.Bind("Milk Molar Upgrades", "Jump Height Upgrade", "0:13, 3:14, 4:16, 5:18, 6:20, 7:22, 8:25", "Jump force applied to player when jumping. Default is 5.");
            configCarryWeightUpgrade = Config.Bind("Milk Molar Upgrades", "Carry Weight Upgrade", "0:0, 1:2, 2:4, 3:6, 4:8, 5:10, 6:12, 7:14, 8:16, 9:18, 10:20", "Percent carry weight upgrade. Default is 0");
            configIncreasedInventorySizeUpgrade = Config.Bind("Milk Molar Upgrades", "Increased Inventory Upgrade", "0:4, 10:5, 15:6, 20:7, 25:8", "How many item slots the player has. Default is 4.");
            configCritChanceUpgrade = Config.Bind("Milk Molar Upgrades", "Crit Chance Upgrade", "0:0, 5:1, 6:3, 7:5, 10:7, 15:10, 20:15, 30:25", "Percent chance of critical hits. Critical hits will deal double damage. Default is 0");
            configClimbSpeedUpgrade = Config.Bind("Milk Molar Upgrades", "Climb Speed Upgrade", "0:4, 5:5, 6:6, 7:7, 8:8, 9:9, 10:10", "Climb speed when climbing ladders. Default is 4.");
            configFallDamageReductionUpgrade = Config.Bind("Milk Molar Upgrades", "Fall Damage Reduction Upgrade", "0:0, 1:5, 2:10, 4:15, 5:20", "Percent damage reduction from falling. Default is 0");
            configHealthRegenUpgrade = Config.Bind("Milk Molar Upgrades", "Health Regen Upgrade", "0:0, 10:1, 20:2, 30:3, 35:4, 40:5", "Health given per second");
            configBailOutUpgrade = Config.Bind("Milk Molar Upgrades", "Bail Out Upgrade", "0:0, 5:1, 10:5, 15:10, 20:20, 25:30", "Chance to activate upgrade when player takes damage. When activated, the players damage will be negated. Default is 0.");
            configCorporateKickbackUpgrade = Config.Bind("Milk Molar Upgrades", "Corporate Kickback Upgrade", "0:0, 5:2.5, 10:5, 15:6, 20:7, 25:8, 30:9, 35:10, 40:25", "Chance to activate upgrade when player takes damage. When activated, the player will heal");

            // Mega Milk Molar Upgrades Configs
            configSignalTransmitterUpgrade = Config.Bind("Mega Milk Molar Upgrades", "Signal Transmitter Upgrade", 5, "Cost of the Signal Transmitter upgrade. One time purchase.");
            //configIncreasedShopDealsUpgrade = Config.Bind("Mega Milk Molar Upgrades", "Increased Shop Deals Upgrade", "", "");
            configItemDropshipLandingSpeedUpgrade = Config.Bind("Mega Milk Molar Upgrades", "Item Dropship Landing Speed Upgrade", 10, "");
            configKeepItemsOnShipChanceUpgrade = Config.Bind("Mega Milk Molar Upgrades", "Keep Items On Ship Upgrade", "0:0, 5:25, 10:50, 15:75, 20:100", "Chance of keeping scrap items on ship. Default is 0");
            configTravelDiscountUpgrade = Config.Bind("Mega Milk Molar Upgrades", "Travel Discount Upgrade", "0:0, 5:25, 10:50, 20:75", "Percent travel discount. Default is 0");
            //configCompanyCruiserHealthUpgrade = Config.Bind("Mega Milk Molar Upgrades", "Company Cruiser Health Upgrade", "", "");
            //configCompanyCruiserAccelerationUpgrade = Config.Bind("Mega Milk Molar Upgrades", "Company Cruiser Acceleration Upgrade", "", "");
            //configCompanyCruiserSpeedUpgrade = Config.Bind("Mega Milk Molar Upgrades", "Company Cruiser Speed Upgrade", "", "");
            //configCompanyCruiserTurningUpgrade = Config.Bind("Mega Milk Molar Upgrades", "Company Cruiser Turning Upgrade", "", "");
            //configCompanyCruiserDamageUpgrade = Config.Bind("Mega Milk Molar Upgrades", "Company Cruiser Damage Upgrade", "", "");
            configRevivePlayerUpgrade = Config.Bind("Mega Milk Molar Upgrades", "Revive Player Upgrade", 5, "Repeatable upgrade. Revives the player selected on the ship monitor.");
            // insanity drain when together
            // increased unlockables
            // add more days to quota
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

            InteractiveTerminalManager.RegisterApplication<UpgradeTerminalUIPlayer>(["mmu", "milk molar upgrades", "milk molar", "mm player"], caseSensitive: false);
            InteractiveTerminalManager.RegisterApplication<UpgradeTerminalUIGroup>(["mmmu", "mega milk molar upgrades", "mega molar", "mega", "mm group"], caseSensitive: false);

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
            Type[] types;
            try
            {
                types = Assembly.GetExecutingAssembly().GetTypes();
            }
            catch (ReflectionTypeLoadException ex)
            {
                types = ex.Types.Where(t => t != null).ToArray();
            }

            foreach (var type in types)
            {
                var methods = type.GetMethods(BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Static);
                foreach (var method in methods)
                {
                    LoggerInstance.LogDebug(method.Name);
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
