using UnityEngine;

namespace PlayableAdsShort
{
    public sealed class StageView : MonoBehaviour
    {
        public Camera worldCamera;
        public WorldGameInput worldInput;
        public WorldNavigation navigation;
        public Transform actorLayer;
        public Transform targetLayer;
        public Transform hintLayer;
        public Transform effectsLayer;
        public Transform overlayLayer;
        public AudioSource audioSource;
        public ActorView heroView;
        public ChestView chestView;
        public TargetView[] targetViews;

        public void Apply(GameConfig config)
        {
            worldInput.SetCamera(worldCamera);
        }

        public void Play(AudioClip clip, float volume = 1f)
        {
            if (clip != null && audioSource != null)
            {
                audioSource.PlayOneShot(clip, volume);
            }
        }
    }
}
