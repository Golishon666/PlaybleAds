using UnityEngine;

namespace PlayableAdsShort
{
    public sealed class SpriteSequenceAnimator : MonoBehaviour
    {
        public SpriteRenderer spriteRenderer;
        public Sprite[] frames;
        [Min(1f)] public float framesPerSecond = 24f;
        public bool playOnEnable = true;
        public bool loop = true;
        public bool randomStartFrame;

        private int _frameIndex;
        private float _elapsed;
        private bool _playing;
        private bool _loopCurrent;

        public float Duration => frames == null || frames.Length == 0 ? 0f : frames.Length / framesPerSecond;

        private void OnEnable()
        {
            if (playOnEnable)
            {
                if (loop)
                {
                    PlayLoop();
                }
                else
                {
                    PlayOnce();
                }
            }
        }

        private void Update()
        {
            if (!_playing || frames == null || frames.Length == 0)
            {
                return;
            }

            _elapsed += Time.deltaTime;
            int nextFrame = Mathf.FloorToInt(_elapsed * framesPerSecond);
            if (_loopCurrent)
            {
                nextFrame %= frames.Length;
            }
            else if (nextFrame >= frames.Length)
            {
                nextFrame = frames.Length - 1;
                _playing = false;
            }

            if (nextFrame != _frameIndex)
            {
                _frameIndex = nextFrame;
                spriteRenderer.sprite = frames[_frameIndex];
            }
        }

        public void PlayLoop()
        {
            Play(true);
        }

        public void PlayOnce()
        {
            Play(false);
        }

        public void Stop()
        {
            _playing = false;
        }

        private void Play(bool shouldLoop)
        {
            if (frames == null || frames.Length == 0 || spriteRenderer == null)
            {
                return;
            }

            _loopCurrent = shouldLoop;
            _frameIndex = randomStartFrame ? Random.Range(0, frames.Length) : 0;
            _elapsed = _frameIndex / framesPerSecond;
            spriteRenderer.sprite = frames[_frameIndex];
            _playing = true;
        }
    }
}
