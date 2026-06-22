using System;
using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;
using DG.Tweening;
using UnityEngine;
using UnityEngine.Scripting;

namespace PlayableAdsShort
{
    [Preserve]
    public sealed class GameSequence : IGameSequence
    {
        private readonly GameConfig _config;
        private readonly StageView _stage;
        private readonly IViewFactory _factory;
        private readonly List<GameObject> _hintObjects = new List<GameObject>();
        private readonly List<GameObject> _actionPathObjects = new List<GameObject>();
        private readonly List<PathDotProgress> _actionPathDots = new List<PathDotProgress>();

        [Preserve]
        public GameSequence(GameConfig config, StageView stage, IViewFactory factory)
        {
            _config = config;
            _stage = stage;
            _factory = factory;
        }

        public async UniTask PlayChestAsync(ActorView hero, ChestView chest, GameState state, CancellationToken token)
        {
            ClearHint();
            _stage.Play(_config.clickClip, 0.75f);
            IReadOnlyList<Vector3> route = BuildRoute(hero.Position, chest.ApproachSpot);
            await PlayPathPreviewAsync(route, chest.HintPosition, chest.HintScale, token);
            try
            {
                _stage.PlayFootsteps(_config.footstepClip, _config.footstepVolume);
                await AwaitMovementTween(hero.MoveAlong(route, _config.moveSpeed), hero, route, token);
            }
            finally
            {
                _stage.StopFootsteps();
                ClearActionPath();
            }

            _stage.Play(_config.chestClip, 0.95f);
            chest.Open();
            await WaitForChestOpenAsync(chest, token);
            PlayBurst(_factory.CreateChestBurstVfx(), chest.Position);
            PlayBurst(_factory.CreateChestRewardFlightVfx(), chest.Position);
            await AwaitTween(chest.PlayRewardFlight(hero.RewardCatchPosition), token);
            state.OpenChest(chest.Reward);
            hero.EquipWeapon();
            PlayBurst(_factory.CreateRewardSparkVfx(), hero.RewardCatchPosition);
            hero.SetStrength(state.HeroStrength);
            hero.SetPoweredVisual(true);
            _stage.Play(_config.powerClip, 0.85f);
            await AwaitTween(hero.Punch(), token);
            await AwaitTween(chest.HideAfterUpgrade(), token);
        }

        public async UniTask PlayAttackAsync(ActorView hero, TargetView target, GameState state, CancellationToken token)
        {
            ClearHint();
            _stage.Play(_config.clickClip, 0.7f);
            IReadOnlyList<Vector3> route = BuildRoute(hero.Position, target.AttackSpot);
            await PlayPathPreviewAsync(route, target.HintPosition, target.HintScale, token);
            bool useEarlySuperAttack = IsSecondGoblinTarget(target);
            bool superAttackPlayed = false;
            Action earlySuperAttack = null;
            if (useEarlySuperAttack)
            {
                earlySuperAttack = () =>
                {
                    superAttackPlayed = true;
                    hero.PlaySuperAttack();
                };
            }

            try
            {
                _stage.PlayFootsteps(_config.footstepClip, _config.footstepVolume);
                await AwaitMovementTween(
                    hero.MoveAlong(route, _config.moveSpeed),
                    hero,
                    route,
                    token,
                    useEarlySuperAttack ? GetTwoPathDotsBeforeEndDistance(route) : -1f,
                    earlySuperAttack);
            }
            finally
            {
                _stage.StopFootsteps();
                ClearActionPath();
            }

            if (!superAttackPlayed && IsSecondGoblinTarget(target))
            {
                hero.PlaySuperAttack();
            }
            else if (!superAttackPlayed)
            {
                hero.PlayAttack();
            }

            hero.Face(target.Position);
            _stage.Play(target.Kind == TargetKind.WaterEnemy ? _config.powerClip : _config.hitClip, 0.63f);

            Tween weaponThrow = null;
            if (target.Kind == TargetKind.WaterEnemy)
            {
                await UniTask.Delay((int)(_config.attackImpactDelay * 1000f), cancellationToken: token);
                weaponThrow = hero.ThrowWeaponAt(target.ImpactPosition, _config.weaponProjectilePrefab, _stage.effectsLayer);
                await UniTask.Delay((int)(hero.weaponThrowOutDuration * 1000f), cancellationToken: token);
            }
            else
            {
                await UniTask.Delay((int)(_config.attackImpactDelay * 1000f), cancellationToken: token);
            }

            PlayBurst(_factory.CreateImpactVfx(target.Kind), target.ImpactPosition);
            if (target.Kind == TargetKind.WaterEnemy)
            {
                _stage.Play(_config.waterHitClip, 0.51f);
            }

            await AwaitTween(target.HitImpact(), token);
            target.SetDefeated();
            hero.SetSuperAttacking(false);
            PlayBurst(_factory.CreateDeathVfx(target), target.ImpactPosition);
            state.Defeat(target.Id, target.Reward);
            hero.SetStrength(state.HeroStrength);
            hero.SetPoweredVisual(state.HeroStrength >= 13);
            await AwaitTween(weaponThrow, token);
            await AwaitTween(hero.Punch(), token);
            await UniTask.Delay((int)(_config.attackRecoveryDuration * 1000f), cancellationToken: token);
        }

