using UnityEngine;

namespace MilkMolars
{
    internal class NotificationOverlay : MonoBehaviour
    {
        public Animator animator = null!;
        public AudioSource audioSource = null!;

        public static void ShowNotification(bool mega, bool showVisual, bool playSFX, float volume)
        {
            if (!showVisual && !playSFX) { return; }
            var obj = GameObject.Instantiate(MilkMolarsContentHandler.Instance.MilkMolarsAssets!.NotificationOverlayPrefab);
            if (obj == null) { return; }
            NotificationOverlay? overlay = obj.GetComponent<NotificationOverlay>();
            if (overlay == null) { return; }

            if (showVisual)
                overlay.animator.SetTrigger(mega ? "showmega" : "show");

            if (playSFX)
            {
                overlay.audioSource.volume = volume;
                overlay.audioSource.Play();
            }

            GameObject.Destroy(obj, 6f);
        }
    }
}
