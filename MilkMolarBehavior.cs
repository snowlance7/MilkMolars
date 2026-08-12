using GameNetcodeStuff;
using SnowyLib;
using System.Collections;
using Unity.Netcode;
using UnityEngine;
using static MilkMolars.Plugin;

namespace MilkMolars
{
    internal class MilkMolarBehavior : PhysicsProp
    {
        [HideInInspector] public PlayerControllerB? lastPlayerHeldBy;
        [HideInInspector] public PlayerControllerB? playerFoundBy;


        Vector3 restingRotation = Vector3.zero;


        [HideInInspector] public NetworkVariable<int> ContributeAmount = new NetworkVariable<int>(1, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);
        [HideInInspector] public NetworkVariable<int> SharedContributeAmount = new NetworkVariable<int>(1, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);
        [HideInInspector] public NetworkVariable<ActivateMethod> ActivateMethod = new NetworkVariable<ActivateMethod>(Plugin.ActivateMethod.Use, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);
        [HideInInspector] public NetworkVariable<bool> UpgradePointsToFinder = new NetworkVariable<bool>(false, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);
        [HideInInspector] public NetworkVariable<bool> NonPlayersCanActivate = new NetworkVariable<bool>(false, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);

        public static bool PlaySound = true;
        public static float SoundVolume = 1f;
        public static bool ShowNotification = true;
        public static int cfgContributeAmount = 1;
        public static int cfgSharedContributeAmount = 1;
        public static ActivateMethod cfgActivateMethod = Plugin.ActivateMethod.Use;
        public static bool cfgUpgradePointsToFinder = false;
        public static bool cfgNonPlayersCanActivate = false;
        //public static string SpawnAmount = "";

        [InitConfig]
        public static void Init()
        {   
            PlaySound = PluginInstance.Config.Bind("Milk Molar Options", "Milk Molar | Play Sound", true, "Play sound when milk molar is activated").Value;
            SoundVolume = PluginInstance.Config.Bind("Milk Molar Options", "Milk Molar | Sound Volume", 1f, "Sound volume when milk molar is activated").Value;
            ShowNotification = PluginInstance.Config.Bind("Milk Molar Options", "Milk Molar | Show Notification", true, "Show a notification on the HUD when a Milk Molar is activated").Value;
            cfgContributeAmount = PluginInstance.Config.Bind("Milk Molar Options", "Milk Molar | Contribute Amount", 1, "How many points to give to the player who activates it").Value;
            cfgSharedContributeAmount = PluginInstance.Config.Bind("Milk Molar Options", "Milk Molar | Shared Contribute Amount", 1, "How many points to give to give other players (Only works if Shared is set to true)").Value;
            cfgActivateMethod = PluginInstance.Config.Bind("Milk Molar Options", "Milk Molar | Activate Method", Plugin.ActivateMethod.Grab, "Activation method for Milk Molars.\nGrab: Grabbing the Milk Molar will activate it.\nUse: Using the Milk Molar while its in your inventory will activate it.\nShip: Milk Molar will activate when brought inside the ship.\nSell: Milk Molar will activate when sold to the company.").Value;
            cfgUpgradePointsToFinder = PluginInstance.Config.Bind("Milk Molar Options", "Milk Molar | Upgrade Points to Finder", false, "This only works when `ActivateMethod` is `SHIP` or `SELL`. Setting this to true will only give points(ContributeAmount) to the first person who held the Milk Molar when activating it. SharedContributeAmount will be given to other players.").Value;
            cfgNonPlayersCanActivate = PluginInstance.Config.Bind("Milk Molar Options", "Milk Molar | NonPlayersCanActivate", false, "This only works if `ActivateMethod` is `ReturnToShip`. If true, if anything that isnt a player drops the milk molar in the ship, it will still activate and all players will get `SharedContributeAmount`.").Value;
        }

        public override void Start()
        {
            if (IsServer)
            {
                ContributeAmount.Value = cfgContributeAmount;
                SharedContributeAmount.Value = cfgSharedContributeAmount;
                ActivateMethod.Value = cfgActivateMethod;
                UpgradePointsToFinder.Value = cfgUpgradePointsToFinder;
                NonPlayersCanActivate.Value = cfgNonPlayersCanActivate;

                if (UnityEngine.Random.Range(0, 2) == 0)
                {
                    SetRestingRotationRpc(new Vector3(90, 0, 0));
                }
            }

            base.Start();

            if (ActivateMethod.Value == Plugin.ActivateMethod.Use)
            {
                itemProperties.toolTips = ["Activate [LMB]"];
            }
            if (ActivateMethod.Value == Plugin.ActivateMethod.Grab)
            {
                grabbable = false;
                customGrabTooltip = "Activate [E]";
            }
        }

        public override void Update()
        {
            GameUpdate();

            if (playerHeldBy != null)
            {
                lastPlayerHeldBy = playerHeldBy;
                playerFoundBy ??= playerHeldBy;
            }
        }

