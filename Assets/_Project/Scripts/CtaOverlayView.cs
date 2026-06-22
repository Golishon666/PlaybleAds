using DG.Tweening;
using UnityEngine;

namespace PlayableAdsShort
{
    public sealed class CtaOverlayView : MonoBehaviour
    {
        public Transform visualRoot;
        public SpriteRenderer panelRenderer;
        public TextMesh titleText;
        public TextMesh subtitleText;
        public TextMesh buttonText;
        public string title = PlayableConstants.Cta.Title;
        public string subtitle = PlayableConstants.Cta.Subtitle;
        public string callToAction = PlayableConstants.Cta.Button;
        public string targetUrl = PlayableConstants.Cta.Url;
        public float hiddenScale = 0.9f;
        public float fadeInDuration = 0.32f;
        public float scaleInDuration = 0.42f;

        public void Bind()
        {
            gameObject.SetActive(false);
            titleText.text = title;
            subtitleText.text = subtitle;
            buttonText.text = callToAction;
            if (panelRenderer != null)
            {
                panelRenderer.color = new Color(panelRenderer.color.r, panelRenderer.color.g, panelRenderer.color.b, 0f);
            }
        }

        public Tween Show()
        {
            gameObject.SetActive(true);
            visualRoot.localScale = Vector3.one * hiddenScale;
            Sequence sequence = DOTween.Sequence().SetLink(gameObject);
            if (panelRenderer != null)
            {
                sequence.Append(panelRenderer.DOFade(1f, fadeInDuration).SetEase(Ease.OutQuad));
            }
            sequence.Join(visualRoot.DOScale(1f, scaleInDuration).SetEase(Ease.OutBack));
            return sequence;
        }

        private void OnMouseUpAsButton()
        {
            Application.OpenURL(targetUrl);
        }
    }
}
