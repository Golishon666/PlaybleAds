using System;
using System.Collections.Generic;
using System.Linq;
using DG.Tweening;
using UnityEngine;

namespace PlayableAdsShort
{
    public sealed class WorldSwayView : MonoBehaviour
    {
        public string[] targetNames = { "boat", "ship" };
        public Vector2 positionAmplitude = new Vector2(PlayableConstants.Sway.PositionAmplitudeX, PlayableConstants.Sway.PositionAmplitudeY);
        public float rotationAmplitude = PlayableConstants.Sway.RotationAmplitude;
        public float cycleDuration = PlayableConstants.Sway.CycleDuration;
        public float phaseStep = 0.35f;
        public Ease swayEase = Ease.InOutSine;
        public bool includeInactive;

        private readonly List<Transform> _targets = new List<Transform>();
        private readonly Dictionary<Transform, PoseState> _baseStates = new Dictionary<Transform, PoseState>();

        private void OnEnable()
        {
            BindTargets();
            StartSway();
        }

        private void OnDisable()
        {
            for (int i = 0; i < _targets.Count; i++)
            {
                if (_targets[i] != null)
                {
                    _targets[i].DOKill();
                }
            }
        }

        private void BindTargets()
        {
            _targets.Clear();
            if (targetNames == null || targetNames.Length == 0)
            {
                return;
            }

            var names = new HashSet<string>(
                targetNames.Where(name => !string.IsNullOrWhiteSpace(name)),
                StringComparer.OrdinalIgnoreCase);

            foreach (Transform child in GetComponentsInChildren<Transform>(includeInactive))
            {
                if (child != transform && names.Contains(child.name))
                {
                    _targets.Add(child);
                }
            }
        }

        private void StartSway()
        {
            float duration = Mathf.Max(0.1f, cycleDuration);
            for (int i = 0; i < _targets.Count; i++)
            {
                Transform target = _targets[i];
                if (target == null)
                {
                    continue;
                }

                Vector3 basePosition = target.localPosition;
                Vector3 baseEuler = target.localEulerAngles;
                if (!_baseStates.TryGetValue(target, out PoseState baseState))
                {
                    baseState = new PoseState(basePosition, baseEuler);
                    _baseStates[target] = baseState;
                }

                Vector3 positivePosition = baseState.Position + new Vector3(positionAmplitude.x, positionAmplitude.y, 0f);
                Vector3 negativePosition = baseState.Position - new Vector3(positionAmplitude.x, positionAmplitude.y, 0f);
                Vector3 positiveEuler = baseState.Euler + new Vector3(0f, 0f, rotationAmplitude);
                Vector3 negativeEuler = baseState.Euler - new Vector3(0f, 0f, rotationAmplitude);

                target.DOKill();
                target.localPosition = baseState.Position;
                target.localEulerAngles = baseState.Euler;

                Sequence sequence = DOTween.Sequence().SetDelay(i * phaseStep).SetLoops(-1, LoopType.Restart).SetLink(target.gameObject);
                sequence.Append(target.DOLocalMove(positivePosition, duration).SetEase(swayEase));
                sequence.Join(target.DOLocalRotate(positiveEuler, duration).SetEase(swayEase));
                sequence.Append(target.DOLocalMove(baseState.Position, duration).SetEase(swayEase));
                sequence.Join(target.DOLocalRotate(baseState.Euler, duration).SetEase(swayEase));
                sequence.Append(target.DOLocalMove(negativePosition, duration).SetEase(swayEase));
                sequence.Join(target.DOLocalRotate(negativeEuler, duration).SetEase(swayEase));
                sequence.Append(target.DOLocalMove(baseState.Position, duration).SetEase(swayEase));
                sequence.Join(target.DOLocalRotate(baseState.Euler, duration).SetEase(swayEase));
            }
        }

        private struct PoseState
        {
            public PoseState(Vector3 position, Vector3 euler)
            {
                Position = position;
                Euler = euler;
            }

            public readonly Vector3 Position;
            public readonly Vector3 Euler;
        }
    }
}
