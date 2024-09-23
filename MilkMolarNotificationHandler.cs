using BepInEx.Logging;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using static MilkMolars.Plugin;

namespace MilkMolars
{
    internal class MilkMolarNotificationHandler : MonoBehaviour
    {
        private static MilkMolarNotificationHandler _instance = null!;
        private static ManualLogSource logger = Plugin.LoggerInstance;

        public static MilkMolarNotificationHandler Instance
        {
            get
            {
                // If the instance doesn't exist, try to find it in the scene
                if (_instance == null)
                {
                    _instance = FindObjectOfType<MilkMolarNotificationHandler>();

                    // If it's still null, create a new GameObject and add the component
                    if (_instance == null)
                    {
                        GameObject singletonObject = new GameObject("MilkMolarNotificationHandler");
                        DontDestroyOnLoad(singletonObject);
                        _instance = singletonObject.AddComponent<MilkMolarNotificationHandler>();
                    }
                }
                return _instance;
            }
        }

        private static GameObject MilkMolarNotif = null!;
        private static GameObject MegaMilkMolarNotif = null!;

        private Coroutine? notificationCoroutine = null!;

        public static void GetUIIcons()
        {
            GameObject existingUI = GameObject.Find("/Systems/UI/Canvas/IngamePlayerHUD/OpenEyes");

            if (MilkMolarNotif == null)
            {
                MilkMolarNotif = new GameObject("MilkMolarNotif");
                MilkMolarNotif.transform.SetParent(existingUI.transform, worldPositionStays: false);
                UnityEngine.UI.Image milkMolarNotifImage = MilkMolarNotif.AddComponent<UnityEngine.UI.Image>();
                milkMolarNotifImage.sprite = NetworkHandler.Instance.MilkMolarUIIcon;

                RectTransform rectTransform = MilkMolarNotif.GetComponent<RectTransform>();
                rectTransform.sizeDelta = new Vector2(configNotificationSize.Value, configNotificationSize.Value);
                rectTransform.anchoredPosition = new Vector2(configNotificationPositionX.Value, configNotificationPositionY.Value);
                MilkMolarNotif.SetActive(false);
                milkMolarNotifImage.color = new Color(1f, 1f, 1f, 0f);
            }
            if (MegaMilkMolarNotif == null)
            {
                MegaMilkMolarNotif = new GameObject("MegaMilkMolarNotif");
                MegaMilkMolarNotif.transform.SetParent(existingUI.transform, worldPositionStays: false);
                UnityEngine.UI.Image megaMilkMolarNotifImage = MegaMilkMolarNotif.AddComponent<UnityEngine.UI.Image>();
                megaMilkMolarNotifImage.sprite = NetworkHandler.Instance.MegaMilkMolarUIIcon;

                RectTransform rectTransform = MegaMilkMolarNotif.GetComponent<RectTransform>();
                rectTransform.sizeDelta = new Vector2(configNotificationSize.Value, configNotificationSize.Value); // 250 250
                rectTransform.anchoredPosition = new Vector2(configNotificationPositionX.Value, configNotificationPositionY.Value); // 0 175
                megaMilkMolarNotifImage.color = new Color(1f, 1f, 1f, 0f);
                MegaMilkMolarNotif.SetActive(false);
            }
        }

        public void ShowNotification(bool mega)
        {
            if (configPlaySound.Value) { localPlayer.statusEffectAudio.PlayOneShot(ActivateSFX, configSoundVolume.Value); }

            if (notificationCoroutine == null)
            {
                notificationCoroutine = StartCoroutine(ShowNotificationCoroutine(mega));
            }
        }

        private IEnumerator ShowNotificationCoroutine(bool mega)
        {
            logger.LogDebug("Starting notification coroutine");
            float elapsedTime = 0f;
            float duration = 1f;
            float waitTime = 3f;
            GameObject notif;
            UnityEngine.UI.Image image;

            if (mega)
            {
                notif = MegaMilkMolarNotif;
                image = MegaMilkMolarNotif.GetComponent<UnityEngine.UI.Image>();
            }
            else
            {
                notif = MilkMolarNotif;
                image = MilkMolarNotif.GetComponent<UnityEngine.UI.Image>();
            }

            notif.SetActive(configShowNotification.Value);
            Color color = image.color;

            while (elapsedTime < duration)
            {
                elapsedTime += Time.deltaTime;
                color.a = Mathf.Lerp(0f, 1f, elapsedTime / duration);
                image.color = color;
                yield return null;
            }

            yield return new WaitForSeconds(waitTime);

            elapsedTime = 0f;
            while (elapsedTime < duration)
            {
                elapsedTime += Time.deltaTime;
                color.a = Mathf.Lerp(1f, 0f, elapsedTime / duration);
                image.color = color;
                yield return null;
            }

            notif.SetActive(false);
            notificationCoroutine = null;
            logger.LogDebug("Ending notification coroutine");
        }
    }
}
