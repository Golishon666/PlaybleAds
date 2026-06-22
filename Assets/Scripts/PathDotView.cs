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
        public Ease popEase = Ease.OutBack;
        public Ease fadeEase = Ease.InQuad;

        public void Show(Vector3 position)
        {
            transform.DOKill();
            visualRoot.DOKill();
            transform.position = position;
            visualRoot.localScale = Vector3.zero;
            SetAlpha(1f);
        }

        public Tween Pop(float delay, bool keepVisible = false)
        {
            Sequence sequence = DOTween.Sequence().SetDelay(delay).SetLink(gameObject);
            sequence.Append(visualRoot.DOScale(1f, popDuration).SetEase(popEase));
            if (keepVisible)
            {
                return sequence;
            }

            sequence.AppendInterval(visibleDuration);
            sequence.Append(spriteRenderer.DOFade(0f, fadeDuration).SetEase(fadeEase));
            sequence.OnComplete(() => Destroy(gameObject));
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
