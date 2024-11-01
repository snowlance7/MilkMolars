using System;
using System.Collections.Generic;
using System.Text;
using InteractiveTerminalAPI.UI;
using InteractiveTerminalAPI;
using InteractiveTerminalAPI.UI.Application;
using BepInEx.Logging;
using InteractiveTerminalAPI.UI.Cursor;
using InteractiveTerminalAPI.UI.Screen;
using static MilkMolars.Plugin;
using UnityEngine.Animations.Rigging;

// https://github.com/WhiteSpike/InteractiveTerminalAPI/wiki/Examples#simple-example-with-code-snippets

namespace MilkMolars.UI
{
    internal class UpgradeTerminalUIGroup : PageApplication
    {
        private static ManualLogSource logger = LoggerInstance;

        public override void Initialization()
        {
            logger.LogDebug("Initializing Upgrade Terminal UI Group");

            MilkMolarController.InUpgradeUI = false;
            MilkMolarController.InMegaUpgradeUI = true;

            //MilkMolarController.RefreshLGUUpgrades(mega: true);

            (MilkMolarUpgrade[][], CursorMenu[], IScreen[]) entries = GetPageEntries(NetworkHandler.MegaMilkMolarUpgrades.ToArray());

            MilkMolarUpgrade[][] pagesUpgrades = entries.Item1;
            CursorMenu[] cursorMenus = entries.Item2;
            IScreen[] screens = entries.Item3;

            for (int i = 0; i < pagesUpgrades.Length; i++)
            {
                CursorElement[] elements = new CursorElement[pagesUpgrades[i].Length];

                for (int j = 0; j < elements.Length; j++)
                {
                    if (pagesUpgrades[i][j] == null) continue;

                    MilkMolarUpgrade upgrade = pagesUpgrades[i][j];

                    elements[j] = new CursorElement()
                    {
                        Name = upgrade.GetUpgradeString(),
                        Action = () => BuyUpgrade(upgrade.name, j)
                    };
                }

                cursorMenus[i] = new CursorMenu()
                {
                    cursorIndex = 0,
                    elements = elements
                };
                CursorMenu cursorMenu = cursorMenus[i];
                screens[i] = new BoxedScreen()
                {
                    Title = "Mega Milk Molar Upgrades", // Title is the text that is displayed in the box on top of the screen
                    elements =
                             [
                                  new TextElement()
                                  {
                                     Text = "These upgrades will affect the entire crew"
                                  },
                                  new TextElement() // This text element is here to give space between the text and the user prompt
                                  {
                                     Text = " "
                                  },
                                  cursorMenu
                             ]
                };
            }

            currentPage = initialPage;
            currentCursorMenu = initialPage.GetCurrentCursorMenu();
            currentScreen = initialPage.GetCurrentScreen();
        }

        protected override int GetEntriesPerPage<T>(T[] entries)
        {
            return 4;
        }

        public void BuyUpgrade(string upgradeName, int index)
        {
            logger.LogDebug("Buying upgrade");
            MilkMolarUpgrade upgrade = MilkMolarController.GetUpgradeByName(upgradeName, megaUpgrade: true);
            if (upgrade == null) return;
            logger.LogDebug("BuyUpgrade: " + upgrade.name);

            if (MilkMolarController.BuyMegaMilkMolarUpgrade(upgrade))
            {
                currentCursorMenu.elements[currentCursorMenu.cursorIndex].Name = upgrade.GetUpgradeString();
                logger.LogDebug("Bought upgrade " + upgrade.name);
            }
            else
            {
                UnityEngine.Object.FindObjectOfType<Terminal>().PlayTerminalAudioServerRpc(1);
            }
        }
    }
}
