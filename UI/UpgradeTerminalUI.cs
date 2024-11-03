/*using InteractiveTerminalAPI.UI;
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
                    Title = LguConstants.MAIN_SCREEN_TITLE,
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

            foreach (CustomTerminalNode node in filteredNodes)
            {
                if (node.UnlockPrice > 0)
                {
                    node.UnlockPrice = MilkMolarController.GetMolarPrice(node.UnlockPrice, node.SharedUpgrade);
                }
                if (node.Prices.Length > 0)
                {
                    for (int i = 0; i < node.Prices.Length; i++)
                    {
                        node.Prices[i] = MilkMolarController.GetMolarPrice(node.Prices[i], node.SharedUpgrade);
                    }
                }
                if (node.Description != null)
                {
                    node.Description = Regex.Replace(node.Description, @"\$(\d+)", match =>
                    {
                        int price = int.Parse(match.Groups[1].Value);
                        int molarPrice = MilkMolarController.GetMolarPrice(price, node.SharedUpgrade);
                        return tooth + molarPrice.ToString();
                    });
                }
            }

            List<CursorElement> cursorElements = [];
            PageCursorElement sharedPage = GetFilteredUpgradeNodes(ref filteredNodes, ref cursorElements, (x) => x.SharedUpgrade, LguConstants.MAIN_SCREEN_TITLE, "Mega Milk Molar Upgrades");
            PageCursorElement individualPage = GetFilteredUpgradeNodes(ref filteredNodes, ref cursorElements, (x) => !x.SharedUpgrade, LguConstants.MAIN_SCREEN_TITLE, "Milk Molar Upgrades");

            if (cursorElements.Count > 1)
            {
                CursorElement[] upgradeElements = [.. cursorElements];
                CursorMenu upgradeCursorMenu = CursorMenu.Create(startingCursorIndex: 0, elements: upgradeElements);
                IScreen upgradeScreen = new BoxedScreen()
                {
                    Title = LguConstants.MAIN_SCREEN_TITLE,
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
                            Text = LguConstants.MAIN_SCREEN_TOP_TEXT,
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
                    CustomTerminalNode upgrade = upgrades[j];
                    if (upgrade == null) continue;
                    elements[j] = new MMUUpgradeCursorElement()
                    {
                        Node = upgrade,
                        Action = () => BuyUpgrade(upgrade, () => SwitchScreen(page, previous: true)),
                        Active = (x) => CanBuyUpgrade((CustomTerminalNode)((MMUUpgradeCursorElement)x).Node)
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

            string name1 = ((CustomTerminalNode)element.Node).Name;
            string name2 = ((CustomTerminalNode)element2.Node).Name;
            return name1.CompareTo(name2);
        }

        int CompareCurrentPriceReversed(CursorElement cursor1, CursorElement cursor2)
        {
            if (cursor1 == null) return 1;
            if (cursor2 == null) return -1;
            MMUUpgradeCursorElement element = cursor1 as MMUUpgradeCursorElement;
            MMUUpgradeCursorElement element2 = cursor2 as MMUUpgradeCursorElement;

            int currentPrice1 = ((CustomTerminalNode)element.Node).Unlocked && ((CustomTerminalNode)element.Node).CurrentUpgrade >= ((CustomTerminalNode)element.Node).MaxUpgrade ? int.MinValue : ((CustomTerminalNode)element.Node).GetCurrentPrice();
            int currentPrice2 = ((CustomTerminalNode)element2.Node).Unlocked && ((CustomTerminalNode)element2.Node).CurrentUpgrade >= ((CustomTerminalNode)element2.Node).MaxUpgrade ? int.MinValue : ((CustomTerminalNode)element2.Node).GetCurrentPrice();
            return currentPrice2.CompareTo(currentPrice1);
        }

        int CompareCurrentPrice(CursorElement cursor1, CursorElement cursor2)
        {
            if (cursor1 == null) return 1;
            if (cursor2 == null) return -1;
            MMUUpgradeCursorElement element = cursor1 as MMUUpgradeCursorElement;
            MMUUpgradeCursorElement element2 = cursor2 as MMUUpgradeCursorElement;

            CustomTerminalNode node1 = element.Node as CustomTerminalNode;
            CustomTerminalNode node2 = element2.Node as CustomTerminalNode;

            int currentPrice1 = node1.Unlocked && node1.CurrentUpgrade >= node1.MaxUpgrade ? int.MaxValue : node1.GetCurrentPrice();
            int currentPrice2 = node2.Unlocked && node2.CurrentUpgrade >= node2.MaxUpgrade ? int.MaxValue : node2.GetCurrentPrice();
            return currentPrice1.CompareTo(currentPrice2);
        }

        static bool CanBuyUpgrade(CustomTerminalNode node)
        {
            bool maxLevel = node.CurrentUpgrade >= node.MaxUpgrade;
            if (maxLevel && node.Unlocked)
                return false;

            int price = node.GetCurrentPrice();

            if (node.SharedUpgrade)
            {
                int megaMolars = NetworkHandler.MegaMilkMolars.Value;
                return megaMolars >= price;
            }
            else
            {
                return MilkMolarController.MilkMolars >= price;
            }
        }

        public void BuyUpgrade(CustomTerminalNode node, Action backAction)
        {
            bool maxLevel = node.CurrentUpgrade >= node.MaxUpgrade;
            if (maxLevel && node.Unlocked)
            {
                ErrorMessage(node.Name, node.Description, backAction, LguConstants.REACHED_MAX_LEVEL);
                return;
            }
            int price = node.GetCurrentPrice();

            if (node.SharedUpgrade)
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

        void PurchaseUpgrade(CustomTerminalNode node, int molarPrice, Action backAction)
        {
            if (node.SharedUpgrade)
            {
                NetworkHandler.Instance.BuyMegaMilkMolarUpgradeServerRpc(molarPrice);
            }
            else
            {
                MilkMolarController.MilkMolars -= molarPrice;
            }

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
    }

    internal class MMUUpgradeCursorElement : CursorElement
    {
        private const string tooth = "🦷";

        public object Node { get; set; }

        public override string GetText(int availableLength)
        {
            CustomTerminalNode Node = (CustomTerminalNode)this.Node;

            StringBuilder sb = new StringBuilder();
            sb.Append(new string(LguConstants.WHITE_SPACE, 2));
            string name = Node.Name.Length > LguConstants.NAME_LENGTH ? Node.Name.Substring(0, LguConstants.NAME_LENGTH) : Node.Name + new string(LguConstants.WHITE_SPACE, Mathf.Max(0, LguConstants.NAME_LENGTH - Node.Name.Length));
            if (!Active(this))
            {
                if (Node.Unlocked && Node.CurrentUpgrade >= Node.MaxUpgrade)
                {
                    sb.Append(string.Format(LguConstants.COLOR_INITIAL_FORMAT, LguConstants.HEXADECIMAL_DARK_GREEN));
                }
                else
                {
                    sb.Append(string.Format(LguConstants.COLOR_INITIAL_FORMAT, LguConstants.HEXADECIMAL_GREY));
                }
            }
            sb.Append(name);

            int currentLevel = Node.GetCurrentLevel();
            int remainingLevels = Node.GetRemainingLevels();
            string levels = new string(LguConstants.FILLED_LEVEL, currentLevel) + new string(LguConstants.EMPTY_LEVEL, remainingLevels) + new string(LguConstants.WHITE_SPACE, Mathf.Max(0, LguConstants.LEVEL_LENGTH - currentLevel - remainingLevels));
            sb.Append(LguConstants.WHITE_SPACE);
            sb.Append(levels);
            sb.Append(LguConstants.WHITE_SPACE);
            if (remainingLevels > 0)
            {
                AppendPriceText(ref sb);
                AppendSaleText(ref sb);
            }
            else
            {
                sb.Append("Maxed!");
            }
            if (!Active(this)) sb.Append(LguConstants.COLOR_FINAL_FORMAT);
            return sb.ToString();
        }

        void AppendPriceText(ref StringBuilder sb)
        {
            CustomTerminalNode Node = (CustomTerminalNode)this.Node;

            int price = Node.GetCurrentPrice();
            int currentMolars = MilkMolarController.GetCurrentMolarCount(Node.SharedUpgrade);

            if (price <= currentMolars)
            {
                sb.Append(price);
                sb.Append(tooth);
            }
            else
            {
                sb.Append(string.Format(LguConstants.COLOR_INITIAL_FORMAT, LguConstants.HEXADECIMAL_DARK_RED));
                sb.Append(price);
                sb.Append(tooth);
                sb.Append(LguConstants.COLOR_FINAL_FORMAT);
            }
        }

        [MethodImpl(MethodImplOptions.NoInlining | MethodImplOptions.NoOptimization)]
        void AppendSaleText(ref StringBuilder sb)
        {
            CustomTerminalNode Node = (CustomTerminalNode)this.Node;

            if (Node.SalePercentage < 1f)
            {
                sb.Append(LguConstants.WHITE_SPACE);
                sb.Append($"({(1 - Node.SalePercentage) * 100:F0}% OFF)");
            }
        }
    }
}*/