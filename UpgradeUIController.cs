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
        public ListView lvUpgradeList;

        public Button btnYou;
        public Button btnGroup;

        public bool LeftTabSelected = true;

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

            veMain = root.Q<VisualElement>("veMain");
            veMain.style.display = DisplayStyle.None;
            if (veMain == null) { logger.LogError("veMain not found."); return; }

            lvUpgradeList = root.Q<ListView>("lvUpgradeList");
            if (lvUpgradeList == null) { logger.LogError("lvUpgradeList not found."); return; }

            // Find elements

            btnYou = root.Q<Button>("btnYou");
            if (btnYou == null) { logger.LogError("btnYou not found."); return; }

            btnGroup = root.Q<Button>("btnGroup");
            if (btnGroup == null) { logger.LogError("btnGroup not found."); return; }

            logger.LogDebug("Got Controls for UI");

            // Add event handlers
            btnYou.clickable.clicked += () => ButtonYouClicked();
            btnGroup.clickable.clicked += () => ButtonGroupClicked();

            logger.LogDebug("UIControllerScript: Start() complete");
        }

        private void Update()
        {
            if (veMain.style.display == DisplayStyle.Flex && (Keyboard.current.escapeKey.wasPressedThisFrame || Keyboard.current.tabKey.wasPressedThisFrame)) { HideUI(); }

            if (MilkMolarInputs.Instance.OpenUIKey.WasPressedThisFrame() && localPlayer.CheckConditionsForEmote())
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

            SelectTab(left: true);
            

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

        public void SelectTab(bool left)
        {
            if (left)
            {
                btnGroup.style.borderBottomWidth = 0;
                btnGroup.style.borderTopWidth = 0;
                btnGroup.style.borderRightWidth = 0;
                btnGroup.style.borderLeftWidth = 0;

                btnYou.style.borderBottomWidth = 2;
                btnYou.style.borderTopWidth = 2;
                btnYou.style.borderRightWidth = 2;
                btnYou.style.borderLeftWidth = 2;
                LeftTabSelected = true;


            }
            else
            {
                btnYou.style.borderBottomWidth = 0;
                btnYou.style.borderTopWidth = 0;
                btnYou.style.borderRightWidth = 0;
                btnYou.style.borderLeftWidth = 0;

                btnGroup.style.borderBottomWidth = 2;
                btnGroup.style.borderTopWidth = 2;
                btnGroup.style.borderRightWidth = 2;
                btnGroup.style.borderLeftWidth = 2;
                LeftTabSelected = false;


            }
        }

        private void ButtonYouClicked()
        {
            logger.LogDebug("Button Head clicked");
            if (!LeftTabSelected)
            {
                SelectTab(left: true);
            }
        }

        private void ButtonGroupClicked()
        {
            logger.LogDebug("Button Chest clicked");

            if (LeftTabSelected)
            {
                SelectTab(left: false);
            }
        }
    }
}
