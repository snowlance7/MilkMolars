using BepInEx.Logging;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;

namespace MilkMolars
{
    


    public class MilkMolarUpgrade
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
            Repeatable,
            LGUTier,
            LGUOneTimeUnlock
        }

        public string name;
        public string title;
        public string description;
        public UpgradeType type;

        public int cost;
        public bool unlocked;

        public int currentTier = 0;
        public float[] amountPerTier;
        public int[] costsPerTier;

        public bool fullyUpgraded;

        public bool LGUUpgrade = false;

        [JsonIgnore]
        public int count { get { return costsPerTier.Length; } }
        [JsonIgnore]
        public int maxTier { get { return count - 1; } }
        [JsonIgnore]
        public float currentTierPercent { get { return amountPerTier[currentTier] / 100; } }
        [JsonIgnore]
        public float currentTierAmount { get { return amountPerTier[currentTier]; } }
        [JsonIgnore]
        public int nextTierCost { get { return costsPerTier[currentTier + 1]; } }

        public MilkMolarUpgrade()
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
        }

        public virtual void ActivateRepeatableUpgrade()
        {

        }

        public virtual void ActivateOneTimeUpgrade()
        {
            fullyUpgraded = true;
            unlocked = true;
        }

        public virtual void ActivateCurrentTierUpgrade()
        {

        }

        public void GetTiersFromString(string configString)
        {
            string[] tiers = configString.Split(',');
            costsPerTier = new int[tiers.Length];
            amountPerTier = new float[tiers.Length];
            for (int i = 0; i < tiers.Length; i++)
            {
                string[] tierSplit = tiers[i].Split(':');
                costsPerTier[i] = int.Parse(tierSplit[0].Trim());
                amountPerTier[i] = float.Parse(tierSplit[1].Trim());
            }
        }

        public void GoToNextTier()
        {
            logger.LogDebug("Going to next tier");
            unlocked = true;
            currentTier++;
            if (currentTier >= maxTier)
            {
                fullyUpgraded = true;
            }
        }

        public string GetUpgradeString()
        {
            logger.LogDebug("Getting upgrade string");
            string upgradeString = "";

            switch (type)
            {
                case MilkMolarUpgrade.UpgradeType.TierNumber:
                    if (fullyUpgraded)
                    {
                        upgradeString = $"{title} (Fully Upgraded) " +
                            $"{amountPerTier[maxTier]}";
                        upgradeString += $"\n{GetUpgradeSymbols()}";
                    }
                    else
                    {
                        upgradeString = $"{nextTierCost}{tooth} " +
                            $"{title}: " +
                            $"{amountPerTier[currentTier]} -> {amountPerTier[currentTier + 1]}";
                        upgradeString += $"\n{GetUpgradeSymbols()}";
                    }
                    break;
                case MilkMolarUpgrade.UpgradeType.TierPercent:
                    if (fullyUpgraded)
                    {
                        upgradeString = $"{title} (Fully Upgraded) " +
                            $"{amountPerTier[maxTier]}%";
                        upgradeString += $"\n{GetUpgradeSymbols()}";
                    }
                    else
                    {
                        upgradeString = $"{nextTierCost}{tooth} " +
                            $"{title}: " +
                            $"{amountPerTier[currentTier]}% -> {amountPerTier[currentTier + 1]}%";
                        upgradeString += $"\n{GetUpgradeSymbols()}";
                    }
                    break;
                case MilkMolarUpgrade.UpgradeType.OneTimeUnlock:
                    if (fullyUpgraded) { upgradeString = $"{title} (Fully Upgraded)"; }
                    else { upgradeString = $"{cost}{tooth} {title}"; }
                    break;
                case MilkMolarUpgrade.UpgradeType.Repeatable:
                    upgradeString = $"{cost}{tooth} {title} (Repeatable)";
                    break;
                case MilkMolarUpgrade.UpgradeType.LGUTier:
                    if (fullyUpgraded)
                    {
                        upgradeString = $"{title} (Fully Upgraded)\n ";
                        upgradeString += $"\n{GetUpgradeSymbols()}";
                    }
                    else
                    {
                        upgradeString = $"{nextTierCost}{tooth} {title}";
                        upgradeString += $"\n{GetUpgradeSymbols()}";
                    }
                    break;
                case MilkMolarUpgrade.UpgradeType.LGUOneTimeUnlock:
                    if (fullyUpgraded) { upgradeString = $"{title} (Fully Upgraded)\n "; }
                    else { upgradeString = $"{cost}{tooth} {title}\n "; }
                    break;
                default:
                    break;
            }

            return upgradeString;
        }

        private string GetUpgradeSymbols()
        {
            string text = "";
            for (int i = 0; i < currentTier; i++)
            {
                text += filledUpgradePoint;
            }
            for (int i = currentTier; i < maxTier; i++)
            {
                text += upgradePoint;
            }
            return text;
        }
    }
}
