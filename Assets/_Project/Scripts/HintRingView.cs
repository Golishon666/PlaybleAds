using DG.Tweening;
using UnityEngine;

namespace PlayableAdsShort
{
    public sealed class HintRingView : MonoBehaviour
    {
        public Transform visualRoot;
        public SpriteRenderer spriteRenderer;
        public float pulseScale = 1.12f;
        public float pulseMinAlpha = 0.68f;
        public Ease pulseEase = Ease.InOutSine;

        public void Show(Vector3 position, Vector3 scale)
        {
            transform.position = position;
            visualRoot.localScale = scale;
        }

        public Tween Pulse(float duration)
        {
            Sequence sequence = DOTween.Sequence().SetLoops(-1).SetLink(gameObject);
            Vector3 baseScale = visualRoot.localScale;
            sequence.Append(visualRoot.DOScale(baseScale * pulseScale, duration).SetEase(pulseEase));
            sequence.Join(spriteRenderer.DOFade(pulseMinAlpha, duration).SetEase(pulseEase));
            sequence.Append(visualRoot.DOScale(baseScale, duration).SetEase(pulseEase));
            sequence.Join(spriteRenderer.DOFade(1f, duration).SetEase(pulseEase));
            return sequence;
        }
    }
}
