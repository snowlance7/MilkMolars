using GameNetcodeStuff;
using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using static MilkMolars.Plugin;

namespace MilkMolars.Upgrades
{
    internal class RevivePlayerUpgrade : MilkMolarUpgrade
    {
        public RevivePlayerUpgrade()
        {
            name = "revivePlayer";
            title = "Revive Player";
            description = "Revive player on monitor";
            type = UpgradeType.Repeatable;
            cost = configRevivePlayerUpgrade.Value;
        }

        public override void ActivateRepeatableUpgrade()
        {
            base.ActivateRepeatableUpgrade();
            PlayerControllerB targetedPlayer = StartOfRound.Instance.mapScreen.targetedPlayer;
            int health = 100;
            targetedPlayer.ResetPlayerBloodObjects(targetedPlayer.isPlayerDead);
            if (targetedPlayer.isPlayerDead || targetedPlayer.isPlayerControlled)
            {
                targetedPlayer.isClimbingLadder = false;
                targetedPlayer.ResetZAndXRotation();
                ((Collider)targetedPlayer.thisController).enabled = true;
                targetedPlayer.health = health;
                targetedPlayer.disableLookInput = false;
                if (targetedPlayer.isPlayerDead)
                {
                    targetedPlayer.isPlayerDead = false;
                    targetedPlayer.isPlayerControlled = true;
                    targetedPlayer.isInElevator = false;
                    targetedPlayer.isInHangarShipRoom = false;
                    targetedPlayer.isInsideFactory = true;
                    StartOfRound.Instance.SetPlayerObjectExtrapolate(enable: false);
                    targetedPlayer.setPositionOfDeadPlayer = false;
                    targetedPlayer.helmetLight.enabled = false;
                    targetedPlayer.Crouch(crouch: false);
                    targetedPlayer.criticallyInjured = false;
                    if ((UnityEngine.Object)(object)targetedPlayer.playerBodyAnimator != null)
                    {
                        targetedPlayer.playerBodyAnimator.SetBool("Limp", false);
                    }
                    targetedPlayer.bleedingHeavily = false;
                    targetedPlayer.activatingItem = false;
                    targetedPlayer.twoHanded = false;
                    targetedPlayer.inSpecialInteractAnimation = false;
                    targetedPlayer.disableSyncInAnimation = false;
                    targetedPlayer.inAnimationWithEnemy = null;
                    targetedPlayer.holdingWalkieTalkie = false;
                    targetedPlayer.speakingToWalkieTalkie = false;
                    targetedPlayer.isSinking = false;
                    targetedPlayer.isUnderwater = false;
                    targetedPlayer.sinkingValue = 0f;
                    targetedPlayer.statusEffectAudio.Stop();
                    targetedPlayer.DisableJetpackControlsLocally();
                    targetedPlayer.health = health;
                    targetedPlayer.mapRadarDotAnimator.SetBool("dead", false);
                    targetedPlayer.deadBody = null;
                    if (targetedPlayer == GameNetworkManager.Instance.localPlayerController)
                    {
                        HUDManager.Instance.gasHelmetAnimator.SetBool("gasEmitting", false);
                        targetedPlayer.hasBegunSpectating = false;
                        HUDManager.Instance.RemoveSpectateUI();
                        HUDManager.Instance.gameOverAnimator.SetTrigger("revive");
                        targetedPlayer.hinderedMultiplier = 1f;
                        targetedPlayer.isMovementHindered = 0;
                        targetedPlayer.sourcesCausingSinking = 0;
                        HUDManager.Instance.HideHUD(hide: false);
                    }
                }
                SoundManager.Instance.earsRingingTimer = 0f;
                targetedPlayer.voiceMuffledByEnemy = false;
                if (targetedPlayer.currentVoiceChatIngameSettings == null)
                {
                    StartOfRound.Instance.RefreshPlayerVoicePlaybackObjects();
                }
                if (targetedPlayer.currentVoiceChatIngameSettings != null)
                {
                    if ((UnityEngine.Object)(object)targetedPlayer.currentVoiceChatIngameSettings.voiceAudio == null)
                    {
                        targetedPlayer.currentVoiceChatIngameSettings.InitializeComponents();
                    }
                    if ((UnityEngine.Object)(object)targetedPlayer.currentVoiceChatIngameSettings.voiceAudio == null)
                    {
                        return;
                    }
                    ((Component)(object)targetedPlayer.currentVoiceChatIngameSettings.voiceAudio).GetComponent<OccludeAudio>().overridingLowPass = false;
                }
            }
            StartOfRound.Instance.livingPlayers++;
            if (GameNetworkManager.Instance.localPlayerController == targetedPlayer)
            {
                targetedPlayer.bleedingHeavily = false;
                targetedPlayer.criticallyInjured = false;
                targetedPlayer.playerBodyAnimator.SetBool("Limp", false);
                targetedPlayer.health = health;
                HUDManager.Instance.UpdateHealthUI(health, hurtPlayer: false);
                targetedPlayer.spectatedPlayerScript = null;
                ((Behaviour)(object)HUDManager.Instance.audioListenerLowPass).enabled = false;
                StartOfRound.Instance.SetSpectateCameraToGameOverMode(enableGameOver: false, targetedPlayer);
                TimeOfDay.Instance.DisableAllWeather();
                StartOfRound.Instance.UpdatePlayerVoiceEffects();
                targetedPlayer.thisPlayerModel.enabled = true;
            }
            else
            {
                targetedPlayer.thisPlayerModel.enabled = true;
                targetedPlayer.thisPlayerModelLOD1.enabled = true;
                targetedPlayer.thisPlayerModelLOD2.enabled = true;
            }
        }
    }
}
