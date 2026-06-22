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
            transform.DOKill();
            visualRoot.DOKill();
            transform.position = position;
            visualRoot.localScale = scale;
            SetAlpha(1f);
        }

        public Tween Pulse(float duration, bool destroyOnComplete = false)
        {
            Sequence sequence = DOTween.Sequence().SetLink(gameObject);
            Vector3 baseScale = visualRoot.localScale;
            sequence.Append(visualRoot.DOScale(baseScale * pulseScale, duration).SetEase(pulseEase));
            sequence.Join(spriteRenderer.DOFade(pulseMinAlpha, duration).SetEase(pulseEase));
            sequence.Append(visualRoot.DOScale(baseScale, duration).SetEase(pulseEase));
            sequence.Join(spriteRenderer.DOFade(1f, duration).SetEase(pulseEase));
            if (destroyOnComplete)
            {
                sequence.OnComplete(() => Destroy(gameObject));
            }

            return sequence;
        }

        private void SetAlpha(float alpha)
        {
            if (spriteRenderer == null)
            {
                return;
            }

            Color color = spriteRenderer.color;
            color.a = alpha;
            spriteRenderer.color = color;
        }
    }
}