        public async UniTask PlayInvalidAsync(TargetView target, CancellationToken token, bool showMarker = true)
        {
            if (showMarker)
            {
                PlaySelectionPulse(target.HintPosition, target.HintScale);
            }

            _stage.Play(_config.invalidClip, 0.65f);
            await AwaitTween(target.PulseInvalid(_config.invalidShakeDuration), token);
        }

        public async UniTask PlayInvalidAsync(ChestView chest, CancellationToken token)
        {
            PlaySelectionPulse(chest.HintPosition, chest.HintScale);
            _stage.Play(_config.invalidClip, 0.65f);
            await AwaitTween(chest.PulseInvalid(_config.invalidShakeDuration), token);
        }

        public async UniTask PlayCtaAsync(CancellationToken token)
        {
            ClearHint();
            await UniTask.Delay((int)(_config.ctaDelay * 1000f), cancellationToken: token);
            CtaOverlayView overlay = _factory.CreateCtaOverlay();
            _stage.Play(_config.ctaClip, 0.85f);
            await AwaitTween(overlay.Show(), token);
        }

        public void ShowHint(ActorView hero, ChestView chest)
        {
            ShowHint(BuildRoute(hero.Position, chest.ApproachSpot), chest.HintPosition, chest.HintScale);
        }

        public void ShowHint(ActorView hero, TargetView target)
        {
            ShowHint(BuildRoute(hero.Position, target.AttackSpot), target.HintPosition, target.HintScale);
        }

        public void ClearHint()
        {
            foreach (GameObject hintObject in _hintObjects)
            {
                if (hintObject != null)
                {
                    Object.Destroy(hintObject);
                }
            }

            _hintObjects.Clear();
            ClearActionPath();
        }

        private void ShowHint(IReadOnlyList<Vector3> route, Vector3 ringPosition, Vector3 scale)
        {
            ClearHint();
            HintRingView ring = _factory.CreateHintRing();
            ring.Show(ringPosition, scale);
            _hintObjects.Add(ring.gameObject);
            CreatePath(route, persistent: true);
        }

        private async UniTask PlayPathPreviewAsync(IReadOnlyList<Vector3> route, Vector3 markerPosition, Vector3 markerScale, CancellationToken token)
        {
            ClearActionPath();
            ShowSelectionMarkerUntilArrival(markerPosition, markerScale);
            CreatePath(route, persistent: false);
            await UniTask.Delay(PlayableConstants.Motion.PathPreviewDelayMs, cancellationToken: token);
        }

        private void CreatePath(IReadOnlyList<Vector3> route, bool persistent)
        {
            float routeLength = GetRouteLength(route);
            for (int i = 1; i <= PlayableConstants.Motion.PathDotCount; i++)
            {
                float t = i / (float)PlayableConstants.Motion.PathDotCount;
                PathDotView dot = _factory.CreatePathDot();
                dot.Show(SampleRoute(route, t));
                dot.Pop(i * _config.pathDotDelay, keepVisible: true);
                if (persistent)
                {
                    _hintObjects.Add(dot.gameObject);
                }
                else
                {
                    _actionPathObjects.Add(dot.gameObject);
                    _actionPathDots.Add(new PathDotProgress(dot, routeLength * t));
                }
            }
        }