        void GameUpdate()
        {
            if (currentUseCooldown >= 0f)
            {
                currentUseCooldown -= Time.deltaTime;
            }
            if (base.IsOwner)
            {
                if (isBeingUsed && itemProperties.requiresBattery)
                {
                    if (insertedBattery.charge > 0f)
                    {
                        if (!itemProperties.itemIsTrigger)
                        {
                            insertedBattery.charge -= Time.deltaTime / itemProperties.batteryUsage;
                        }
                    }
                    else if (!insertedBattery.empty)
                    {
                        insertedBattery.empty = true;
                        if (isBeingUsed)
                        {
                            Debug.Log("Use up batteries local");
                            isBeingUsed = false;
                            UseUpBatteries();
                            UseUpItemBatteriesServerRpc();
                        }
                    }
                }
                if (!wasOwnerLastFrame)
                {
                    wasOwnerLastFrame = true;
                }
            }
            else if (wasOwnerLastFrame)
            {
                wasOwnerLastFrame = false;
            }
            if (!isHeld && parentObject == null)
            {
                if (fallTime < 1f)
                {
                    reachedFloorTarget = false;
                    FallWithCurve();
                    if (base.transform.localPosition.y - targetFloorPosition.y < 0.05f && !hasHitGround)
                    {
                        PlayDropSFX();
                        OnHitGround();
                    }
                    return;
                }
                if (!reachedFloorTarget)
                {
                    if (!hasHitGround)
                    {
                        PlayDropSFX();
                        OnHitGround();
                    }
                    reachedFloorTarget = true;
                    if (floorYRot == -1)
                    {
                        base.transform.rotation = Quaternion.Euler(restingRotation.x, base.transform.eulerAngles.y, restingRotation.z);
                    }
                    else
                    {
                        base.transform.rotation = Quaternion.Euler(restingRotation.x, (float)(floorYRot + itemProperties.floorYOffset) + 90f, restingRotation.z);
                    }
                }
                base.transform.localPosition = targetFloorPosition;
            }
            else
            {
                if (!isHeld && !isHeldByEnemy)
                {
                    return;
                }
                reachedFloorTarget = false;
                if (playerHeldBy != null && !isPocketed && playerHeldBy.scrapJiggleAudioDelay > -4f)
                {
                    if (playerHeldBy.scrapJiggleAudioDelay <= 0f)
                    {
                        playerHeldBy.scrapJiggleAudioDelay = -5f;
                        JiggleItemEffect(Random.Range(0.18f, 0.36f));
                    }
                    else
                    {
                        playerHeldBy.scrapJiggleAudioDelay -= Time.deltaTime;
                    }
                }
            }
        }

        public override void DiscardItem()
        {
            if (ActivateMethod.Value == Plugin.ActivateMethod.ReturnToShip)
            {
                if (base.IsOwner)
                {
                    HUDManager.Instance.ClearControlTips();
                    if (playerHeldBy != null)
                    {
                        playerHeldBy.IsInspectingItem = false;
                        playerHeldBy.activatingItem = false;
                    }
                }
                if (playerHeldBy != null)
                {
                    if (localPlayer == playerHeldBy && playerHeldBy.isInHangarShipRoom)
                    {
                        ActivateMolarWithDelay((int)localPlayer.actualClientId);
                    }
                }
                else if (NonPlayersCanActivate.Value && base.transform.position.y > -100f && StartOfRound.Instance.shipInnerRoomBounds.bounds.ClosestPoint(base.transform.position) == base.transform.position)
                {
                    ActivateMolarWithDelay();
                }
                playerHeldBy = null;
            }
            else
            {
                base.DiscardItem();
            }
        }

        public void ActivateMolarWithDelay(int clientId = -1)
        {
            IEnumerator activateMolarWithDelay(int clientId)
            {
                yield return null;
                //yield return new WaitForSeconds(0.5f);
                ActivateMolarRpc(clientId);
            }

            StartCoroutine(activateMolarWithDelay(clientId));
        }

        public override void InteractItem()
        {
            if (ActivateMethod.Value == Plugin.ActivateMethod.Grab)
            {
                ActivateMolarRpc((int)localPlayer.actualClientId);
            }
            else
            {
                base.InteractItem();
            }
        }

        public override void ItemActivate(bool used, bool buttonDown = true)
        {
            base.ItemActivate(used, buttonDown);
            if (!buttonDown) { return; }

            if (IsOwner && ActivateMethod.Value == Plugin.ActivateMethod.Use)
                ActivateMolarRpc((int)localPlayer.actualClientId);
        }

        [Rpc(SendTo.Everyone)]
        public void ActivateMolarRpc(int playerActivatingId = -1)
        {
            logger.LogDebug("Activating milk molar");
            PlayerControllerB? playerActivating = playerActivatingId == -1 ? null : PlayerFromId((ulong)playerActivatingId);
            playerActivating = UpgradePointsToFinder.Value && (ActivateMethod.Value == Plugin.ActivateMethod.ReturnToShip || ActivateMethod.Value == Plugin.ActivateMethod.SellToCompany) ? playerFoundBy : playerActivating;

            if (localPlayer == playerActivating && ContributeAmount.Value > 0)
            {
                NotificationOverlay.ShowNotification(false, ShowNotification, PlaySound, SoundVolume);
                AddCurrency(ContributeAmount.Value);
            }
            else if (localPlayer != playerActivating && SharedContributeAmount.Value > 0)
            {
                NotificationOverlay.ShowNotification(false, ShowNotification, PlaySound, SoundVolume);
                AddCurrency(SharedContributeAmount.Value);
            }

            switch (ActivateMethod.Value)
            {
                case Plugin.ActivateMethod.Grab:
                    if (IsServer) NetworkObject.Despawn(destroy: true);
                    break;
                case Plugin.ActivateMethod.Use:
                    if (IsOwner) localPlayer.DespawnHeldObject();
                    break;
                case Plugin.ActivateMethod.ReturnToShip:
                    if (IsServer) NetworkObject.Despawn(destroy: true);
                    break;
                case Plugin.ActivateMethod.SellToCompany:
                    break;
                default:
                    break;
            }
        }

        [Rpc(SendTo.Everyone)]
        public void SetRestingRotationRpc(Vector3 rotation)
        {
            restingRotation = rotation;
        }
    }
}