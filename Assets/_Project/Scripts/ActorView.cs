using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;

namespace PlayableAdsShort
{
    public sealed class ActorView : MonoBehaviour
    {
        public Transform motionRoot;
        public Transform visualRoot;
        public Animator animator;
        public Transform weaponRoot;
        public bool startWithoutWeapon = true;
        public SpriteRenderer groundMarker;
        public WorldStrengthBadge strengthBadge;
        public Color defaultMarkerColor = Color.white;
        public Color poweredMarkerColor = new Color(0.35f, 0.85f, 1f, 1f);
        public Vector3 facingRightEuler = new Vector3(0f, 155f, 0f);
        public Vector3 facingLeftEuler = new Vector3(0f, 205f, 0f);
        public Ease moveEase = Ease.InOutSine;
        public Ease punchEase = Ease.OutQuad;
        public float punchScale = 0.14f;
        public float punchDuration = 0.22f;
        public int punchVibrato = 5;
        public float punchElasticity = 0.65f;

        private Vector3 _visualBaseScale;
        private Vector3 _weaponBaseScale;
        private bool _weaponEquipped;

        public Vector3 Position => motionRoot.position;

        private void Awake()
        {
            _visualBaseScale = visualRoot.localScale;
            if (weaponRoot != null)
            {
                _weaponBaseScale = weaponRoot.localScale;
            }
        }

        public void Bind(int strength)
        {
            visualRoot.localScale = _visualBaseScale;
            groundMarker.color = defaultMarkerColor;
            SetWeaponEquipped(!startWithoutWeapon);
            SetStrength(strength);
        }

        private void LateUpdate()
        {
            if (!_weaponEquipped && weaponRoot != null)
            {
                weaponRoot.localScale = Vector3.zero;
            }
        }

        public void SetStrength(int strength)
        {
            strengthBadge.SetValue(strength);
        }

        public Tween MoveTo(Vector3 position, float duration)
        {
            Face(position);
            SetMoving(true);
            return motionRoot
                .DOMove(position, duration)
                .SetEase(moveEase)
                .OnComplete(() => SetMoving(false))
                .SetLink(gameObject);
        }

        public Tween MoveAlong(IReadOnlyList<Vector3> route, float speed)
        {
            Sequence sequence = DOTween.Sequence().SetLink(gameObject);
            Vector3 cursor = motionRoot.position;
            SetMoving(true);

            for (int i = 0; i < route.Count; i++)
            {
                Vector3 point = route[i];
                float distance = Vector3.Distance(cursor, point);
                if (distance <= 0.01f)
                {
                    continue;
                }

                Vector3 destination = point;
                sequence.AppendCallback(() => Face(destination));
                sequence.Append(motionRoot.DOMove(destination, distance / Mathf.Max(0.01f, speed)).SetEase(moveEase));
                cursor = point;
            }

            sequence.OnComplete(() => SetMoving(false));
            return sequence;
        }

        public Tween Punch()
        {
            SetTrigger(PlayableConstants.Animation.PowerTriggerHash);
            return visualRoot
                .DOPunchScale(Vector3.one * punchScale, punchDuration, punchVibrato, punchElasticity)
                .SetEase(punchEase)
                .SetLink(gameObject);
        }

        public void PlayAttack()
        {
            SetTrigger(PlayableConstants.Animation.AttackTriggerHash);
        }

        public void EquipWeapon()
        {
            SetWeaponEquipped(true);
        }

        public void SetPoweredVisual(bool powered)
        {
            groundMarker.color = powered ? poweredMarkerColor : defaultMarkerColor;
        }

        public void Face(Vector3 target)
        {
            visualRoot.localEulerAngles = target.x >= motionRoot.position.x ? facingRightEuler : facingLeftEuler;
        }

        private void SetMoving(bool moving)
        {
            if (animator != null)
            {
                animator.SetBool(PlayableConstants.Animation.MovingBoolHash, moving);
            }
        }

        private void SetWeaponEquipped(bool equipped)
        {
            _weaponEquipped = equipped;
            if (weaponRoot != null)
            {
                weaponRoot.localScale = equipped ? _weaponBaseScale : Vector3.zero;
            }
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
