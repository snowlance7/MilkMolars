using MoreShipUpgrades.UI.TerminalNodes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text.RegularExpressions;
using static MilkMolars.Plugin;

namespace MilkMolars.LGU
{
    internal class LGUCompatibility
    {
        private static readonly BepInEx.Logging.ManualLogSource logger = LoggerInstance;
        private const string upgradePoint = "\u2B1C";
        private const string filledUpgradePoint = "\u2B1B";
        private const string tooth = "🦷";

        private static bool? _enabled;

        internal static bool enabled
        {
            get
            {
                if (_enabled == null)
                {
                    _enabled = BepInEx.Bootstrap.Chainloader.PluginInfos.ContainsKey("com.malco.lethalcompany.moreshipupgrades") && configLGUCompatible.Value;
                }
                return (bool)_enabled;
            }
        }

        public static string GetDescription(string description, bool shared)
        {
            return Regex.Replace(description, @"\$(\d+)", match =>
            {
                int price = int.Parse(match.Groups[1].Value);
                int molarPrice = MilkMolarController.GetMolarPrice(price, shared);
                return tooth + molarPrice.ToString();
            });
        }
    }
}