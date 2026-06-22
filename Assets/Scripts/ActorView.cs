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
        public float strengthIncreasePulseDuration = 0.28f;
        public Vector3 rewardCatchOffset = new Vector3(0f, 0.85f, -0.2f);
        public float weaponThrowOutDuration = 0.24f;
        public float weaponThrowReturnDuration = 0.28f;
        public float weaponThrowHoldDuration = 0.06f;
        public float weaponThrowSpinDegrees = 540f;
        public Ease weaponThrowOutEase = Ease.OutQuad;
        public Ease weaponThrowReturnEase = Ease.InOutSine;

        private Vector3 _visualBaseScale;
        private Vector3 _weaponBaseScale;
        private bool _weaponEquipped;
        private int _strength;
        private bool _strengthInitialized;

        public Vector3 Position => motionRoot.position;
        public Vector3 RewardCatchPosition => weaponRoot != null ? weaponRoot.position : motionRoot.position + rewardCatchOffset;

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
            bool increased = _strengthInitialized && strength > _strength;
            _strength = strength;
            _strengthInitialized = true;

            strengthBadge.SetValue(strength);
            if (increased)
            {
                strengthBadge.Pulse(strengthIncreasePulseDuration);
            }
        }

        public Tween MoveAlong(IReadOnlyList<Vector3> route, float speed)
        {
            var points = new List<Vector3>();
            Vector3 cursor = motionRoot.position;
            float totalDistance = 0f;
            SetMoving(true);

            for (int i = 0; i < route.Count; i++)
            {
                Vector3 point = route[i];
                float distance = Vector3.Distance(cursor, point);
                if (distance <= 0.01f)
                {
                    continue;
                }

                points.Add(point);
                totalDistance += distance;
                cursor = point;
            }

            if (points.Count == 0)
            {
                SetMoving(false);
                return DOTween.Sequence().SetLink(gameObject);
            }

            Face(points[0]);
            return motionRoot
                .DOPath(points.ToArray(), totalDistance / Mathf.Max(0.01f, speed), PathType.Linear)
                .SetEase(moveEase)
                .OnWaypointChange(index =>
                {
                    if (index >= 0 && index < points.Count)
                    {
                        Face(points[index]);
                    }
                })
                .OnComplete(() => SetMoving(false))
                .SetLink(gameObject);
        }

        public Tween Punch()
        {
            SetTrigger(PlayableConstants.Animation.PowerTriggerHash);
            return null;
        }

        public void PlayAttack()
        {
            SetTrigger(PlayableConstants.Animation.AttackTriggerHash);
        }

        public void PlaySecondAttack()
        {
            SetTrigger(PlayableConstants.Animation.SecondAttackTriggerHash);
        }

        public void PlayShopUpdate()
        {
            SetTrigger(PlayableConstants.Animation.ShopUpdateTriggerHash);
        }

        public Tween ThrowWeaponAt(Vector3 targetPosition, GameObject projectilePrefab, Transform projectileParent)
        {
            if (weaponRoot == null || !_weaponEquipped)
            {
                return null;
            }

            GameObject projectile = projectilePrefab != null
                ? Instantiate(projectilePrefab, weaponRoot.position, weaponRoot.rotation, projectileParent)
                : Instantiate(weaponRoot.gameObject, weaponRoot.position, weaponRoot.rotation);
            Transform projectileRoot = projectile.transform;
            Vector3 originalLocalScale = weaponRoot.localScale;
            bool usesPrefabProjectile = projectilePrefab != null;
            Vector3 projectileScale = usesPrefabProjectile ? projectileRoot.localScale : weaponRoot.lossyScale;

            Sequence sequence = DOTween.Sequence().SetLink(gameObject);
            projectileRoot.SetParent(projectileParent, true);
            projectileRoot.localScale = projectileScale;
            projectile.SetActive(true);
            SetRenderersEnabled(projectileRoot, true);
            weaponRoot.localScale = Vector3.zero;

            sequence.Append(projectileRoot.DOMove(targetPosition, weaponThrowOutDuration).SetEase(weaponThrowOutEase));
            sequence.Join(projectileRoot.DORotate(Vector3.forward * weaponThrowSpinDegrees, weaponThrowOutDuration, RotateMode.WorldAxisAdd));
            sequence.AppendInterval(weaponThrowHoldDuration);
            sequence.Append(projectileRoot.DOMove(weaponRoot.position, weaponThrowReturnDuration).SetEase(weaponThrowReturnEase));
            sequence.Join(projectileRoot.DORotate(Vector3.forward * weaponThrowSpinDegrees, weaponThrowReturnDuration, RotateMode.WorldAxisAdd));
            sequence.OnComplete(() =>
            {
                weaponRoot.localScale = originalLocalScale;
                Destroy(projectile);
            });
            sequence.OnKill(() =>
            {
                if (weaponRoot != null)
                {
                    weaponRoot.localScale = originalLocalScale;
                }

                if (projectile != null)
                {
                    Destroy(projectile);
                }
            });
            return sequence;
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

        private static void SetRenderersEnabled(Transform root, bool enabled)
        {
            Renderer[] renderers = root.GetComponentsInChildren<Renderer>(includeInactive: true);
            for (int i = 0; i < renderers.Length; i++)
            {
                renderers[i].enabled = enabled;
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
