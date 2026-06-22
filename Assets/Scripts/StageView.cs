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

        private AudioSource _footstepSource;

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

        public void PlayFootsteps(AudioClip clip, float volume)
        {
            if (clip == null)
            {
                return;
            }

            AudioSource source = GetFootstepSource();
            if (source == null)
            {
                return;
            }

            if (source.clip == clip && source.isPlaying)
            {
                source.volume = volume;
                return;
            }

            source.clip = clip;
            source.volume = volume;
            source.loop = true;
            source.Play();
        }

        public void StopFootsteps()
        {
            if (_footstepSource != null)
            {
                _footstepSource.Stop();
            }
        }

        private AudioSource GetFootstepSource()
        {
            if (_footstepSource != null)
            {
                return _footstepSource;
            }

            if (audioSource == null)
            {
                return null;
            }

            _footstepSource = gameObject.AddComponent<AudioSource>();
            _footstepSource.playOnAwake = false;
            _footstepSource.outputAudioMixerGroup = audioSource.outputAudioMixerGroup;
            _footstepSource.spatialBlend = audioSource.spatialBlend;
            _footstepSource.priority = audioSource.priority;
            return _footstepSource;
        }
    }
}
