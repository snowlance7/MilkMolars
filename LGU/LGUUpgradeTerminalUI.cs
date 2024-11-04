using InteractiveTerminalAPI.UI;
using InteractiveTerminalAPI.UI.Application;
using InteractiveTerminalAPI.UI.Cursor;
using InteractiveTerminalAPI.UI.Page;
using InteractiveTerminalAPI.UI.Screen;
using MilkMolars;
using MilkMolars.LGU;
using MoreShipUpgrades.Input;
using MoreShipUpgrades.Managers;
using MoreShipUpgrades.Misc.Util;
using MoreShipUpgrades.UI.Cursor;
using MoreShipUpgrades.UI.TerminalNodes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using UnityEngine;
using UnityEngine.InputSystem;
using static UnityEngine.InputSystem.InputAction;

namespace MilkMolars.LGU
{
    internal class LGUUpgradeTerminalUI : PageApplication
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

            List<CursorElement> cursorElements = [];
            PageCursorElement sharedPage = GetFilteredUpgradeNodes(ref filteredNodes, ref cursorElements, (x) => x.SharedUpgrade, LguConstants.MAIN_SCREEN_TITLE, LguConstants.MAIN_SCREEN_SHARED_UPGRADES_TEXT, true);
            PageCursorElement individualPage = GetFilteredUpgradeNodes(ref filteredNodes, ref cursorElements, (x) => !x.SharedUpgrade, LguConstants.MAIN_SCREEN_TITLE, LguConstants.MAIN_SCREEN_INDIVIDUAL_UPGRADES_TEXT, false);

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
        PageCursorElement GetFilteredUpgradeNodes(ref CustomTerminalNode[] nodes, ref List<CursorElement> list, Func<CustomTerminalNode, bool> predicate, string pageTitle, string cursorName, bool shared)
        {
            PageCursorElement page = null;
            CustomTerminalNode[] filteredNodes = nodes.Where(predicate).ToArray();
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
            MilkMolarController.InUpgradeUI = false;
            MilkMolarController.InMegaUpgradeUI = false;
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
                        Active = (x) => CanBuyUpgrade(((MMUUpgradeCursorElement)x).GetNode())
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
            string name1 = element.GetNode().Name;
            string name2 = element2.GetNode().Name;
            return name1.CompareTo(name2);
        }
        int CompareCurrentPriceReversed(CursorElement cursor1, CursorElement cursor2)
        {
            if (cursor1 == null) return 1;
            if (cursor2 == null) return -1;
            MMUUpgradeCursorElement element = cursor1 as MMUUpgradeCursorElement;
            MMUUpgradeCursorElement element2 = cursor2 as MMUUpgradeCursorElement;
            int currentPrice1 = element.GetNode().Unlocked && element.GetNode().CurrentUpgrade >= element.GetNode().MaxUpgrade ? int.MinValue : MilkMolarController.GetMolarPrice(element.GetNode().GetCurrentPrice(), element.GetNode().SharedUpgrade);
            int currentPrice2 = element2.GetNode().Unlocked && element2.GetNode().CurrentUpgrade >= element2.GetNode().MaxUpgrade ? int.MinValue : MilkMolarController.GetMolarPrice(element2.GetNode().GetCurrentPrice(), element2.GetNode().SharedUpgrade);
            return currentPrice2.CompareTo(currentPrice1);
        }
        int CompareCurrentPrice(CursorElement cursor1, CursorElement cursor2)
        {
            if (cursor1 == null) return 1;
            if (cursor2 == null) return -1;
            MMUUpgradeCursorElement element = cursor1 as MMUUpgradeCursorElement;
            MMUUpgradeCursorElement element2 = cursor2 as MMUUpgradeCursorElement;
            int currentPrice1 = element.GetNode().Unlocked && element.GetNode().CurrentUpgrade >= element.GetNode().MaxUpgrade ? int.MaxValue : MilkMolarController.GetMolarPrice(element.GetNode().GetCurrentPrice(), element.GetNode().SharedUpgrade);
            int currentPrice2 = element2.GetNode().Unlocked && element2.GetNode().CurrentUpgrade >= element2.GetNode().MaxUpgrade ? int.MaxValue : MilkMolarController.GetMolarPrice(element2.GetNode().GetCurrentPrice(), element2.GetNode().SharedUpgrade);
            return currentPrice1.CompareTo(currentPrice2);
        }
        static bool CanBuyUpgrade(CustomTerminalNode node)
        {
            bool maxLevel = node.CurrentUpgrade >= node.MaxUpgrade;
            if (maxLevel && node.Unlocked)
                return false;

            /*int groupCredits = UpgradeBus.Instance.GetTerminal().groupCredits;
            int price = node.GetCurrentPrice();
            return groupCredits >= price;*/
            int molars = MilkMolarController.GetCurrentMolarCount(node.SharedUpgrade);
            int molarPrice = MilkMolarController.GetMolarPrice(node.GetCurrentPrice(), node.SharedUpgrade);
            return molarPrice <= molars;

        }
        public void BuyUpgrade(CustomTerminalNode node, Action backAction)
        {
            bool maxLevel = node.CurrentUpgrade >= node.MaxUpgrade;
            if (maxLevel && node.Unlocked)
            {
                ErrorMessage(node.Name, LGUCompatibility.GetDescription(node.Description, node.SharedUpgrade), backAction, LguConstants.REACHED_MAX_LEVEL);
                return;
            }
            int currentMolars = MilkMolarController.GetCurrentMolarCount(node.SharedUpgrade);
            int molarPrice = MilkMolarController.GetMolarPrice(node.GetCurrentPrice(), node.SharedUpgrade);
            if (molarPrice > currentMolars)
            {
                ErrorMessage(node.Name, LGUCompatibility.GetDescription(node.Description, node.SharedUpgrade), backAction, "You do not have enough milk molars to purchase this upgrade.");
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
            Confirm(node.Name, LGUCompatibility.GetDescription(node.Description, node.SharedUpgrade), () => PurchaseUpgrade(node, molarPrice, backAction), backAction, $"Do you wish to purchase this upgrade for {molarPrice} milk molars?");
        }
        void PurchaseUpgrade(CustomTerminalNode node, int price, Action backAction)
        {
            //terminal.BuyItemsServerRpc([], terminal.groupCredits - price, terminal.numberOfItemsInDropship); // The only vanilla rpc that syncs credits without ownership check
            //LguStore.Instance.AddUpgradeSpentCreditsServerRpc(price);
            if (node.SharedUpgrade)
            {
                NetworkHandler.Instance.BuyMegaMilkMolarUpgradeServerRpc(price);
            }
            else
            {
                MilkMolarController.MilkMolars -= price;
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

        [MethodImpl(MethodImplOptions.NoInlining | MethodImplOptions.NoOptimization)]
        public CustomTerminalNode GetNode() { return (CustomTerminalNode)this.Node; }

        [MethodImpl(MethodImplOptions.NoInlining | MethodImplOptions.NoOptimization)]
        public override string GetText(int availableLength)
        {
            CustomTerminalNode Node = GetNode(); ;

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

        [MethodImpl(MethodImplOptions.NoInlining | MethodImplOptions.NoOptimization)]
        void AppendPriceText(ref StringBuilder sb)
        {
            CustomTerminalNode Node = GetNode();

            int price = MilkMolarController.GetMolarPrice(Node.GetCurrentPrice(), Node.SharedUpgrade);
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
}