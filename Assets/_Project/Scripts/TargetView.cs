using DG.Tweening;
using UnityEngine;

namespace PlayableAdsShort
{
    public sealed class TargetView : SelectableWorldView
    {
        public Transform visualRoot;
        public Transform attackAnchor;
        public Transform hintAnchor;
        public Collider hitCollider;
        public Animator animator;
        public SpriteSequenceAnimator sequenceAnimator;
        public SpriteRenderer availabilityGlow;
        public WorldStrengthBadge strengthBadge;
        public string targetId;
        public string displayName;
        public TargetKind kind = TargetKind.GroundEnemy;
        public int strength = 1;
        public int reward = 1;
        public bool endsGame;
        public int hintOrder;
        public Vector3 hintScale = Vector3.one;
        public float slashRotation;
        public Color availableGlowColor = Color.white;
        public Color unavailableGlowColor = Color.white;
        public Color invalidColor = Color.red;
        public float defeatedFadeDuration = 0.3f;
        public float defeatedHoldDuration = 0.55f;
        public float defeatedScale = 0.72f;
        public float invalidShakeStrength = 0.16f;
        public int invalidShakeVibrato = 18;
        public float invalidPunchScale = 0.12f;
        public float targetPunchScale = 0.2f;

        private bool _defeated;
        private Vector3 _visualBaseScale;
        private Color _glowBaseColor;

        public override string Id => string.IsNullOrWhiteSpace(targetId) ? gameObject.name : targetId;
        public string DisplayName => string.IsNullOrWhiteSpace(displayName) ? gameObject.name : displayName;
        public TargetKind Kind => kind;
        public int Strength => strength;
        public int Reward => reward;
        public bool EndsGame => endsGame;
        public int HintOrder => hintOrder;
        public Vector3 Position => transform.position;
        public Vector3 HintPosition => hintAnchor != null ? hintAnchor.position : transform.position;
        public Vector3 HintScale => hintScale;
        public Vector3 AttackSpot => attackAnchor != null ? attackAnchor.position : transform.position;
        public float SlashRotation => slashRotation;

        private void Awake()
        {
            _visualBaseScale = visualRoot.localScale;
            if (availabilityGlow != null)
            {
                _glowBaseColor = availabilityGlow.color;
            }
        }

        public void Bind()
        {
            _defeated = false;
            hitCollider.enabled = true;
            strengthBadge.SetValue(strength);
            strengthBadge.gameObject.SetActive(true);
            visualRoot.localScale = _visualBaseScale;
            visualRoot.gameObject.SetActive(true);
            if (availabilityGlow != null)
            {
                availabilityGlow.gameObject.SetActive(true);
                availabilityGlow.color = _glowBaseColor;
            }
            if (sequenceAnimator != null)
            {
                sequenceAnimator.PlayLoop();
            }
        }

        public void SetAvailable(bool available)
        {
            hitCollider.enabled = !_defeated;
            if (availabilityGlow != null)
            {
                availabilityGlow.color = available ? availableGlowColor : unavailableGlowColor;
            }
        }

        public void SetDefeated()
        {
            _defeated = true;
            hitCollider.enabled = false;
            SetTrigger(PlayableConstants.Animation.DeathTriggerHash);
            if (sequenceAnimator != null)
            {
                sequenceAnimator.Stop();
            }

            Sequence sequence = DOTween.Sequence().SetLink(gameObject);
            sequence.AppendInterval(defeatedHoldDuration);
            sequence.Append(visualRoot.DOScale(_visualBaseScale * defeatedScale, defeatedFadeDuration).SetEase(Ease.InBack));
            sequence.Join(strengthBadge.FadeOut(defeatedFadeDuration));
            if (availabilityGlow != null)
            {
                sequence.Join(availabilityGlow.DOFade(0f, defeatedFadeDuration));
            }
            sequence.OnComplete(() =>
            {
                visualRoot.gameObject.SetActive(false);
                strengthBadge.gameObject.SetActive(false);
                if (availabilityGlow != null)
                {
                    availabilityGlow.gameObject.SetActive(false);
                }
            });
        }

        public Tween PulseInvalid(float duration)
        {
            SetTrigger(PlayableConstants.Animation.InvalidTriggerHash);
            if (availabilityGlow != null)
            {
                availabilityGlow.color = invalidColor;
            }
            visualRoot.DOKill();
            Sequence sequence = DOTween.Sequence().SetLink(gameObject);
            sequence.Append(visualRoot.DOShakePosition(duration, invalidShakeStrength, invalidShakeVibrato, 90f, false, true));
            sequence.Join(visualRoot.DOPunchScale(Vector3.one * invalidPunchScale, duration, 5, 0.7f));
            sequence.Join(strengthBadge.FlashInvalid(duration));
            return sequence;
        }

        public Tween HitImpact()
        {
            SetTrigger(PlayableConstants.Animation.HitTriggerHash);
            Sequence sequence = DOTween.Sequence().SetLink(gameObject);
            sequence.Append(visualRoot.DOPunchScale(Vector3.one * targetPunchScale, 0.18f, 7, 0.8f));
            sequence.Join(strengthBadge.Pulse(0.22f));
            return sequence;
        }

        private void SetTrigger(int triggerHash)
        {
            if (animator != null)
            {
                animator.SetTrigger(triggerHash);
            }
        }
    }
}
