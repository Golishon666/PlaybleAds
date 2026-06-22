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
        public float initialVisibleScale = 0.72f;
        public Vector3 positionOffset = new Vector3(0f, 0f, -0.05f);
        public Ease popEase = Ease.OutBack;
        public Ease fadeEase = Ease.InQuad;

        private Tween _popTween;

        public void Show(Vector3 position)
        {
            transform.DOKill();
            _popTween?.Kill();
            if (visualRoot != null)
            {
                visualRoot.DOKill();
                visualRoot.localScale = Vector3.one * initialVisibleScale;
            }

            transform.position = position + positionOffset;
            SetAlpha(1f);
        }

        public Tween Pop(float delay, bool keepVisible = false)
        {
            Sequence sequence = DOTween.Sequence().SetDelay(delay).SetLink(gameObject);
            _popTween = sequence;
            if (visualRoot != null)
            {
                sequence.Append(visualRoot.DOScale(1f, popDuration).SetEase(popEase));
            }

            if (keepVisible)
            {
                return sequence;
            }

            sequence.AppendInterval(visibleDuration);
            if (spriteRenderer != null)
            {
                sequence.Append(spriteRenderer.DOFade(0f, fadeDuration).SetEase(fadeEase));
            }

            sequence.OnComplete(() => Destroy(gameObject));
            return sequence;
        }

        public Tween Hide()
        {
            transform.DOKill();
            _popTween?.Kill();
            if (visualRoot != null)
            {
                visualRoot.DOKill();
            }

            if (spriteRenderer == null)
            {
                Destroy(gameObject);
                return null;
            }

            return spriteRenderer
                .DOFade(0f, fadeDuration)
                .SetEase(fadeEase)
                .SetLink(gameObject)
                .OnComplete(() => Destroy(gameObject));
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
