using DG.Tweening;
using UnityEngine;

namespace PlayableAdsShort
{
    public sealed class ChestView : SelectableWorldView
    {
        public Transform visualRoot;
        public Transform approachAnchor;
        public Transform hintAnchor;
        public Collider hitCollider;
        public Animator animator;
        public WorldStrengthBadge strengthBadge;
        public string targetId = PlayableConstants.Ids.Chest;
        public int strength = PlayableConstants.Gameplay.ChestStrength;
        public int reward = PlayableConstants.Gameplay.ChestReward;
        public Vector3 hintScale = Vector3.one;
        public float openPunchScale = 0.16f;
        public float openPunchDuration = 0.28f;
        public int openPunchVibrato = 6;
        public float openPunchElasticity = 0.7f;
        public float invalidShakeStrength = 0.14f;
        public int invalidShakeVibrato = 16;

        public override string Id => string.IsNullOrWhiteSpace(targetId) ? PlayableConstants.Ids.Chest : targetId;
        public int Strength => strength;
        public int Reward => reward;
        public Vector3 Position => transform.position;
        public Vector3 HintPosition => hintAnchor != null ? hintAnchor.position : transform.position;
        public Vector3 HintScale => hintScale;
        public Vector3 ApproachSpot => approachAnchor != null ? approachAnchor.position : transform.position;

        public void Bind()
        {
            hitCollider.enabled = true;
            strengthBadge.SetValue(strength);
        }

        public void Open()
        {
            if (animator != null)
            {
                animator.SetTrigger(PlayableConstants.Animation.OpenTriggerHash);
            }

            visualRoot.DOPunchScale(Vector3.one * openPunchScale, openPunchDuration, openPunchVibrato, openPunchElasticity).SetLink(gameObject);
        }

        public Tween PulseInvalid(float duration)
        {
            Sequence sequence = DOTween.Sequence().SetLink(gameObject);
            sequence.Append(visualRoot.DOShakePosition(duration, invalidShakeStrength, invalidShakeVibrato, 90f, false, true));
            sequence.Join(strengthBadge.FlashInvalid(duration));
            return sequence;
        }
    }
}
