﻿using InteractiveTerminalAPI.UI;
using InteractiveTerminalAPI.UI.Application;
using BepInEx.Logging;
using InteractiveTerminalAPI.UI.Cursor;
using InteractiveTerminalAPI.UI.Screen;

// https://github.com/WhiteSpike/InteractiveTerminalAPI/wiki/Examples#simple-example-with-code-snippets

namespace MilkMolars
{
    internal class UpgradeTerminalUIPlayer : PageApplication
    {
        private static ManualLogSource logger = Plugin.LoggerInstance;
        
        public override void Initialization()
        {
            logger.LogDebug("Initializing UpgradeTerminalUIPlayer");

            //MilkMolarController.RefreshLGUUpgrades(mega: false);

            (MilkMolarUpgrade[][], CursorMenu[], IScreen[]) entries = GetPageEntries(MilkMolarController.MilkMolarUpgrades.ToArray());

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
                    Title = "Milk Molar Upgrades", // Title is the text that is displayed in the box on top of the screen
                    elements =
                             [
                                  new TextElement()
                                  {
                                     Text = "These upgrades will affect the player"
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
            MilkMolarUpgrade upgrade = MilkMolarController.GetUpgradeByName(upgradeName);
            if (upgrade == null) return;
            logger.LogDebug("BuyUpgrade: " + upgrade.name);

            if (MilkMolarController.BuyMilkMolarUpgrade(upgrade))
            {
                currentCursorMenu.elements[currentCursorMenu.cursorIndex].Name = upgrade.GetUpgradeString();
                logger.LogDebug("Bought upgrade " + upgrade.name);
            }
            else
            {
                UnityEngine.Object.FindObjectOfType<Terminal>().PlayTerminalAudioServerRpc(1);
            }
        }

        protected override void RemoveInputBindings()
        {
            base.RemoveInputBindings();
            MilkMolarController.InUpgradeUI = false;
        }

        protected override void AddInputBindings()
        {
            base.AddInputBindings();
            MilkMolarController.InUpgradeUI = true;
        }
    }
}
