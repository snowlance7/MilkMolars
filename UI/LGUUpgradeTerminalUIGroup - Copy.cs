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
using InteractiveTerminalAPI.UI.Page;
using MoreShipUpgrades.Misc.Util;
using MoreShipUpgrades.UI.Cursor;
using static UnityEngine.InputSystem.InputAction;
using UnityEngine.InputSystem;

// https://github.com/WhiteSpike/InteractiveTerminalAPI/wiki/Examples#simple-example-with-code-snippets

namespace MilkMolars.UI
{
    internal class LGUUpgradeTerminalUIGroup : PageApplication
    {
        const int UPGRADES_PER_PAGE = 12;
        protected override int GetEntriesPerPage<T>(T[] entries)
        {
            return UPGRADES_PER_PAGE;
        }
        public override void Initialization()
        {
            CustomTerminalNode[] filteredNodes = UpgradeBus.Instance.terminalNodes.Where(x => x.Visible && (x.UnlockPrice > 0 || x.Prices.Length > 0)).ToArray();

            if (filteredNodes.Length == 0)
            {
                CursorElement[] elements =
                [
                    CursorElement.Create(name: "Leave", action: () => UnityEngine.Object.Destroy(InteractiveTerminalManager.Instance)),
                ];
                CursorMenu cursorMenu = CursorMenu.Create(startingCursorIndex: 0, elements: elements);
                IScreen screen = new BoxedScreen()
                {
                    Title = "LGU Milk Molar Upgrades",
                    elements =
                    [
                        new TextElement()
                        {
                            Text = LguConstants.MAIN_SCREEN_TOP_TEXT_NO_ENTRIES,
                        },
                        new TextElement()
                        {
                            Text = " "
                        },
                        cursorMenu
                    ]
                };
                currentPage = PageCursorElement.Create(startingPageIndex: 0, elements: [screen], cursorMenus: [cursorMenu]);
                currentCursorMenu = cursorMenu;
                currentScreen = screen;
                return;
            }

            List<CursorElement> cursorElements = [];
            PageCursorElement sharedPage = GetFilteredUpgradeNodes(ref filteredNodes, ref cursorElements, (x) => x.SharedUpgrade, LguConstants.MAIN_SCREEN_TITLE, LguConstants.MAIN_SCREEN_SHARED_UPGRADES_TEXT);
            PageCursorElement individualPage = GetFilteredUpgradeNodes(ref filteredNodes, ref cursorElements, (x) => !x.SharedUpgrade, LguConstants.MAIN_SCREEN_TITLE, LguConstants.MAIN_SCREEN_INDIVIDUAL_UPGRADES_TEXT);

            if (cursorElements.Count > 1)
            {
                CursorElement[] upgradeElements = [.. cursorElements];
                CursorMenu upgradeCursorMenu = CursorMenu.Create(startingCursorIndex: 0, elements: upgradeElements);
                IScreen upgradeScreen = new BoxedScreen()
                {
                    Title = "LGU Milk Molar Upgrades",
                    elements =
                    [
                        upgradeCursorMenu
                    ]
                };
                initialPage = PageCursorElement.Create(startingPageIndex: 0, elements: [upgradeScreen], cursorMenus: [upgradeCursorMenu]);
            }
            else
            {
                if (sharedPage == null)
                {
                    initialPage = individualPage;
                }
                else
                {
                    initialPage = sharedPage;
                }
            }
            currentPage = initialPage;
            currentCursorMenu = currentPage.GetCurrentCursorMenu();
            currentScreen = currentPage.GetCurrentScreen();
        }
        PageCursorElement GetFilteredUpgradeNodes(ref CustomTerminalNode[] nodes, ref List<CursorElement> list, Func<CustomTerminalNode, bool> predicate, string pageTitle, string cursorName)
        {
            PageCursorElement page = null;
            CustomTerminalNode[] filteredNodes = nodes.Where(predicate).ToArray();
            if (filteredNodes.Length > 0)
            {
                page = BuildUpgradePage(filteredNodes, pageTitle);
                list.Add(CursorElement.Create(name: cursorName, action: () => SwitchToUpgradeScreen(page, previous: true)));
            }
            return page;
        }
        void SwitchToUpgradeScreen(PageCursorElement page, bool previous)
        {
            InteractiveTerminalAPI.Compat.InputUtils_Compat.CursorExitKey.performed -= OnScreenExit;
            InteractiveTerminalAPI.Compat.InputUtils_Compat.CursorExitKey.performed += OnUpgradeStoreExit;
            SwitchScreen(page, previous);
        }

