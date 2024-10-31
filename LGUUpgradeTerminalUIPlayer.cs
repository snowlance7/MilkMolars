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
using System.Linq;
using MoreShipUpgrades.UI.TerminalNodes;
using MoreShipUpgrades.Managers;

// https://github.com/WhiteSpike/InteractiveTerminalAPI/wiki/Examples#simple-example-with-code-snippets

namespace MilkMolars
{
    internal class LGUUpgradeTerminalUIPlayer : PageApplication
    {
        private static ManualLogSource logger = Plugin.LoggerInstance;

        public override void Initialization()
        {
            logger.LogDebug("Initializing LGU Upgrade Terminal UI Player");

            MoreShipUpgrades.UI.TerminalNodes.CustomTerminalNode[] filteredNodes = MoreShipUpgrades.Managers.UpgradeBus.Instance.terminalNodes.Where(x => x.Visible && !x.SharedUpgrade && (x.UnlockPrice > 0 || (x.OriginalName == MoreShipUpgrades.UpgradeComponents.TierUpgrades.Player.NightVision.UPGRADE_NAME && (x.Prices.Length > 0 && x.Prices[0] != 0)))).ToArray();

            (CustomTerminalNode[][], CursorMenu[], IScreen[]) entries = GetPageEntries(filteredNodes);

            CustomTerminalNode[][] pagesUpgrades = entries.Item1;
            CursorMenu[] cursorMenus = entries.Item2;
            IScreen[] screens = entries.Item3;

            for (int i = 0; i < pagesUpgrades.Length; i++)
            {
                CursorElement[] elements = new CursorElement[pagesUpgrades[i].Length];

                for (int j = 0; j < elements.Length; j++)
                {
                    if (pagesUpgrades[i][j] == null) continue;

                    CustomTerminalNode upgrade = pagesUpgrades[i][j];

                    elements[j] = new CursorElement()
                    {
                        Name = GetUpgradeString(upgrade),
                        Action = () => BuyUpgrade(upgrade)
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
                    Title = "LGU Milk Molar Upgrades", // Title is the text that is displayed in the box on top of the screen
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

        private string GetUpgradeString(CustomTerminalNode node)
        {
            throw new NotImplementedException(); // TODO
        }

        protected override int GetEntriesPerPage<T>(T[] entries)
        {
            return 4;
        }

        public void BuyUpgrade(CustomTerminalNode node)
        {
            int molarPrice = GetMolarPrice(node.GetCurrentPrice());

            if (MilkMolarController.MilkMolars <= molarPrice)
            {
                if (!node.Unlocked)
                {
                    LguStore.Instance.HandleUpgrade(node);
                }
                else if (node.Unlocked && node.MaxUpgrade > node.CurrentUpgrade)
                {
                    LguStore.Instance.HandleUpgrade(node, increment: true);
                }

                currentCursorMenu.elements[currentCursorMenu.cursorIndex].Name = upgrade.GetUpgradeString(); // TODO
            }
            else
            {
                UnityEngine.Object.FindObjectOfType<Terminal>().PlayTerminalAudioServerRpc(1);
            }
        }

        public int GetMolarPrice(int price)
        {
            return (int)(price / configLGUMilkMolarContributeAmount.Value);
        }
    }
}