        private void PlaySelectionPulse(Vector3 position, Vector3 scale)
        {
            HintRingView ring = _factory.CreateHintRing();
            ring.Show(position, scale);
            ring.Pulse(_config.selectionPulseDuration, destroyOnComplete: true);
        }

        private void ShowSelectionMarkerUntilArrival(Vector3 position, Vector3 scale)
        {
            HintRingView ring = _factory.CreateHintRing();
            ring.Show(position, scale);
            ring.Pulse(_config.selectionPulseDuration, destroyOnComplete: false);
            _actionPathObjects.Add(ring.gameObject);
        }

        private void ClearActionPath()
        {
            foreach (GameObject pathObject in _actionPathObjects)
            {
                if (pathObject != null)
                {
                    Object.Destroy(pathObject);
                }
            }

            _actionPathObjects.Clear();
            _actionPathDots.Clear();
        }

        private IReadOnlyList<Vector3> BuildRoute(Vector3 from, Vector3 desiredDestination)
        {
            var route = new List<Vector3> { from };
            if (_stage.navigation == null)
            {
                route.Add(desiredDestination);
                return route;
            }

            IReadOnlyList<Vector3> navigationRoute = _stage.navigation.BuildRoute(from, desiredDestination);
            for (int i = 0; i < navigationRoute.Count; i++)
            {
                if ((navigationRoute[i] - route[route.Count - 1]).sqrMagnitude > 0.0001f)
                {
                    route.Add(navigationRoute[i]);
                }
            }

            return route;
        }

        private static Vector3 SampleRoute(IReadOnlyList<Vector3> route, float normalizedDistance)
        {
            if (route.Count == 0)
            {
                return Vector3.zero;
            }

            float totalLength = 0f;
            for (int i = 1; i < route.Count; i++)
            {
                totalLength += Vector3.Distance(route[i - 1], route[i]);
            }

            float targetDistance = totalLength * normalizedDistance;
            float travelled = 0f;
            for (int i = 1; i < route.Count; i++)
            {
                float segmentLength = Vector3.Distance(route[i - 1], route[i]);
                if (travelled + segmentLength >= targetDistance)
                {
                    float segmentT = segmentLength <= 0.0001f ? 0f : (targetDistance - travelled) / segmentLength;
                    return Vector3.Lerp(route[i - 1], route[i], segmentT);
                }

                travelled += segmentLength;
            }

            return route[route.Count - 1];
        }

        private static float GetRouteLength(IReadOnlyList<Vector3> route)
        {
            float totalLength = 0f;
            for (int i = 1; i < route.Count; i++)
            {
                totalLength += Vector3.Distance(route[i - 1], route[i]);
            }

            return totalLength;
        }

        private static float GetRouteProgressDistance(IReadOnlyList<Vector3> route, Vector3 position)
        {
            float bestDistance = 0f;
            float bestSqrDistance = float.PositiveInfinity;
            float travelled = 0f;

            for (int i = 1; i < route.Count; i++)
            {
                Vector3 start = route[i - 1];
                Vector3 end = route[i];
                Vector3 segment = end - start;
                float segmentLength = segment.magnitude;
                if (segmentLength <= 0.0001f)
                {
                    continue;
                }

                float t = Mathf.Clamp01(Vector3.Dot(position - start, segment) / (segmentLength * segmentLength));
                Vector3 projected = Vector3.Lerp(start, end, t);
                float sqrDistance = (position - projected).sqrMagnitude;
                if (sqrDistance < bestSqrDistance)
                {
                    bestSqrDistance = sqrDistance;
                    bestDistance = travelled + segmentLength * t;
                }

                travelled += segmentLength;
            }

            return bestDistance;
        }

        private void HidePassedActionPathDots(float progressDistance)
        {
            for (int i = 0; i < _actionPathDots.Count; i++)
            {
                PathDotProgress dot = _actionPathDots[i];
                if (dot.Hidden || progressDistance < dot.Distance)
                {
                    continue;
                }

                dot.Hidden = true;
                if (dot.View != null)
                {
                    dot.View.Hide();
                }
            }
        }

        private static void PlayBurst(VfxBurstView burst, Vector3 position)
        {
            if (burst == null)
            {
                return;
            }

            burst.Show(position);
            burst.Play(PlayableConstants.Effects.BurstDuration);
        }