        void OnUpgradeStoreExit(CallbackContext context)
        {
            ResetScreen();
            InteractiveTerminalAPI.Compat.InputUtils_Compat.CursorExitKey.performed += OnScreenExit;
            InteractiveTerminalAPI.Compat.InputUtils_Compat.CursorExitKey.performed -= OnUpgradeStoreExit;
        }
        PageCursorElement BuildUpgradePage(CustomTerminalNode[] nodes, string title)
        {
            (CustomTerminalNode[][], CursorMenu[], IScreen[]) entries = GetPageEntries(nodes);

            CustomTerminalNode[][] pagesUpgrades = entries.Item1;
            CursorMenu[] cursorMenus = entries.Item2;
            IScreen[] screens = entries.Item3;
            PageCursorElement page = null;
            for (int i = 0; i < pagesUpgrades.Length; i++)
            {
                CustomTerminalNode[] upgrades = pagesUpgrades[i];
                CursorElement[] elements = new CursorElement[upgrades.Length];
                cursorMenus[i] = CursorMenu.Create(startingCursorIndex: 0, elements: elements);
                CursorMenu cursorMenu = cursorMenus[i];
                screens[i] = new BoxedOutputScreen<string, string>()
                {
                    Title = title,
                    elements =
                    [
                        new TextElement()
                        {
                            Text = LguConstants.MAIN_SCREEN_TOP_TEXT,
                        },
                        new TextElement()
                        {
                            Text = " "
                        },
                        cursorMenu,
                    ]
                };
                for (int j = 0; j < upgrades.Length; j++)
                {
                    CustomTerminalNode upgrade = upgrades[j];
                    if (upgrade == null) continue;
                    elements[j] = new UpgradeCursorElement()
                    {
                        Node = upgrade,
                        Action = () => BuyUpgrade(upgrade, () => SwitchScreen(page, previous: true)),
                        Active = (x) => CanBuyUpgrade(((UpgradeCursorElement)x).Node)
                    };
                }
            }
            page = PageCursorElement.Create(0, screens, cursorMenus);
            return page;
        }
        static bool CanBuyUpgrade(CustomTerminalNode node)
        {
            //if (UpgradeBus.Instance.PluginConfiguration.ALTERNATIVE_ITEM_PROGRESSION && UpgradeBus.Instance.PluginConfiguration.ITEM_PROGRESSION_NO_PURCHASE_UPGRADES) return true;
            bool maxLevel = node.CurrentUpgrade >= node.MaxUpgrade;
            if (maxLevel && node.Unlocked)
                return false;

            int molarPrice = GetMolarPrice(node);

            if (node.SharedUpgrade)
            {
                return molarPrice <= NetworkHandler.MegaMilkMolars.Value;
            }
            else
            {
                return molarPrice <= MilkMolarController.MilkMolars;
            }
        }
        public void BuyUpgrade(CustomTerminalNode node, Action backAction)
        {
            int molars;
            if (node.SharedUpgrade)
            {
                molars = NetworkHandler.MegaMilkMolars.Value;
            }
            else
            {
                molars = MilkMolarController.MilkMolars;
            }

            bool maxLevel = node.CurrentUpgrade >= node.MaxUpgrade;
            if (maxLevel && node.Unlocked)
            {
                ErrorMessage(node.Name, node.Description, backAction, LguConstants.REACHED_MAX_LEVEL);
                return;
            }
            int price = GetMolarPrice(node);
            if (molars < price)
            {
                ErrorMessage(node.Name, node.Description, backAction, LguConstants.NOT_ENOUGH_CREDITS);
                return;
            }
            /*StringBuilder discoveredItems = new();
            List<string> items = ItemProgressionManager.GetDiscoveredItems(node);
            if (items.Count > 0)
            {
                discoveredItems.Append("\n\nDiscovered items: ");
                for (int i = 0; i < items.Count; i++)
                {
                    string item = items[i];
                    discoveredItems.Append(item);
                    if (i < items.Count - 1) discoveredItems.Append(", ");
                }
            }
            if (UpgradeBus.Instance.PluginConfiguration.ALTERNATIVE_ITEM_PROGRESSION && UpgradeBus.Instance.PluginConfiguration.ITEM_PROGRESSION_NO_PURCHASE_UPGRADES)
            {
                ErrorMessage(node.Name, node.Description, backAction, " ");
                return;
            }*/
            Confirm(node.Name, node.Description, () => PurchaseUpgrade(node, price, backAction), backAction, $"Do you wish to purchase this upgrade for {price} molars?");
        }
        void PurchaseUpgrade(CustomTerminalNode node, int price, Action backAction)
        {
            terminal.BuyItemsServerRpc([], terminal.groupCredits - price, terminal.numberOfItemsInDropship); // The only vanilla rpc that syncs credits without ownership check
            LguStore.Instance.AddUpgradeSpentCreditsServerRpc(price);
            if (!node.Unlocked)
            {
                LguStore.Instance.HandleUpgrade(node);
            }
            else if (node.Unlocked && node.MaxUpgrade > node.CurrentUpgrade)
            {
                LguStore.Instance.HandleUpgrade(node, true);
            }
            backAction();
        }

        public static int GetMolarPrice(CustomTerminalNode node)
        {
            if (node.SharedUpgrade)
            {
                return (int)(node.GetCurrentPrice() / configLGUMegaMilkMolarContributeAmount.Value);
            }
            else
            {
                return (int)(node.GetCurrentPrice() / configLGUMilkMolarContributeAmount.Value);
            }
        }
    }
}
