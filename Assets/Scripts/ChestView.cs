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
        public Transform rewardRoot;
        public Transform rewardVisualRoot;
        public Vector3 rewardSpawnOffset = new Vector3(0f, 0.8f, -0.15f);
        public Vector3 rewardEndOffset = new Vector3(0f, 0f, -0.08f);
        public Vector3 rewardSpinEuler = new Vector3(0f, 0f, -720f);
        public float rewardFlightDuration = PlayableConstants.Motion.ChestRewardFlightDuration;
        public float rewardArcHeight = PlayableConstants.Motion.ChestRewardArcHeight;
        public float rewardStartScale = 1f;
        public float rewardEndScale = 0.75f;
        public float rewardLandPunchScale = 0.1f;
        public float rewardLandPunchDuration = 0.16f;
        public Ease rewardFlightEase = Ease.InOutSine;
        public float upgradeHideDelay = 0.05f;
        public float upgradeHideDuration = 0.28f;
        public float upgradeHideScale = 0.72f;
        public Ease upgradeHideEase = Ease.InBack;
        public float invalidShakeStrength = 0.14f;
        public int invalidShakeVibrato = 16;

        private Vector3 _visualBaseScale;
        private Vector3 _rewardRootBaseLocalPosition;
        private Vector3 _rewardVisualBaseLocalEuler;

        public override string Id => string.IsNullOrWhiteSpace(targetId) ? PlayableConstants.Ids.Chest : targetId;
        public int Strength => strength;
        public int Reward => reward;
        public Vector3 Position => transform.position;
        public Vector3 HintPosition => hintAnchor != null ? hintAnchor.position : transform.position;
        public Vector3 HintScale => hintScale;
        public Vector3 ApproachSpot => approachAnchor != null ? approachAnchor.position : transform.position;

        private void Awake()
        {
            if (visualRoot != null)
            {
                _visualBaseScale = visualRoot.localScale;
            }

            if (rewardRoot != null)
            {
                _rewardRootBaseLocalPosition = rewardRoot.localPosition;
            }

            if (rewardVisualRoot != null)
            {
                _rewardVisualBaseLocalEuler = rewardVisualRoot.localEulerAngles;
            }
        }

        public void Bind()
        {
            gameObject.SetActive(true);
            hitCollider.enabled = true;
            if (visualRoot != null)
            {
                visualRoot.gameObject.SetActive(true);
                visualRoot.localScale = _visualBaseScale;
            }

            strengthBadge.gameObject.SetActive(true);
            strengthBadge.SetValue(strength);
            ResetReward();
        }

        public void Open()
        {
            if (animator != null)
            {
                animator.SetTrigger(PlayableConstants.Animation.OpenTriggerHash);
            }

            visualRoot.DOPunchScale(Vector3.one * openPunchScale, openPunchDuration, openPunchVibrato, openPunchElasticity).SetLink(gameObject);
        }

        public Tween PlayRewardFlight(Vector3 targetPosition)
        {
            if (rewardRoot == null)
            {
                return DOTween.Sequence().SetLink(gameObject);
            }

            Transform rewardVisual = rewardVisualRoot != null ? rewardVisualRoot : rewardRoot;
            rewardRoot.DOKill();
            rewardVisual.DOKill();

            Vector3 start = transform.position + rewardSpawnOffset;
            Vector3 end = targetPosition + rewardEndOffset;
            Vector3 arc = Vector3.Lerp(start, end, 0.5f) + Vector3.up * rewardArcHeight;

            rewardRoot.gameObject.SetActive(true);
            rewardRoot.position = start;
            rewardVisual.localScale = Vector3.one * rewardStartScale;
            rewardVisual.localEulerAngles = _rewardVisualBaseLocalEuler;

            Sequence sequence = DOTween.Sequence().SetLink(gameObject);
            sequence.Append(rewardRoot.DOPath(new[] { start, arc, end }, rewardFlightDuration, PathType.CatmullRom).SetEase(rewardFlightEase));
            sequence.Join(rewardVisual.DOLocalRotate(rewardSpinEuler, rewardFlightDuration, RotateMode.FastBeyond360).SetEase(Ease.Linear));
            sequence.Join(rewardVisual.DOScale(Vector3.one * rewardEndScale, rewardFlightDuration).SetEase(rewardFlightEase));
            sequence.Append(rewardVisual.DOPunchScale(Vector3.one * rewardLandPunchScale, rewardLandPunchDuration, 5, 0.65f));
            sequence.OnComplete(ResetReward);
            return sequence;
        }

        public Tween HideAfterUpgrade()
        {
            hitCollider.enabled = false;

            Sequence sequence = DOTween.Sequence().SetLink(gameObject);
            sequence.AppendInterval(upgradeHideDelay);
            if (visualRoot != null)
            {
                sequence.Append(visualRoot.DOScale(_visualBaseScale * upgradeHideScale, upgradeHideDuration).SetEase(upgradeHideEase));
            }
            else
            {
                sequence.AppendInterval(upgradeHideDuration);
            }

            sequence.Join(strengthBadge.FadeOut(upgradeHideDuration));
            sequence.OnComplete(() =>
            {
                if (visualRoot != null)
                {
                    visualRoot.gameObject.SetActive(false);
                }

                strengthBadge.gameObject.SetActive(false);
                gameObject.SetActive(false);
            });
            return sequence;
        }

        public Tween PulseInvalid(float duration)
        {
            Sequence sequence = DOTween.Sequence().SetLink(gameObject);
            sequence.Append(visualRoot.DOShakePosition(duration, invalidShakeStrength, invalidShakeVibrato, 90f, false, true));
            sequence.Join(strengthBadge.FlashInvalid(duration));
            return sequence;
        }

        private void ResetReward()
        {
            if (rewardRoot == null)
            {
                return;
            }

            rewardRoot.DOKill();
            rewardRoot.localPosition = _rewardRootBaseLocalPosition;
            rewardRoot.gameObject.SetActive(false);

            if (rewardVisualRoot != null)
            {
                rewardVisualRoot.DOKill();
                rewardVisualRoot.localEulerAngles = _rewardVisualBaseLocalEuler;
                rewardVisualRoot.localScale = Vector3.one * rewardStartScale;
            }
        }
    }
}
