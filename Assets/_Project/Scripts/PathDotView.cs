using DG.Tweening;
using UnityEngine;

namespace PlayableAdsShort
{
    public sealed class PathDotView : MonoBehaviour
    {
        public Transform visualRoot;
        public SpriteRenderer spriteRenderer;
        public float popDuration = 0.18f;
        public float visibleDuration = 0.35f;
        public float fadeDuration = 0.18f;

        public void Show(Vector3 position)
        {
            transform.position = position;
            visualRoot.localScale = Vector3.zero;
        }

        public Tween Pop(float delay)
        {
            Sequence sequence = DOTween.Sequence().SetDelay(delay).SetLink(gameObject);
            sequence.Append(visualRoot.DOScale(1f, popDuration).SetEase(Ease.OutBack));
            sequence.AppendInterval(visibleDuration);
            sequence.Append(spriteRenderer.DOFade(0f, fadeDuration).SetEase(Ease.InQuad));
            sequence.OnComplete(() => Destroy(gameObject));
            return sequence;
        }
    }
}
