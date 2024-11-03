using BepInEx.Logging;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;

namespace MilkMolars
{
    public abstract class MilkMolarUpgrade
    {
        private static ManualLogSource logger = Plugin.LoggerInstance;

        private const string upgradePoint = "\u2B1C";
        private const string filledUpgradePoint = "\u2B1B";
        private const string tooth = "🦷";

        public enum UpgradeType
        {
            TierNumber,
            TierPercent,
            OneTimeUnlock,
            Repeatable
        }

        public string Name { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
        public UpgradeType Type { get; set; }
        public bool Shared { get; set; }
        public bool Visible { get; set; }

        public int UnlockCost { get; set; }
        public bool Unlocked;

        public int CurrentTier { get; private set; } = 0;
        public float[] AmountPerTier { get; private set; }
        public int[] CostsPerTier { get; private set; }

        public bool FullyUpgraded;

        [JsonIgnore]
        public int Count { get { return CostsPerTier.Length; } }
        [JsonIgnore]
        public int MaxTier { get { return Count - 1; } }
        [JsonIgnore]
        public float CurrentTierPercent { get { return AmountPerTier[CurrentTier] / 100; } }
        [JsonIgnore]
        public float CurrentTierAmount { get { return AmountPerTier[CurrentTier]; } }
        [JsonIgnore]
        public int NextTierCost { get { return CostsPerTier[CurrentTier + 1]; } }

        /*public MilkMolarUpgrade()
        {

        }

        public MilkMolarUpgrade(string name, string title, UpgradeType type, string tierString = null)
        {
            logger.LogDebug("Adding " + name);
            this.name = name;
            this.title = title;
            this.type = type;

            if (tierString != null)
            {
                GetTiersFromString(tierString);
            }
        }

        public MilkMolarUpgrade(string name, string title, UpgradeType type, int cost)
        {
            logger.LogDebug("Adding " + name);
            this.name = name;
            this.title = title;
            this.type = type;
            this.cost = cost;
        }*/

        public virtual bool CanBuyUpgrade()
        {
            int price = GetCurrentPrice();

            if (Shared)
            {
                int megaMolars = NetworkHandler.MegaMilkMolars.Value;
                return megaMolars >= price && !FullyUpgraded;
            }
            else
            {
                return MilkMolarController.MilkMolars >= price && !FullyUpgraded;
            }
        }

        public virtual void ActivateRepeatableUpgrade()
        {
            logger.LogDebug("Activating Repeatable Upgrade " + Name);
        }

        public virtual void ActivateOneTimeUpgrade()
        {
            logger.LogDebug("Activating One Time Upgrade for " + Name);
            FullyUpgraded = true;
            Unlocked = true;
        }

        public virtual void ActivateCurrentTierUpgrade()
        {
            logger.LogDebug("Activating Current Tier Upgrade for " + Name);
        }

        public int GetCurrentPrice()
        {
            if (Type == UpgradeType.TierNumber || Type == UpgradeType.TierPercent)
            {
                return NextTierCost;
            }
            else
            {
                return UnlockCost;
            }
        }

        public void GetTiersFromString(string configString)
        {
            string[] tiers = configString.Split(',');
            CostsPerTier = new int[tiers.Length];
            AmountPerTier = new float[tiers.Length];
            for (int i = 0; i < tiers.Length; i++)
            {
                string[] tierSplit = tiers[i].Split(':');
                CostsPerTier[i] = int.Parse(tierSplit[0].Trim());
                AmountPerTier[i] = float.Parse(tierSplit[1].Trim());
            }
        }

        public void GoToNextTier()
        {
            logger.LogDebug("Going to next tier");
            Unlocked = true;
            CurrentTier++;
            if (CurrentTier >= MaxTier)
            {
                FullyUpgraded = true;
            }
        }

        public string GetUpgradeString()
        {
            string upgradeString = "";

            switch (Type)
            {
                case MilkMolarUpgrade.UpgradeType.TierNumber:
                    if (FullyUpgraded)
                    {
                        upgradeString = $"{Title} (Fully Upgraded) " +
                            $"{AmountPerTier[MaxTier]}";
                        upgradeString += $"\n{GetUpgradeSymbols()}";
                    }
                    else
                    {
                        upgradeString = $"{NextTierCost}{tooth} " +
                            $"{Title}: " +
                            $"{AmountPerTier[CurrentTier]} -> {AmountPerTier[CurrentTier + 1]}";
                        upgradeString += $"\n{GetUpgradeSymbols()}";
                    }
                    break;
                case MilkMolarUpgrade.UpgradeType.TierPercent:
                    if (FullyUpgraded)
                    {
                        upgradeString = $"{Title} (Fully Upgraded) " +
                            $"{AmountPerTier[MaxTier]}%";
                        upgradeString += $"\n{GetUpgradeSymbols()}";
                    }
                    else
                    {
                        upgradeString = $"{NextTierCost}{tooth} " +
                            $"{Title}: " +
                            $"{AmountPerTier[CurrentTier]}% -> {AmountPerTier[CurrentTier + 1]}%";
                        upgradeString += $"\n{GetUpgradeSymbols()}";
                    }
                    break;
                case MilkMolarUpgrade.UpgradeType.OneTimeUnlock:
                    if (FullyUpgraded) { upgradeString = $"{Title} (Fully Upgraded)"; }
                    else { upgradeString = $"{UnlockCost}{tooth} {Title}"; }
                    break;
                case MilkMolarUpgrade.UpgradeType.Repeatable:
                    upgradeString = $"{UnlockCost}{tooth} {Title} (Repeatable)";
                    break;
                default:
                    break;
            }

            return upgradeString;
        }

        private string GetUpgradeSymbols()
        {
            string text = "";
            for (int i = 0; i < CurrentTier; i++)
            {
                text += filledUpgradePoint;
            }
            for (int i = CurrentTier; i < MaxTier; i++)
            {
                text += upgradePoint;
            }
            return text;
        }
    }
}
