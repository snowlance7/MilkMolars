using InteractiveTerminalAPI.UI;
using InteractiveTerminalAPI.UI.Application;
using InteractiveTerminalAPI.UI.Cursor;
using InteractiveTerminalAPI.UI.Page;
using InteractiveTerminalAPI.UI.Screen;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Text.RegularExpressions;
using UnityEngine;
using UnityEngine.Bindings;
using UnityEngine.InputSystem;
using static UnityEngine.InputSystem.InputAction;

namespace MilkMolars.UI
{
    internal class UpgradeTerminalUI : PageApplication
    {
        const int UPGRADES_PER_PAGE = 12;
        private const string tooth = "🦷";
        protected override int GetEntriesPerPage<T>(T[] entries)
        {
            return UPGRADES_PER_PAGE;
        }

        public override void Initialization()
        {
            //CustomTerminalNode[] filteredNodes = UpgradeBus.Instance.terminalNodes.Where(x => x.Visible && (x.UnlockPrice > 0 || x.Prices.Length > 0)).ToArray();
            MilkMolarUpgrade[] milkMolarUpgrades = MilkMolarController.AllMilkMolarUpgrades.Where(x => x.Visible && (x.UnlockCost > 0 || x.CostsPerTier.Length > 0)).ToArray();


            if (milkMolarUpgrades.Length == 0)
            {
                CursorElement[] elements =
                [
                    CursorElement.Create(name: "Leave", action: () => UnityEngine.Object.Destroy(InteractiveTerminalManager.Instance)),
                ];
                CursorMenu cursorMenu = CursorMenu.Create(startingCursorIndex: 0, elements: elements);
                IScreen screen = new BoxedScreen()
                {
                    Title = "Milk Molar Upgrades",
                    elements =
                    [
                        new TextElement()
                        {
                            Text = "There are no upgrades to show",
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
            PageCursorElement sharedPage = GetFilteredUpgradeNodes(ref milkMolarUpgrades, ref cursorElements, (x) => x.Shared, "Milk Molar Upgrades", "Mega Milk Molar Upgrades", true);
            PageCursorElement individualPage = GetFilteredUpgradeNodes(ref milkMolarUpgrades, ref cursorElements, (x) => !x.Shared, "Milk Molar Upgrades", "Milk Molar Upgrades", false);

            if (cursorElements.Count > 1)
            {
                CursorElement[] upgradeElements = [.. cursorElements];
                CursorMenu upgradeCursorMenu = CursorMenu.Create(startingCursorIndex: 0, elements: upgradeElements);
                IScreen upgradeScreen = new BoxedScreen()
                {
                    Title = "Milk Molar Upgrades",
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

        PageCursorElement GetFilteredUpgradeNodes(ref MilkMolarUpgrade[] nodes, ref List<CursorElement> list, Func<MilkMolarUpgrade, bool> predicate, string pageTitle, string cursorName, bool shared)
        {
            PageCursorElement page = null;
            MilkMolarUpgrade[] filteredNodes = nodes.Where(predicate).ToArray();
            if (filteredNodes.Length > 0)
            {
                page = BuildUpgradePage(filteredNodes, pageTitle);
                list.Add(CursorElement.Create(name: cursorName, action: () => SwitchToUpgradeScreen(page, previous: true, shared)));
            }
            return page;
        }

        void SwitchToUpgradeScreen(PageCursorElement page, bool previous, bool shared)
        {
            MilkMolarController.InUpgradeUI = !shared;
            MilkMolarController.InMegaUpgradeUI = shared;

            InteractiveTerminalAPI.Compat.InputUtils_Compat.CursorExitKey.performed -= OnScreenExit;
            InteractiveTerminalAPI.Compat.InputUtils_Compat.CursorExitKey.performed += OnUpgradeStoreExit;
            SwitchScreen(page, previous);
        }

        void OnUpgradeStoreExit(CallbackContext context)
        {
            ResetScreen();
            InteractiveTerminalAPI.Compat.InputUtils_Compat.CursorExitKey.performed += OnScreenExit;
            InteractiveTerminalAPI.Compat.InputUtils_Compat.CursorExitKey.performed -= OnUpgradeStoreExit;
            MilkMolarController.InMegaUpgradeUI = false;
            MilkMolarController.InUpgradeUI = false;
        }

        PageCursorElement BuildUpgradePage(MilkMolarUpgrade[] nodes, string title)
        {
            (MilkMolarUpgrade[][], CursorMenu[], IScreen[]) entries = GetPageEntries(nodes);

            MilkMolarUpgrade[][] pagesUpgrades = entries.Item1;
            CursorMenu[] cursorMenus = entries.Item2;
            IScreen[] screens = entries.Item3;
            PageCursorElement page = null;
            for (int i = 0; i < pagesUpgrades.Length; i++)
            {
                MilkMolarUpgrade[] upgrades = pagesUpgrades[i];
                CursorElement[] elements = new CursorElement[upgrades.Length];
                cursorMenus[i] = CursorMenu.Create(startingCursorIndex: 0, elements: elements,
                    sorting: [
                        CompareName,
                        CompareCurrentPrice,
                        CompareCurrentPriceReversed
                        ]
                );
                CursorMenu cursorMenu = cursorMenus[i];
                screens[i] = new BoxedOutputScreen<string, string>()
                {
                    Title = title,
                    elements =
                    [
                        new TextElement()
                        {
                            Text = "Upgrades: ",
                        },
                        new TextElement()
                        {
                            Text = " "
                        },
                        cursorMenu,
                    ],
                    Output = (x) => x,
                    Input = GetCurrentSort,
                };
                for (int j = 0; j < upgrades.Length; j++)
                {
                    MilkMolarUpgrade upgrade = upgrades[j];
                    if (upgrade == null) continue;
                    elements[j] = new MMUUpgradeCursorElement()
                    {
                        Upgrade = upgrade,
                        Action = () => BuyUpgrade(upgrade, () => SwitchScreen(page, previous: true)),
                        Active = (x) => ((MMUUpgradeCursorElement)x).Upgrade.CanBuyUpgrade()
                    };
                }
            }
            page = PageCursorElement.Create(0, screens, cursorMenus);
            return page;
        }

        string GetCurrentSort()
        {
            int currentSort = currentCursorMenu.sortingIndex;
            return currentSort switch
            {
                0 => $"Sorted by: Alphabetical [{InteractiveTerminalAPI.Compat.InputUtils_Compat.ChangeApplicationSortingKey.GetBindingDisplayString()}]",
                1 => $"Sorted by: Price (Ascending) [{InteractiveTerminalAPI.Compat.InputUtils_Compat.ChangeApplicationSortingKey.GetBindingDisplayString()}]",
                2 => $"Sorted by: Price (Descending) [{InteractiveTerminalAPI.Compat.InputUtils_Compat.ChangeApplicationSortingKey.GetBindingDisplayString()}]",
                _ => "",
            };
        }

        int CompareName(CursorElement cursor1, CursorElement cursor2)
        {
            if (cursor1 == null) return 1;
            if (cursor2 == null) return -1;
            MMUUpgradeCursorElement element = cursor1 as MMUUpgradeCursorElement;
            MMUUpgradeCursorElement element2 = cursor2 as MMUUpgradeCursorElement;

            string name1 = (element.Upgrade).Name;
            string name2 = (element2.Upgrade).Name;
            return name1.CompareTo(name2);
        }

        int CompareCurrentPriceReversed(CursorElement cursor1, CursorElement cursor2)
        {
            if (cursor1 == null) return 1;
            if (cursor2 == null) return -1;
            MMUUpgradeCursorElement element = cursor1 as MMUUpgradeCursorElement;
            MMUUpgradeCursorElement element2 = cursor2 as MMUUpgradeCursorElement;

            int currentPrice1 = (element.Upgrade).Unlocked && (element.Upgrade).CurrentTier >= (element.Upgrade).MaxTier ? int.MinValue : (element.Upgrade).GetCurrentPrice();
            int currentPrice2 = (element2.Upgrade).Unlocked && (element2.Upgrade).CurrentTier >= (element2.Upgrade).MaxTier ? int.MinValue : (element2.Upgrade).GetCurrentPrice();
            return currentPrice2.CompareTo(currentPrice1);
        }

        int CompareCurrentPrice(CursorElement cursor1, CursorElement cursor2)
        {
            if (cursor1 == null) return 1;
            if (cursor2 == null) return -1;
            MMUUpgradeCursorElement element = cursor1 as MMUUpgradeCursorElement;
            MMUUpgradeCursorElement element2 = cursor2 as MMUUpgradeCursorElement;

            int currentPrice1 = (element.Upgrade).Unlocked && (element.Upgrade).CurrentTier >= (element.Upgrade).MaxTier ? int.MaxValue : (element.Upgrade).GetCurrentPrice();
            int currentPrice2 = (element2.Upgrade).Unlocked && (element2.Upgrade).CurrentTier >= (element2.Upgrade).MaxTier ? int.MaxValue : (element2.Upgrade).GetCurrentPrice();
            return currentPrice2.CompareTo(currentPrice1);
        }

        public void BuyUpgrade(MilkMolarUpgrade node, Action backAction)
        {
            bool maxLevel = node.CurrentTier >= node.MaxTier;
            if (maxLevel && node.Unlocked)
            {
                ErrorMessage(node.Name, node.Description, backAction, "You have reached the maximum level for this upgrade.");
                return;
            }
            int price = node.GetCurrentPrice();

            if (node.Shared)
            {
                int megaMolars = NetworkHandler.MegaMilkMolars.Value;
                if (megaMolars < price)
                {
                    ErrorMessage(node.Name, node.Description, backAction, "You do not have enough mega milk molars to purchase this upgrade.");
                    return;
                }
            }
            else
            {
                if (MilkMolarController.MilkMolars < price)
                {
                    ErrorMessage(node.Name, node.Description, backAction, "You do not have enough milk molars to purchase this upgrade.");
                    return;
                }
            }

            Confirm(node.Name, node.Description, () => PurchaseUpgrade(node, price, backAction), backAction, $"Do you wish to purchase this upgrade for {price} molars?");
        }

        void PurchaseUpgrade(MilkMolarUpgrade node, int molarPrice, Action backAction)
        {
            if (node.Shared)
            {
                NetworkHandler.Instance.BuyMegaMilkMolarUpgradeServerRpc(node.Name, molarPrice);
            }
            else
            {
                MilkMolarController.BuyMilkMolarUpgrade(node);
            }
            backAction();
        }
    }

    internal class MMUUpgradeCursorElement : CursorElement
    {
        private const string tooth = "🦷";

        public MilkMolarUpgrade Upgrade { get; set; }

        public override string GetText(int availableLength)
        {
            return Upgrade.GetUpgradeString();
        }
    }
}