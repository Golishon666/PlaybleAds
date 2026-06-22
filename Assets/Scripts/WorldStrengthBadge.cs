using DG.Tweening;
using UnityEngine;

namespace PlayableAdsShort
{
    public sealed class WorldStrengthBadge : MonoBehaviour
    {
        public SpriteRenderer glowRenderer;
        public TextMesh frontText;
        public TextMesh shadowText;
        public Color textColor = Color.white;
        public Color shadowColor = new Color(0f, 0f, 0f, 0.9f);
        public Color invalidColor = new Color(1f, 0.12f, 0.05f, 1f);
        public float pulseScale = 0.16f;
        public int pulseVibrato = 5;

        private Color _glowColor;
        private Vector3 _baseScale;

        private void Awake()
        {
            _baseScale = transform.localScale;
            if (glowRenderer != null)
            {
                _glowColor = glowRenderer.color;
            }
        }

        public void SetValue(int value)
        {
            string text = value.ToString();
            frontText.text = text;
            frontText.color = textColor;
            if (shadowText != null)
            {
                shadowText.text = text;
                shadowText.color = shadowColor;
            }
        }

        public Tween Pulse(float duration)
        {
            transform.DOKill();
            transform.localScale = _baseScale;
            return transform.DOPunchScale(Vector3.one * pulseScale, duration, pulseVibrato, 0.7f).SetLink(gameObject);
        }

        public Tween FlashInvalid(float duration)
        {
            if (glowRenderer == null)
            {
                return Pulse(duration);
            }

            Color original = _glowColor.a > 0f ? _glowColor : glowRenderer.color;
            Sequence sequence = DOTween.Sequence().SetLink(gameObject);
            sequence.Append(glowRenderer.DOColor(invalidColor, duration * 0.45f));
            sequence.Append(glowRenderer.DOColor(original, duration * 0.55f));
            return sequence;
        }

        public Tween FadeOut(float duration)
        {
            Sequence sequence = DOTween.Sequence().SetLink(gameObject);
            if (glowRenderer != null)
            {
                sequence.Join(glowRenderer.DOFade(0f, duration));
            }
            if (frontText != null)
            {
                Color frontColor = frontText.color;
                sequence.Join(DOTween.To(
                    () => frontText.color.a,
                    alpha => frontText.color = new Color(frontColor.r, frontColor.g, frontColor.b, alpha),
                    0f,
                    duration));
            }
            if (shadowText != null)
            {
                Color shadow = shadowText.color;
                sequence.Join(DOTween.To(
                    () => shadowText.color.a,
                    alpha => shadowText.color = new Color(shadow.r, shadow.g, shadow.b, alpha),
                    0f,
                    duration));
            }
            return sequence;
        }
    }
}
