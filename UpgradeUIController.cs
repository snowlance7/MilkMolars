using BepInEx.Logging;
using GameNetcodeStuff;
using LethalLib;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UIElements;
using static MilkMolars.Plugin;
//using TMPro;

namespace MilkMolars
{
    public class UpgradeUIController : MonoBehaviour
    {
        private static ManualLogSource logger = Plugin.LoggerInstance;

        public static UpgradeUIController Instance;

        public VisualElement veMain;

        public Button btnYou;
        public Button btnGroup;

        public GrabbableObject HeadItem;
        public GrabbableObject ChestItem;
        public GrabbableObject FeetItem;

        private void Start()
        {
            logger.LogDebug("UIController: Start()");

            if (Instance == null)
            {
                Instance = this;
            }

            // Get UIDocument
            logger.LogDebug("Getting UIDocument");
            UIDocument uiDocument = GetComponent<UIDocument>();
            if (uiDocument == null) { logger.LogError("uiDocument not found."); return; }

            // Get VisualTreeAsset
            logger.LogDebug("Getting visual tree asset");
            if (uiDocument.visualTreeAsset == null) { logger.LogError("visualTreeAsset not found."); return; }

            // Instantiate root
            VisualElement root = uiDocument.visualTreeAsset.Instantiate();
            if (root == null) { logger.LogError("root is null!"); return; }
            logger.LogDebug("Adding root");
            uiDocument.rootVisualElement.Add(root);
            if (uiDocument.rootVisualElement == null) { logger.LogError("uiDocument.rootVisualElement not found."); return; }
            logger.LogDebug("Got root");
            root = uiDocument.rootVisualElement;

            veMain = uiDocument.rootVisualElement.Q<VisualElement>("veMain");
            veMain.style.display = DisplayStyle.None;
            if (veMain == null) { logger.LogError("veMain not found."); return; }

            // Find elements

            btnYou = root.Q<Button>("btnYou");
            if (btnYou == null) { logger.LogError("btnYou not found."); return; }

            btnChest = root.Q<Button>("btnChest");
            if (btnChest == null) { logger.LogError("btnChest not found."); return; }

            btnFeet = root.Q<Button>("btnFeet");
            if (btnFeet == null) { logger.LogError("btnFeet not found."); return; }

            logger.LogDebug("Got Controls for UI");

            // Add event handlers
            btnHead.clickable.clicked += () => ButtonHeadClicked();
            btnChest.clickable.clicked += () => ButtonChestClicked();
            btnFeet.clickable.clicked += () => ButtonFeetClicked();

            logger.LogDebug("UIControllerScript: Start() complete");
        }

        private void Update()
        {
            if (veMain.style.display == DisplayStyle.Flex && (Keyboard.current.escapeKey.wasPressedThisFrame || Keyboard.current.tabKey.wasPressedThisFrame)) { HideUI(); }

            if (SCPItemsInputs.Instance.OpenUIKey.WasPressedThisFrame() && localPlayer.CheckConditionsForEmote())
            {
                if (veMain.style.display == DisplayStyle.None) { ShowUI(); }
                else { HideUI(); }
            }
            /*if (showingUI)
            {
                UnityEngine.Cursor.lockState = CursorLockMode.None;
                UnityEngine.Cursor.visible = true;
                IngamePlayerSettings.Instance.playerInput.DeactivateInput();
                StartOfRound.Instance.localPlayerController.disableLookInput = true;
            }*/
        }

        public void ShowUI()
        {
            logger.LogDebug("Showing UI");
            veMain.style.display = DisplayStyle.Flex;

            // TODO: Change background to item icon and set text to item name
            if (HeadItem != null)
            {
                btnHead.text = HeadItem.itemProperties.itemName;
                btnHead.style.backgroundImage = new StyleBackground(HeadItem.itemProperties.itemIcon);
            }
            else
            {
                btnHead.text = "Head Slot";
                btnHead.style.backgroundImage = null;
            }

            if (ChestItem != null)
            {
                btnChest.text = ChestItem.itemProperties.itemName;
                btnChest.style.backgroundImage = new StyleBackground(ChestItem.itemProperties.itemIcon);
            }
            else
            {
                btnChest.text = "Chest Slot";
                btnChest.style.backgroundImage = null;
            }

            if (FeetItem != null)
            {
                btnFeet.text = FeetItem.itemProperties.itemName;
                btnFeet.style.backgroundImage = new StyleBackground(FeetItem.itemProperties.itemIcon);
            }
            else
            {
                btnFeet.text = "Feet Slot";
                btnFeet.style.backgroundImage = null;
            }

            UnityEngine.Cursor.lockState = CursorLockMode.None;
            UnityEngine.Cursor.visible = true;
            StartOfRound.Instance.localPlayerController.disableMoveInput = true;
            StartOfRound.Instance.localPlayerController.disableInteract = true;
            StartOfRound.Instance.localPlayerController.disableLookInput = true;
        }

        public void HideUI()
        {
            logger.LogDebug("Hiding UI");
            veMain.style.display = DisplayStyle.None;

            UnityEngine.Cursor.lockState = CursorLockMode.Locked;
            UnityEngine.Cursor.visible = false;
            StartOfRound.Instance.localPlayerController.disableMoveInput = false;
            StartOfRound.Instance.localPlayerController.disableInteract = false;
            StartOfRound.Instance.localPlayerController.disableLookInput = false;
        }

        private void ButtonHeadClicked()
        {
            logger.LogDebug("Button Head clicked");

            if (HeadItem == null) { return; }
            HeadItem.GetComponent<WearableItemBehavior>().UnWear();
            HideUI();
        }

        private void ButtonChestClicked()
        {
            logger.LogDebug("Button Chest clicked");

            if (HeadItem == null) { return; }
            ChestItem.GetComponent<WearableItemBehavior>().UnWear();
            HideUI();
        }

        private void ButtonFeetClicked()
        {
            logger.LogDebug("Button Feet clicked");

            if (HeadItem == null) { return; }
            FeetItem.GetComponent<WearableItemBehavior>().UnWear();
            HideUI();
        }
    }
}