        private static bool IsSecondGoblinTarget(TargetView target)
        {
            return target != null && target.Id == PlayableConstants.Ids.LeftEnemy;
        }

        private async UniTask WaitForChestOpenAsync(ChestView chest, CancellationToken token)
        {
            Animator animator = chest.animator;
            if (animator == null)
            {
                await UniTask.Delay((int)(_config.chestImpactDelay * 1000f), cancellationToken: token);
                return;
            }

            await UniTask.Yield(PlayerLoopTiming.Update, token);
            await WaitUntilAnimatorStateAsync(animator, PlayableConstants.Animation.OpenStateHash, token);
            await WaitUntilAnimatorStateTimeAsync(animator, PlayableConstants.Animation.OpenStateHash, _config.chestRewardReleaseNormalizedTime, token);
        }

        private static async UniTask WaitUntilAnimatorStateAsync(Animator animator, int stateHash, CancellationToken token)
        {
            float elapsed = 0f;
            while (elapsed < PlayableConstants.Animation.AnimatorWaitTimeout)
            {
                token.ThrowIfCancellationRequested();
                AnimatorStateInfo current = animator.GetCurrentAnimatorStateInfo(PlayableConstants.Animation.DefaultLayer);
                AnimatorStateInfo next = animator.GetNextAnimatorStateInfo(PlayableConstants.Animation.DefaultLayer);
                if (current.shortNameHash == stateHash || next.shortNameHash == stateHash)
                {
                    return;
                }

                elapsed += Time.deltaTime;
                await UniTask.Yield(PlayerLoopTiming.Update, token);
            }
        }

        private static async UniTask WaitUntilAnimatorStateTimeAsync(Animator animator, int stateHash, float normalizedTime, CancellationToken token)
        {
            normalizedTime = Mathf.Clamp01(normalizedTime);
            float elapsed = 0f;
            while (elapsed < PlayableConstants.Animation.AnimatorWaitTimeout)
            {
                token.ThrowIfCancellationRequested();
                AnimatorStateInfo current = animator.GetCurrentAnimatorStateInfo(PlayableConstants.Animation.DefaultLayer);
                if (current.shortNameHash == stateHash
                    && !animator.IsInTransition(PlayableConstants.Animation.DefaultLayer)
                    && current.normalizedTime >= normalizedTime)
                {
                    return;
                }

                elapsed += Time.deltaTime;
                await UniTask.Yield(PlayerLoopTiming.Update, token);
            }
        }

        private static async UniTask AwaitTween(Tween tween, CancellationToken token)
        {
            if (tween == null)
            {
                return;
            }

            while (tween.IsActive() && tween.IsPlaying())
            {
                token.ThrowIfCancellationRequested();
                await UniTask.Yield(PlayerLoopTiming.Update, token);
            }
        }

        private async UniTask AwaitMovementTween(
            Tween tween,
            ActorView hero,
            IReadOnlyList<Vector3> route,
            CancellationToken token,
            float triggerDistance = -1f,
            Action triggerAction = null)
        {
            if (tween == null)
            {
                return;
            }

            bool triggered = false;
            while (tween.IsActive() && tween.IsPlaying())
            {
                token.ThrowIfCancellationRequested();
                float progressDistance = GetRouteProgressDistance(route, hero.Position);
                HidePassedActionPathDots(progressDistance);
                if (!triggered && triggerAction != null && progressDistance >= triggerDistance)
                {
                    triggered = true;
                    triggerAction.Invoke();
                }

                await UniTask.Yield(PlayerLoopTiming.Update, token);
            }

            if (!triggered && triggerAction != null)
            {
                triggerAction.Invoke();
            }

            HidePassedActionPathDots(float.PositiveInfinity);
        }

        private static float GetTwoPathDotsBeforeEndDistance(IReadOnlyList<Vector3> route)
        {
            float routeLength = GetRouteLength(route);
            float triggerDotIndex = Mathf.Max(0f, PlayableConstants.Motion.PathDotCount - 2f);
            return routeLength * (triggerDotIndex / PlayableConstants.Motion.PathDotCount);
        }

        private sealed class PathDotProgress
        {
            public readonly PathDotView View;
            public readonly float Distance;
            public bool Hidden;

            public PathDotProgress(PathDotView view, float distance)
            {
                View = view;
                Distance = distance;
            }
        }
    }
}
