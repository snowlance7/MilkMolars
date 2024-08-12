using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;

namespace MilkMolars
{
    public class MilkMolarUpgrade
    {
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

        public string name;
        public string title;
        public UpgradeType type;

        public int cost;
        public bool unlocked;

        public int currentTier = -1;
        public int maxTiers;
        public float[] amountPerTier;
        public int[] costsPerTier;

        public bool fullyUpgraded;
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
            maxTiers = tiers.Length;
            costsPerTier = new int[maxTiers];
            amountPerTier = new float[maxTiers];
            for (int i = 0; i < maxTiers; i++)
            {
                string[] tierSplit = tiers[i].Split(':');
                costsPerTier[i] = int.Parse(tierSplit[0].Trim());
                amountPerTier[i] = float.Parse(tierSplit[1].Trim());
            }
        }

        public void GoToNextTier()
        {
            currentTier++;
            if (currentTier >= maxTiers)
            {
                fullyUpgraded = true;
            }
        }

        public string GetUpgradeString()
        {
            string upgradeString = "";

            switch (type)
            {
                case MilkMolarUpgrade.UpgradeType.TierNumber:
                    if (fullyUpgraded)
                    {
                        upgradeString = $"{title} (Fully Upgraded) " +
                            $"{amountPerTier[maxTiers - 1]}";
                        upgradeString += $"\n{GetUpgradeSymbols()}";
                    }
                    else if (currentTier == -1)
                    {
                        upgradeString = $"{costsPerTier[currentTier + 1]}{tooth} " +
                            $"{title}: " +
                            $"0 -> {amountPerTier[0]}";
                        upgradeString += $"\n{GetUpgradeSymbols()}";
                    }
                    else
                    {
                        upgradeString = $"{costsPerTier[currentTier + 1]}{tooth} " +
                            $"{title}: " +
                            $"{amountPerTier[currentTier]} -> {amountPerTier[currentTier + 1]}";
                        upgradeString += $"\n{GetUpgradeSymbols()}";
                    }
                    break;
                case MilkMolarUpgrade.UpgradeType.TierPercent:
                    if (fullyUpgraded)
                    {
                        upgradeString = $"{title} (Fully Upgraded) " +
                            $"{amountPerTier[maxTiers - 1]}%";
                        upgradeString += $"\n{GetUpgradeSymbols()}";
                    }
                    else if (currentTier == -1)
                    {
                        upgradeString = $"{costsPerTier[currentTier + 1]}{tooth} " +
                            $"{title}: " +
                            $"0% -> {amountPerTier[0]}%";
                        upgradeString += $"\n{GetUpgradeSymbols()}";
                    }
                    else
                    {
                        upgradeString = $"{costsPerTier[currentTier + 1]}{tooth} " +
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
                default:
                    break;
            }

            return upgradeString;
        }

        private string GetUpgradeSymbols()
        {
            string text = "";
            if (currentTier == -1)
            {
                for (int i = 0; i < maxTiers; i++)
                {
                    text += upgradePoint;
                }
            }
            else
            {
                for (int i = 0; i < currentTier; i++)
                {
                    text += filledUpgradePoint;
                }
                for (int i = currentTier; i < maxTiers; i++)
                {
                    text += upgradePoint;
                }
            }
            return text;
        }
    }
}
