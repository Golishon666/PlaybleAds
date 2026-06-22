using DG.Tweening;
using UnityEngine;

namespace PlayableAdsShort
{
    public sealed class VfxBurstView : MonoBehaviour
    {
        public Transform visualRoot;
        public SpriteRenderer spriteRenderer;
        public SpriteSequenceAnimator sequenceAnimator;
        public float scalePeak = 1.18f;
        public float scaleInPart = 0.45f;
        public float holdPart = 0.35f;
        public Ease scaleInEase = Ease.OutBack;
        public Ease scaleOutEase = Ease.OutSine;
        public Ease fadeEase = Ease.InQuad;

        public void Show(Vector3 position)
        {
            transform.position = position;
            visualRoot.localScale = Vector3.zero;
        }

        public Tween Play(float duration)
        {
            Sequence sequence = DOTween.Sequence().SetLink(gameObject);
            if (sequenceAnimator != null)
            {
                sequenceAnimator.PlayOnce();
                duration = Mathf.Max(duration, sequenceAnimator.Duration);
            }
            sequence.Append(visualRoot.DOScale(1f, duration * scaleInPart).SetEase(scaleInEase));
            sequence.Append(visualRoot.DOScale(scalePeak, duration * holdPart).SetEase(scaleOutEase));
            if (spriteRenderer != null)
            {
                sequence.Join(spriteRenderer.DOFade(0f, duration * holdPart).SetEase(fadeEase));
            }
            sequence.OnComplete(() => Destroy(gameObject));
            return sequence;
        }
    }
}
